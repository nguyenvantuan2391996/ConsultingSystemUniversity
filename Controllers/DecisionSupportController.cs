using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsultingSystemUniversity.Data;
using ConsultingSystemUniversity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultingSystemUniversity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DecisionSupportController : ControllerBase
    {
        private readonly ConsultingSystemUniversityContext _context;

        public DecisionSupportController(ConsultingSystemUniversityContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> DSS([FromBody] InputData inputData)
        {
            if (inputData.location == "all")
            {
                inputData.location = "";
            }

            var dataDss = await _context.Universites
                .Join(
                _context.Majors,
                u => u.university_code,
                m => m.university_code,
                (u, m) => new
                {
                    universityName = u.university_name,
                    location = u.location,
                    linkWebsite = u.link_website,
                    rank = u.rank,
                    majorId = m.id,
                    majorsGroupId = m.majors_group_id,
                    majorsName = m.majors_name,
                    benchmarks2019 = m.benchmarks_2019,
                    benchmarks2020 = m.benchmarks_2020,
                    examGroup = m.exam_group,
                    c = m.c
                })
                .Join(
                _context.MajorsGroups,
                mu => mu.majorsGroupId,
                mg => mg.majors_group_id,
                (mu, mg) => new
                {
                    universityName = mu.universityName,
                    location = mu.location,
                    linkWebsite = mu.linkWebsite,
                    rank = mu.rank,
                    majorId = mu.majorId,
                    majorsGroupId = mu.majorsGroupId,
                    majorsName = mu.majorsName,
                    benchmarks2019 = mu.benchmarks2019,
                    benchmarks2020 = mu.benchmarks2020,
                    examGroup = mu.examGroup,
                    c = mu.c,
                    majorsGroupName = mg.majors_group_name
                })
                .Where(major => major.majorsGroupId == inputData.majorGroupId && major.examGroup == inputData.examGroup && major.location.Contains(inputData.location) && (major.benchmarks2019 <= inputData.score || major.benchmarks2020 <= inputData.score))
                .ToListAsync();

            if (dataDss == null)
            {
                return NotFound(new { message = "Hệ thống chưa tìm được thông tin phù hợp" });
            }

            // Xử lý tư vấn
            float[,] data = new float[dataDss.Count, 3];

            // Data tư vấn
            int i = 0;
            List<Majors> listMajors = new List<Majors>();
            foreach (var item in dataDss)
            {
                data[i, 0] = (float)item.benchmarks2019;
                data[i, 1] = (float)item.benchmarks2020;
                data[i, 2] = (float)item.rank;

                Majors major = new Majors();
                major.id = item.majorId;
                listMajors.Add(major);

                i++;
            }

            // Chuẩn hóa miền [0;1]
            data = normalizeMatrix(data);

            // Ma trận trọng số
            data = weightedMatrix(data);

            // Tính các giải pháp lý tưởng
            float[,] A = new float[2, 3];
            A = idealSolution(data);

            // Tính khoảng cách tới giải pháp lý tưởng
            float[,] S = new float[2, data.GetLength(0)];
            Topsis ts = new Topsis();
            ts.data = data;
            ts.A = A;
            S = distanceIdealSolution(ts);

            // Tính độ tương tự tới giải pháp lý tưởng
            float[] C = new float[S.GetLength(1)];
            C = calculateC(S);

            // Update C trong bảng major
            for (int j = 0; j < C.Length; j++)
            {
                listMajors[j].c = C[j];
            }
            updateC(listMajors);

            // Trả về danh sách kết quả tư vấn
            var ResultDSS = await _context.Universites
                .Join(
                _context.Majors,
                u => u.university_code,
                m => m.university_code,
                (u, m) => new
                {
                    universityName = u.university_name,
                    location = u.location,
                    linkWebsite = u.link_website,
                    rank = u.rank,
                    majorId = m.id,
                    majorsGroupId = m.majors_group_id,
                    majorsName = m.majors_name,
                    benchmarks2019 = m.benchmarks_2019,
                    benchmarks2020 = m.benchmarks_2020,
                    examGroup = m.exam_group,
                    c = m.c
                })
                .Join(
                _context.MajorsGroups,
                mu => mu.majorsGroupId,
                mg => mg.majors_group_id,
                (mu, mg) => new
                {
                    universityName = mu.universityName,
                    location = mu.location,
                    linkWebsite = mu.linkWebsite,
                    rank = mu.rank,
                    majorId = mu.majorId,
                    majorsGroupId = mu.majorsGroupId,
                    majorsName = mu.majorsName,
                    benchmarks2019 = mu.benchmarks2019,
                    benchmarks2020 = mu.benchmarks2020,
                    examGroup = mu.examGroup,
                    c = mu.c,
                    majorsGroupName = mg.majors_group_name
                })
                .Where(major => major.majorsGroupId == inputData.majorGroupId && major.examGroup == inputData.examGroup && major.location.Contains(inputData.location) && (major.benchmarks2019 <= inputData.score || major.benchmarks2020 <= inputData.score))
                .OrderByDescending(major => major.c)
                .Take(10)
                .ToListAsync();

            return Ok(ResultDSS);
        }

        // Update C trong bảng major
        public Boolean updateC(List<Majors> listMajors)
        {
            for (int i = 0; i < listMajors.Count; i++)
            {
                var major = _context.Majors.Find(listMajors[i].id);
                major.c = listMajors[i].c;

                _context.SaveChanges();
            }
            return true;
        }

        // Chuẩn hóa miền [0,1]
        public float[,] normalizeMatrix(float[,] data)
        {
            float[,] dataStandardize = new float[data.GetLength(0), data.GetLength(1)];

            // Tính tổng bình phương các cột
            float[] sumPower = new float[data.GetLength(1)];
            for (int i = 0; i < sumPower.Length; i++)
            {
                sumPower[i] = 0f;
            }

            for (int i = 0; i < data.GetLength(1); i++)
            {
                for (int j = 0; j < data.GetLength(0); j++)
                {
                    sumPower[i] += (float)Math.Pow(data[j, i], 2);
                }
                sumPower[i] = (float)Math.Sqrt(sumPower[i]);
            }

            for (int i = 0; i < data.GetLength(1); i++)
            {
                for (int j = 0; j < data.GetLength(0); j++)
                {
                    dataStandardize[j, i] = (float)(data[j, i] / sumPower[i]);
                }
            }

            return dataStandardize;
        }

        // Tính ma trận theo trọng số
        public float[,] weightedMatrix(float[,] dataStandardize)
        {
            float[] listTrongSo = new float[dataStandardize.GetLength(1)];
            listTrongSo[0] = 0.35f;
            listTrongSo[1] = 0.45f;
            listTrongSo[2] = 0.2f;

            float[,] dataWeightedMatrix = new float[dataStandardize.GetLength(0), dataStandardize.GetLength(1)];
            for (int i = 0; i < dataStandardize.GetLength(1); i++)
            {
                for (int j = 0; j < dataStandardize.GetLength(0); j++)
                {
                    dataWeightedMatrix[j, i] = (float)(dataStandardize[j, i] * listTrongSo[i]);
                }
            }

            return dataWeightedMatrix;
        }

        //Tính các giải pháp lý tưởng
        public float[,] idealSolution(float[,] dataWeightedMatrix)
        {
            float[,] A = new float[2, dataWeightedMatrix.GetLength(1)];

            for (int i = 0; i < dataWeightedMatrix.GetLength(1); i++)
            {

                float max = dataWeightedMatrix[0, i];
                float min = dataWeightedMatrix[0, i];

                for (int j = 0; j < dataWeightedMatrix.GetLength(0); j++)
                {

                    if (dataWeightedMatrix[j, i] > max)
                    {
                        max = dataWeightedMatrix[j, i];
                    }

                    if (dataWeightedMatrix[j, i] < min)
                    {
                        min = dataWeightedMatrix[j, i];
                    }

                    A[0, i] = max;
                    A[1, i] = min;
                }
            }

            return A;
        }

        //Tính khoảng cách tới giải pháp lý tưởng
        public float[,] distanceIdealSolution(Topsis dataTopsis)
        {
            float[,] S = new float[2, dataTopsis.data.GetLength(0)];
            for (int i = 0; i < S.GetLength(0); i++)
            {
                for (int j = 0; j < S.GetLength(1); j++)
                {
                    S[i, j] = 0f;
                }
            }

            for (int i = 0; i < dataTopsis.data.GetLength(0); i++)
            {
                for (int j = 0; j < dataTopsis.data.GetLength(1); j++)
                {
                    S[0, i] += (float)Math.Pow(dataTopsis.data[i, j] - dataTopsis.A[0, j], 2);
                    S[1, i] += (float)Math.Pow(dataTopsis.data[i, j] - dataTopsis.A[1, j], 2);
                }
                S[0, i] = (float)Math.Sqrt(S[0, i]);
                S[1, i] = (float)Math.Sqrt(S[1, i]);
            }

            return S;
        }

        //Tính độ tương tự tới giải pháp lý tưởng
        public float[] calculateC(float[,] S)
        {
            float[] C = new float[S.GetLength(1)];

            for (int i = 0; i < S.GetLength(1); i++)
            {
                C[i] = S[1, i] / (S[0, i] + S[1, i]);
            }

            return C;
        }
    }
}
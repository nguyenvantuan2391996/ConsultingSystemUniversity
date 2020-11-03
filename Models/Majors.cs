using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Models
{
    public class Majors
    {
        [Key]
        public int id { get; set; }
        public int university_code { get; set; }
        public int majors_group_id { get; set; }
        public string majors_code { get; set; }
        public string majors_name { get; set; }
        public double benchmarks_2019 { get; set; }
        public double benchmarks_2020 { get; set; }
        public string exam_group { get; set; }
        public int targets { get; set; }
        public double c { get; set; }
    }
}

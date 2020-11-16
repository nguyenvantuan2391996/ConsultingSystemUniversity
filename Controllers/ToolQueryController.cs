using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsultingSystemUniversity.Data;
using ConsultingSystemUniversity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultingSystemUniversity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ToolQueryController : ControllerBase
    {
        private readonly ConsultingSystemUniversityContext _context;

        public ToolQueryController(ConsultingSystemUniversityContext context)
        {
            _context = context;
        }

        [HttpPost]
        [EnableCors("CorPolicy")]
        public async Task<ActionResult> ReturnResultSql([FromBody] Query querySql)
        {
            if (querySql.querySql == null || querySql.querySql == "")
            {
                return BadRequest(new { message = "The string argument 'sql' cannot be empty." });
            }

            try
            {
                if (querySql.querySql.ToUpper().Contains("SELECT"))
                {
                    var results = (Object)null;

                    if (querySql.querySql.ToUpper().Contains("ACCOUNT"))
                    {
                        results = await _context.Accounts.FromSql(querySql.querySql).ToListAsync();
                    }

                    if (querySql.querySql.ToUpper().Contains("TOKEN"))
                    {
                        results = await _context.JwtTokens.FromSql(querySql.querySql).ToListAsync();
                    }

                    if (querySql.querySql.ToUpper().Contains("LANGUAGE"))
                    {
                        results = await _context.Languages.FromSql(querySql.querySql).ToListAsync();
                    }

                    if (querySql.querySql.ToUpper().Contains("UNIVERSITY"))
                    {
                        results = await _context.Universites.FromSql(querySql.querySql).ToListAsync();
                    }

                    if (querySql.querySql.ToUpper().Contains("MAJORS_GROUP"))
                    {
                        results = await _context.MajorsGroups.FromSql(querySql.querySql).ToListAsync();
                    }

                    if (querySql.querySql.ToUpper().Contains("MAJORS"))
                    {
                        results = await _context.Majors.FromSql(querySql.querySql).ToListAsync();
                    }

                    if (querySql.querySql.ToUpper().Contains("FEEDBACK"))
                    {
                        results = await _context.Feedbacks.FromSql(querySql.querySql).ToListAsync();
                    }

                    return Ok(results);
                }
                else
                {
                    var results = await _context.Database.ExecuteSqlCommandAsync(querySql.querySql);
                    return Ok(new { message = "The command(s) completed successfully." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
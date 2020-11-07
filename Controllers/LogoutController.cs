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

namespace ConsultingSystemUniversity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogoutController : ControllerBase
    {
        private readonly ConsultingSystemUniversityContext _context;

        public LogoutController(ConsultingSystemUniversityContext context)
        {
            _context = context;
        }

        [HttpPost]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> singout(JwtToken jwtToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Delete token and refresh token
            var jwt = _context.JwtTokens.Where(t => t.token == jwtToken.token && t.refresh_token == jwtToken.refresh_token).FirstOrDefault();
            if (jwt == null)
            {
                return NotFound(new { message = "Not found token to delete" });
            }

            _context.JwtTokens.Remove(jwt);
            await _context.SaveChangesAsync();

            // Making token expried

            return Ok(new { message = "Singout success" });
        }
    }
}
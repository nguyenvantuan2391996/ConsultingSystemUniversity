using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ConsultingSystemUniversity.Data;
using ConsultingSystemUniversity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ConsultingSystemUniversity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ConsultingSystemUniversityContext _context;
        private readonly IConfiguration _config;

        public LoginController(ConsultingSystemUniversityContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [HttpPost]
        public IActionResult Login([FromBody]Account acc)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var account = AuthenticateUser(acc.account_name, ComputeSha256Hash(acc.password));

            if (account != null)
            {
                var jwtToken = GenaretaJSONWebToken(account);

                // add token
                addToken(jwtToken);

                return Ok(new
                {
                    access_token = jwtToken.token,
                    refresh_token = jwtToken.refresh_token,
                    name = account.name,
                    phone = account.phone,
                    address = account.address,
                    status = account.status,
                    role = account.role,
                    languageId = account.language_id
                });
            }

            return BadRequest(new { message = "Account or password uncorrect" });
        }

        public Account AuthenticateUser(string accountName, string password)
        {
            try
            {
                var acc = _context.Accounts.Where(a => a.account_name == accountName && a.password == password).FirstOrDefault();

                return acc;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // GenaretaRefreshToken
        private string GenaretaRefreshToken()
        {
            var refToken = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(refToken);

                return Convert.ToBase64String(refToken);
            }
        }

        private JwtToken GenaretaJSONWebToken(Account account)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //var claims = new[]
            //{
            //    new Claim(JwtRegisteredClaimNames.Sub, "name", account.name),
            //    new Claim(JwtRegisteredClaimNames.Sub, "phone", account.phone),
            //    new Claim(JwtRegisteredClaimNames.Sub, "address", account.address),
            //    new Claim(JwtRegisteredClaimNames.Sub, "status", account.status),
            //};

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:LifeTimeToken"])),
                signingCredentials: credentials
                );

            var encodeToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenaretaRefreshToken();

            JwtToken jwtToken = new JwtToken();
            jwtToken.token = encodeToken;
            jwtToken.refresh_token = refreshToken;
            jwtToken.expiry = DateTime.Now.AddDays(Convert.ToDouble(_config["Jwt:LifeTimeRefreshToken"]));

            return jwtToken;
        }

        [HttpPost("refreshToken")]
        public ActionResult refreshToken([FromBody]JwtToken jwtToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (jwtToken == null)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            var oldJwtToken = _context.JwtTokens.Where(jwt => jwt.token == jwtToken.token && jwt.refresh_token == jwtToken.refresh_token).FirstOrDefault();

            if (oldJwtToken == null)
            {
                return BadRequest(new { message = "Invalid token" });
            }

            if (oldJwtToken.expiry < DateTime.Now)
            {
                return BadRequest(new { message = "Refresh token expiried" });
            }

            // remove old token
            removeToken(oldJwtToken);

            // generate new token and new refresh token
            Account account = new Account();
            var newJwtToken = GenaretaJSONWebToken(account);

            // add new token
            addToken(newJwtToken);

            return Ok(new
            {
                access_token = newJwtToken.token,
                refresh_token = newJwtToken.refresh_token,
                expiry = newJwtToken.expiry
            });
        }

        private Boolean addToken(JwtToken jwtToken)
        {
            if (jwtToken == null)
            {
                return false;
            }

            _context.JwtTokens.Add(jwtToken);
            _context.SaveChanges();
            return true;
        }

        private Boolean removeToken(JwtToken jwtToken)
        {
            if (jwtToken == null)
            {
                return false;
            }

            _context.JwtTokens.Remove(jwtToken);
            _context.SaveChanges();
            return true;
        }
    }
}
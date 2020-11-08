using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConsultingSystemUniversity.Data;
using ConsultingSystemUniversity.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace ConsultingSystemUniversity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly ConsultingSystemUniversityContext _context;

        public AccountController(ConsultingSystemUniversityContext context)
        {
            _context = context;
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

        // GET: api/Account
        [HttpPost("getaccouts")]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> GetAccounts([FromBody] Paging paging)
        {
            if (paging.accounts == null)
            {
                paging.accounts = new Account();
                paging.accounts.account_name = "";
            }

            if (paging.limit == 0 || paging.offset == 0)
            {
                paging.limit = 10;
                paging.offset = 0;
            }

            var listAccount = await _context.Accounts
                .Join(
                _context.Languages,
                acc => acc.language_id,
                lang => lang.language_id,
                (acc, lang) => new
                {
                    id = acc.id,
                    accountName = acc.account_name,
                    name = acc.name,
                    phone = acc.phone,
                    address = acc.address,
                    status = acc.status,
                    languageId = lang.language_id,
                    languageName = lang.language_name
                }
                )
                .Where(acc => acc.accountName.Contains(paging.accounts.account_name))
                .OrderBy(acc => acc.id)
                .Skip(paging.offset)
                .Take(paging.limit)
                .ToListAsync();

            if (listAccount.Count == 0)
            {
                return NotFound(new { message = "Not Found" });
            }

            return Ok(listAccount);
        }

        [HttpPost("getbyid")]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> GetAccount([FromBody] Account acc)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var account = await _context.Accounts.FindAsync(acc.id);

            if (account == null)
            {
                return NotFound();
            }

            return Ok(account);
        }

        [HttpPut("{id}")]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> PutAccount([FromRoute] int id, [FromBody] Account account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != account.id)
            {
                return BadRequest();
            }

            _context.Entry(account).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Accounts
        [HttpPost]
        [EnableCors("CorPolicy")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAccount([FromBody] Account account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var acc = await _context.Accounts.Where(a => a.account_name == account.account_name).FirstOrDefaultAsync();
            if (acc == null)
            {
                account.password = this.ComputeSha256Hash(account.password);
                account.role = "0";

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                return Ok(account.id);
            }
            else
            {
                return BadRequest(new { message = "Account name is available" });
            }
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> DeleteAccount([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound(new { message = "Not found account to delete" });
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return Ok(account);
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.id == id);
        }
    }
}
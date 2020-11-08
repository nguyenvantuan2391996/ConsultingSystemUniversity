using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsultingSystemUniversity.Data;
using ConsultingSystemUniversity.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsultingSystemUniversity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly ConsultingSystemUniversityContext _context;

        public FeedbackController(ConsultingSystemUniversityContext context)
        {
            _context = context;
        }

        // Get feedback
        [HttpGet]
        [EnableCors("CorPolicy")]
        public IActionResult GetFeedback()
        {
            var feedback = _context.Feedbacks
                .Join(
                    _context.Accounts,
                    feed => feed.account_id,
                    acc => acc.id,
                    (feed, acc) => new
                    {
                        id = feed.id,
                        feedback_content = feed.feedback_content,
                        feedback_type = feed.feedback_type,
                        account_name = acc.account_name,
                        name = acc.name,
                        address = acc.address,
                        phone = acc.phone
                    }
                ).ToList();

            if (feedback.Count == 0)
            {
                return NotFound(new { message = "Not found feedback" });
            }

            return Ok(feedback);
        }

        // Add feedback
        [HttpPost]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> addFeedback([FromBody] Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var acc = await _context.Accounts.FindAsync(feedback.account_id);
            if (acc == null)
            {
                return BadRequest(new { message = "Send to feedback fail" });
            }

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Send to feedback success " });
        }

        // Delete feedback
        [HttpDelete]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> DeleteFeedback([FromBody] Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var feed = await _context.Feedbacks.FindAsync(feedback.id);

            if (feed == null)
            {
                return NotFound(new { message = "Not found feedback to delete " });
            }
            else
            {
                _context.Remove(feed);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Delete success " });
            }
        }
    }
}
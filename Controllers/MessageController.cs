using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsultingSystemUniversity.Data;
using ConsultingSystemUniversity.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultingSystemUniversity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ConsultingSystemUniversityContext _context;

        public MessageController(ConsultingSystemUniversityContext context)
        {
            _context = context;
        }

        [HttpPost("getmessage")]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> GetMessage([FromBody] SenderReceive _senderReceive)
        {
            try
            {
                var listMessage = await _context.Messages
                .Join(
                _context.SenderReceives,
                mess => mess.message_id,
                sr => sr.message_id,
                (mess, sr) => new
                {
                    message_id = mess.message_id,
                    message_content = mess.message_content,
                    message_time = mess.message_time,
                    is_read = mess.is_read,
                    sender_id = sr.sender_id,
                    receiver_id = sr.receive_id
                })
                .Where(srs => srs.sender_id == _senderReceive.sender_id && srs.receiver_id == _senderReceive.receive_id || ( srs.sender_id == _senderReceive.receive_id && srs.receiver_id == _senderReceive.sender_id))
                .OrderBy(mes => mes.message_id)
                .ToListAsync();

                return Ok(listMessage);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("addmessage")]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> addMessage([FromBody] DataChat _data)
        {
            if (_data.senderReceive == null || _data == null || _data.messageChat.message_content == null || _data.messageChat.message_content == "")
            {
                return BadRequest();
            }

            try
            {
                // add table message
                _context.Messages.Add(_data.messageChat);
                await _context.SaveChangesAsync();

                //add table sender_receive
                _data.senderReceive.message_id = _data.messageChat.message_id;
                _context.SenderReceives.Add(_data.senderReceive);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Success"});
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            
        }
    }
}
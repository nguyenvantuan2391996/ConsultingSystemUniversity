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

        [HttpPut("updatemessagenoread")]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> UpdateMessageNoRead(SenderReceive _senderReceive)
        {
            if (_senderReceive == null)
            {
                return BadRequest(new { message = "Request is invalid" });
            }

            try
            {
                _context.Messages
                .Join(
                _context.SenderReceives,
                mess => mess.message_id,
                sr => sr.message_id,
                (mess, sr) => new
                {
                    mess,
                    sr
                }
                ).Where(mes => mes.sr.sender_id == _senderReceive.receive_id && mes.sr.receive_id == _senderReceive.sender_id)
                .ToList()
                .ForEach(mes => mes.mess.is_read = "1");
                await _context.SaveChangesAsync();

                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("countmessagenoread")]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> CountMessageNoRead([FromBody] ListIdAccount _list)
        {
            if (_list == null)
            {
                return BadRequest(new { message = "Request is invalid" });
            }

            List<int> listCountMessageNoRead = new List<int>();

            foreach (int receiverId in _list.listId)
            {
                var CountMessageNoRead = await _context.Messages
                .Join(
                _context.SenderReceives,
                mess => mess.message_id,
                sr => sr.message_id,
                (mess, sr) => new
                {
                    mess,
                    sr
                }
                )
                .Where(mes => mes.mess.is_read == "0" && mes.sr.sender_id == receiverId && mes.sr.receive_id == _list.senderId)
                .CountAsync();

                listCountMessageNoRead.Add(CountMessageNoRead);
            }

            return Ok(listCountMessageNoRead);
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
                .Where(srs => srs.sender_id == _senderReceive.sender_id && srs.receiver_id == _senderReceive.receive_id || (srs.sender_id == _senderReceive.receive_id && srs.receiver_id == _senderReceive.sender_id))
                .OrderBy(mes => mes.message_id)
                .ToListAsync();

                return Ok(listMessage);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("addfirstmessage")]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> addFirstMessage([FromBody] DataChat _data)
        {
            if (_data.senderReceive == null || _data == null || _data.messageChat.message_content == null || _data.messageChat.message_content == "")
            {
                return BadRequest(new { message = "Message content is not empty" });
            }

            try
            {
                // Receiver will auto reply if this is first message
                var checkFirstMessage = await _context.SenderReceives.Where(sr => sr.sender_id == _data.senderReceive.sender_id && sr.receive_id == _data.senderReceive.receive_id).FirstOrDefaultAsync();
                if (checkFirstMessage == null)
                {
                    // add table message
                    _context.Messages.Add(_data.messageChat);
                    await _context.SaveChangesAsync();

                    //add table sender_receive
                    _data.senderReceive.message_id = _data.messageChat.message_id;
                    _context.SenderReceives.Add(_data.senderReceive);
                    await _context.SaveChangesAsync();
                }
                return Ok(new { message = "Success" });
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
                return BadRequest(new { message = "Message content is not empty" });
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

                return Ok(new { message = "Success" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("getreceiver")]
        [EnableCors("CorPolicy")]
        public async Task<IActionResult> getListReceiver([FromBody] SenderReceive senderReceive)
        {
            if (senderReceive == null)
            {
                return BadRequest(new { message = "Request is invalid" });
            }

            try
            {
                var listId = await _context.SenderReceives
                    .Where(send => send.sender_id == senderReceive.sender_id)
                    .OrderByDescending(send => send.id)
                    .Select(send => send.receive_id).ToListAsync();

                return Ok(listId);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
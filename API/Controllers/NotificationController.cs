using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationControllers : ControllerBase
    {
        private readonly RabbitMqService _mq;
        private readonly IChatService _chat;

        public NotificationControllers(RabbitMqService mq, IChatService chat)
        {
            _mq = mq;
            _chat = chat;
        }
        [HttpGet]
        public IActionResult GetMessages()
        {
            var msgs = new List<string>();
            string? msg;
            while ((msg = _mq.ReceiveOneForUser("admin")) != null)
            {
                msgs.Add("[RabbitMQ] " + msg);
            }

            // Also get from Redis queue
            string? redisMsg;
            while ((redisMsg = _chat.ReceiveOneForUser("admin")) != null)
            {
                msgs.Add("[Redis] " + redisMsg);
            }

            return Ok(new { success = true, messages = msgs });
        }
    }
}
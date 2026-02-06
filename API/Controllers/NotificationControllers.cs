using System;
using System.Collections.Generic;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Repository.service;


namespace API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationControllers : ControllerBase
    {
        private readonly RabbitMqService _mq;
        private readonly IChatService _chat;
        private readonly IRestaurantProfileMenu _service;

        public NotificationControllers(RabbitMqService mq, IChatService chat, IRestaurantProfileMenu service)
        {
            _mq = mq;
            _chat = chat;
            _service = service;
        }

        // 🔹 Get unread + queued messages
        [HttpGet]
        public IActionResult GetMessages()
        {
            var msgs = new List<string>();
            string? msg;

            while ((msg = _mq.ReceiveOneForUser("admin")) != null)
            {
                msgs.Add("[RabbitMQ] " + msg);
            }

            while ((msg = _chat.ReceiveOneForUser("admin")) != null)
            {
                msgs.Add("[Redis] " + msg);
            }

            return Ok(new { success = true, messages = msgs });
        }

        // 🔹 Get full history
        [HttpGet("history")]
        public IActionResult GetAllNotificationHistory()
        {
            var history = _chat.GetAllNotificationHistory("admin");
            return Ok(new { success = true, messages = history });
        }


        //conque messages for restaurant by customer id
        [HttpGet("restaurant/history/by-customer/{customerId}")]
        public async Task<IActionResult> GetRestaurantNotificationHistoryByCustomer(int customerId)
        {
            int restaurantId = await _service.GetRestaurantIdByCustomer(customerId);
            if (restaurantId == 0)
                return Ok(new { success = true, messages = Array.Empty<string>() });

            // 👇 READ FROM conv
            var history = _chat.GetHistory("restaurant_" + restaurantId, "system", 1000);

            return Ok(new { success = true, messages = history });
        }



        // 🔥 DELETE single notification
        [HttpDelete("delete")]
        public IActionResult DeleteNotification([FromBody] DeleteNotificationRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Message))
            {
                return BadRequest(new { success = false, message = "Message is required" });
            }

            var deleted = _chat.DeleteNotification("admin", req.Message);
            if (!deleted)
            {
                return NotFound(new { success = false, message = "Notification not found" });
            }

            return Ok(new { success = true });
        }

        //conque messages for restaurant by customer id
        [HttpDelete("restaurant/delete/by-customer/{customerId}")]
        public async Task<IActionResult> DeleteRestaurantNotification(
        int customerId,
        [FromBody] DeleteNotificationRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest();

            int restaurantId = await _service.GetRestaurantIdByCustomer(customerId);
            if (restaurantId == 0)
                return NotFound();

            string restaurantKey = $"restaurant_{restaurantId}";

            // 🔥 DELETE FROM conv
            var deleted = _chat.DeleteFromConversation(
                restaurantKey,
                "system",
                req.Message
            );

            return Ok(new { success = deleted });
        }



        [HttpGet("customer/history/{customerId}")]
        public IActionResult GetCustomerNotificationHistory(int customerId)
        {
            string customerKey = $"customer_{customerId}";

            // 🔥 READ FROM conv
            var history = _chat.GetHistory(customerKey, "system", 1000);

            return Ok(new { success = true, messages = history });
        }



        //conque messages for restaurant by customer id
        [HttpDelete("customer/delete/{customerId}")]
        public IActionResult DeleteCustomerNotification(
            int customerId,
            [FromBody] DeleteNotificationRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest();

            string customerKey = $"customer_{customerId}";

            // 🔥 DELETE FROM conv
            var deleted = _chat.DeleteFromConversation(
                customerKey,
                "system",
                req.Message
            );

            return Ok(new { success = deleted });
        }


        // // 🔔 Restaurant notifications
        //   // ================= RESTAURANT =================
        // [HttpGet("restaurant/{restaurantId}")]
        // public IActionResult GetRestaurantNotifications(int restaurantId)
        // {
        //     var messages = new List<string>();
        //     string? msg;
        //     string key = $"restaurant_{restaurantId}";

        //     // 🔔 RabbitMQ messages
        //     while ((msg = _mq.ReceiveOneForUser(key)) != null)
        //     {
        //         messages.Add("[RabbitMQ] " + msg);
        //     }

        //     // 🔔 Redis messages
        //     while ((msg = _chat.ReceiveOneForUser(key)) != null)
        //     {
        //         messages.Add("[Redis] " + msg);
        //     }

        //     return Ok(new
        //     {
        //         success = true,
        //         messages
        //     });
        // }

        //conque messages
        [HttpGet("restaurant/by-customer/{customerId}")]
        public async Task<IActionResult> GetRestaurantNotificationsByCustomer(int customerId)
        {
            int restaurantId = await _service.GetRestaurantIdByCustomer(customerId);
            // Console.WriteLine("Restaurant ID: " + restaurantId);
            if (restaurantId == 0)
                return NotFound("Restaurant not found for this customer");

            // var messages = new List<string>();
            // string? msg;


            var msgs = new List<string>();
            string? msg;
            string key = $"restaurant_{restaurantId}";
            while ((msg = _mq.ReceiveOneForUser(key)) != null)
            {
                msgs.Add("[RabbitMQ] " + msg);
            }

            // Also get from Redis queue
            // string? redisMsg;
            // while ((redisMsg = _chat.ReceiveOneForUser(key)) != null)
            // {
            //     msgs.Add("[Redis] " + redisMsg);
            // }

            return Ok(new { success = true, messages = msgs });

        }
        //conque messages
        // 🔔 Customer notifications
        [HttpGet("customer/{customerId}")]
        public IActionResult GetCustomerNotifications(int customerId)
        {
            var msgs = new List<string>();
            string? msg;
            string key = $"customer_{customerId}";

            // 🔔 RabbitMQ messages
            while ((msg = _mq.ReceiveOneForUser(key)) != null)
            {
                msgs.Add("[RabbitMQ] " + msg);
            }

            // // 🔔 Redis messages
            // string? redisMsg;
            // while ((redisMsg = _chat.ReceiveOneForUser(key)) != null)
            // {
            //     msgs.Add("[Redis] " + redisMsg);
            // }

            return Ok(new { success = true, messages = msgs });
        }
    }

    // DTO
    public class DeleteNotificationRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}

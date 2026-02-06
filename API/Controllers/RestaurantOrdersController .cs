using Microsoft.AspNetCore.Mvc;
using Repository.service;
using API.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/restaurant/orders")]
    public class RestaurantOrdersController : ControllerBase
    {
        private readonly IRestaurantProfileMenu _repo;
        private readonly RabbitMqService _mq;
        private readonly IChatService _chat;

        public RestaurantOrdersController(IRestaurantProfileMenu repo, RabbitMqService mq, IChatService chat)
        {
            _repo = repo;
            _mq = mq;
            _chat = chat;
        }

        // 🔹 GET ORDERS FOR GRID
        [HttpGet("by-customer/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomer(int customerId)
        {
            int restaurantId = await _repo.GetRestaurantIdByCustomer(customerId);

            if (restaurantId == 0)
                return NotFound("Restaurant not found");

            return Ok(await _repo.GetRestaurantOrders(restaurantId));
        }
        // 🔹 STATUS CHANGE (Restaurant Action)
        // [HttpPut("status")]
        // public async Task<IActionResult> UpdateStatus(int bookingId, string status)
        // {
        //     bool result = await _repo.UpdateOrderStatus(bookingId, status);

        //     if (result)
        //     {
        //         // Get customer ID from booking ID
        //         var (customerId, orderId) = await _repo.GetOrderCustomerInfo(bookingId);

        //         if (customerId > 0)
        //         {
        //             // Prepare notification message based on status
        //             string notificationMessage = status.ToLower() switch
        //             {
        //                 "accepted" or "processing" => $"✅ Your order #{orderId} has been accepted by the restaurant and is being prepared!",
        //                 "prepared" => $"✅ Your order #{orderId} has been accepted and prepared! It will be sent for delivery soon!",
        //                 "out for delivery" => $"🚚 Your order #{orderId} has been sent and is out for delivery!",
        //                 "delivered" => $"🎉 Your order #{orderId} has been delivered! Enjoy your meal!",
        //                 _ => $"📦 Your order #{orderId} status has been updated to: {status}"
        //             };

        //             string customerKey = $"customer_{customerId}";

        //             // Send notification via Redis
        //             _chat.SendToUser("System", customerKey, notificationMessage);

        //             // Send notification via RabbitMQ
        //             _mq.SendToUser("System", customerKey, notificationMessage);

        //             Console.WriteLine($"Notification sent to customer {customerId} for order {orderId} with status: {status}");


        //             var restaurantId = await _repo.GetRestaurantIdByCustomer(customerId);



        //             string restaurantKey = $"restaurant_{restaurantId}";
        //             string restaurantMessage = $"📦 Order #{orderId} status updated to: {status}";

        //             // Notify restaurant as well
        //             _chat.SendToUser("System", restaurantKey, restaurantMessage);
        //             _mq.SendToUser("System", restaurantKey, restaurantMessage);

        //         }

        //         return result ? Ok("Updated") : BadRequest("Failed");
        //     }
        // }
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus(int bookingId, string status)
        {
            bool result = await _repo.UpdateOrderStatus(bookingId, status);

            if (result)
            {
                // Get order info (customer ID, restaurant ID, order ID)
                var (customerId, restaurantId, orderId) = await _repo.GetOrderInfo(bookingId);
            
                if (customerId > 0)
                {
                    // Prepare notification message for customer based on status
                    string customerNotificationMessage = status.ToLower() switch
                    {
                        "accepted" or "processing" => $"✅ Your order #{orderId} has been accepted by the restaurant and is being prepared!",
                        "prepared" => $"✅ Your order #{orderId} has been accepted and prepared! It will be sent for delivery soon!",
                        "out for delivery" => $"🚚 Your order #{orderId} has been sent and is out for delivery!",
                        "delivered" => $"🎉 Your order #{orderId} has been delivered! Enjoy your meal!",
                        _ => $"📦 Your order #{orderId} status has been updated to: {status}"
                    };

                    string customerKey = $"customer_{customerId}";

                    // Send notification to customer via Redis
                    _chat.SendToUser("System", customerKey, customerNotificationMessage);

                    // Send notification to customer via RabbitMQ
                    _mq.SendToUser("System", customerKey, customerNotificationMessage);

                    Console.WriteLine($"Notification sent to customer {customerId} for order {orderId} with status: {status}");
                }

                // Send notification to restaurant
                if (restaurantId > 0)
                {


                    // Prepare notification message for restaurant based on status
                    string restaurantNotificationMessage = status.ToLower() switch
                    {
                        "accepted" or "processing" => $"✅ Order #{orderId} status updated to: {status}",
                        "prepared" => $"✅ Order #{orderId} has been marked as prepared!",
                        "out for delivery" => $"🚚 Order #{orderId} has been sent for delivery!",
                        "delivered" => $"🎉 Order #{orderId} has been delivered successfully!",
                        _ => $"📦 Order #{orderId} status updated to: {status}"
                    };

                    string restaurantKey = $"restaurant_{restaurantId}";

                    // Send notification to restaurant via Redis
                    _chat.SendToUser("System", restaurantKey, restaurantNotificationMessage);

                    // Send notification to restaurant via RabbitMQ
                    _mq.SendToUser("System", restaurantKey, restaurantNotificationMessage);

                }

                return Ok("Updated");
            }

            return BadRequest("Failed");
        }


        [HttpGet("card/by-customer/{customerId}")]
        public async Task<IActionResult> GetCardsByCustomer(int customerId)
        {
            int restaurantId = await _repo.GetRestaurantIdByCustomer(customerId);

            if (restaurantId == 0)
                return NotFound("Restaurant not found");

            return Ok(await _repo.GetOrderCardSummary(restaurantId));
        }
    }
}

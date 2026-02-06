using Microsoft.AspNetCore.Mvc;
using API.service;
using Repository.Model;
using API.Services;
using Repository.service;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderInterface _orderService;
        private readonly RabbitMqService _mq;
        private readonly IChatService _chat;
        private readonly IOrderHistoryInterface _orderHistoryService;
        private readonly IRestaurantProfileMenu _restaurantService;

        public OrderController(IOrderInterface orderService, RabbitMqService mq, IChatService chat, IOrderHistoryInterface orderHistoryService, IRestaurantProfileMenu restaurantService)
        {
            _orderService = orderService;
            _mq = mq;
            _chat = chat;
            _orderHistoryService = orderHistoryService;
            _restaurantService = restaurantService;
        }

        // ===============================
        // CREATE ORDER
        // ===============================
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromForm] t_order order)
        {
            try
            {
                // ✅ Pass ONLY image name to service
                if (order.c_imageurlPicture != null)
                {
                    order.c_foodimage = order.c_imageurlPicture.FileName;
                }

                int result = await _orderService.CreateOrderAsync(order);

                if (result == 1)
                {
                    // Send notification to restaurant about new order
                    if (order.c_restaurantid > 0)
                    {
                        // Get restaurant owner's customer ID to send notification
                        int restaurantCustomerId = await _restaurantService.GetCustomerIdByRestaurantId(order.c_restaurantid);
                        if (restaurantCustomerId > 0)
                        {
                            // Notification to restaurant
                            string restaurantMessage = $"🆕 New order #{order.c_orderid} has been placed! Please check and accept the order.";
                            string restaurantKey = $"restaurant_{restaurantCustomerId}";
                            
                            _chat.SendToUser("System", restaurantKey, restaurantMessage);
                            _mq.SendToUser("System", restaurantKey, restaurantMessage);
                        }
                    }
                    
                    return Ok(new
                    {
                        success = true,
                        message = "Order placed successfully"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order placement failed"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in OrderController ---> " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpDelete("cancel/{bookingId}")]
        public async Task<IActionResult> CancelOrder(int bookingId)
        {
            try
            {
                // Get order details before cancelling to send notifications
                var order = await _orderHistoryService.GetOrderByBookingIdAsync(bookingId);
                
                int result = await _orderService.CancelOrderAsync(bookingId);

                if (result == 1)
                {
                    // Send notifications to both customer and restaurant
                    if (order != null)
                    {
                        // Notification to customer
                        string customerMessage = $"❌ Order #{order.c_orderid} has been cancelled. Refund will be processed within 7 working days.";
                        string customerKey = $"customer_{order.c_customerid}";
                        
                        _chat.SendToUser("System", customerKey, customerMessage);
                        _mq.SendToUser("System", customerKey, customerMessage);
                        
                        // Send notification to restaurant using restaurant ID
                        if (order.c_restaurantid > 0)
                        {
                            // Notification to restaurant
                            string restaurantMessage = $"⚠️ Order #{order.c_orderid} has been cancelled by the customer.";
                            string restaurantKey = $"restaurant_{order.c_restaurantid}";
                            
                            _chat.SendToUser("System", restaurantKey, restaurantMessage);
                            _mq.SendToUser("System", restaurantKey, restaurantMessage);
                            
                            // Console.WriteLine($"✅ Restaurant cancellation notification sent - Restaurant ID: {order.c_restaurantid}, Key: {restaurantKey}");
                        }
                        else
                        {
                            // Console.WriteLine($"❌ ERROR: Restaurant ID is 0. Cannot send cancellation notification.");
                        }
                    }
                    
                    return Ok(new
                    {
                        success = true,
                        message = "Order cancelled successfully. The refund amount will be credited to your original payment method within 7 working days."
                    });
                }
                else if (result == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order cannot be cancelled (already processed)"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Error while cancelling order"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CancelOrder ---> " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}

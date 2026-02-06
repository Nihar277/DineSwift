using Microsoft.AspNetCore.Mvc;
using API.service;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderHistoryController : ControllerBase
    {
        private readonly IOrderHistoryInterface _orderHistoryService;

        public OrderHistoryController(IOrderHistoryInterface orderHistoryService)
        {
            _orderHistoryService = orderHistoryService;
        }

        // =========================================
        // 1️⃣ GET ALL ORDER HISTORY (My Orders Page)
        // =========================================
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetAllOrders(int customerId)
        {
            Console.WriteLine(customerId);
            try
            {
                var orders = await _orderHistoryService.GetAllOrdersAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllOrders ---> " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        // =========================================
        // 2️⃣ GET ORDER BY BOOKING ID (Order Details)
        // =========================================
        [HttpGet("details/{bookingId}")]
        public async Task<IActionResult> GetOrderByBookingId(int bookingId)
        {
            try
            {
                var order = await _orderHistoryService.GetOrderByBookingIdAsync(bookingId);

                if (order == null)
                    return NotFound("Order not found");

                return Ok(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetOrderByBookingId ---> " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }


        // =========================================
        // 3️⃣ GET ORDER PROGRESS (Progress Bar)
        // =========================================
        [HttpGet("progress/{bookingId}")]
        public async Task<IActionResult> GetOrderProgress(int bookingId)
        {
            try
            {
                var status = await _orderHistoryService.GetOrderProgressAsync(bookingId);

                if (status == null)
                    return NotFound("Order not found");

                // ✅ 4-stage progress mapping
                var progress = new
                {
                    processing = status is "Processing" or "Prepared" or "Out for delivery" or "Delivered",
                    prepared = status is "Prepared" or "Out for delivery" or "Delivered",
                    outForDelivery = status is "Out for delivery" or "Delivered",
                    delivered = status == "Delivered"
                };

                return Ok(progress);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetOrderProgress ---> " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
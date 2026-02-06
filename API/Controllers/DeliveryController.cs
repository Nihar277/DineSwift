using Microsoft.AspNetCore.Mvc;
using Repository.service;// For IDeliveryService
using Repository.Model;
using System.Runtime.Versioning;
using API.Services;
using Microsoft.AspNetCore.Http;



namespace API.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {

        private readonly IDeliveryService _service;
        private readonly IEmailService _emailService;
        private readonly Repository.Interfaces.IDeliveryProfileService _profileService;

        private readonly RabbitMqService _mq;
        private readonly IChatService _chat;
        private readonly IRestaurantProfileMenu _restaurantService;

        public DeliveryController(IDeliveryService service, IEmailService emailService, RabbitMqService mq, IChatService chat, Repository.Interfaces.IDeliveryProfileService profileService, IRestaurantProfileMenu restaurantService)
        {
            _service = service;
            _emailService = emailService;
            _mq = mq;
            _chat = chat;
            _profileService = profileService;
            _restaurantService = restaurantService;
        }


        // DTO for updating order status
        public class UpdateOrderStatusRequest
        {
            public int BookingId { get; set; }
            public string Status { get; set; }
        }

        [HttpGet]
        [Route("GetAllState")]
        public async Task<IActionResult> GetAllStates()
        {

            var states = await _service.GetAllStateAsync();
            return Ok(states);
        }
        [HttpGet]
        [Route("GetAllCity")]
        public async Task<IActionResult> GetAllCity(int id)
        {
            // Console.WriteLine("---------------------------------------------"+id);
            var states = await _service.GetAllCityAsync(id);
            return Ok(states);
        }

        [HttpPost("register-delivery")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterDeliveryPartner(
    [FromForm] DeliveryPartnerForm form
)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var customer = new t_customer
                {
                    c_fname = form.c_fname,
                    c_lname = form.c_lname,
                    c_email = form.c_email,
                    c_password = form.c_password,
                    c_phonenumber = form.c_phonenumber,
                    c_state = form.c_state,
                    c_city = form.c_city,
                    c_pincode = form.c_pincode,
                    c_address = form.c_address,
                    c_gender = form.c_gender,
                    c_imagefile = form.c_imagefile
                };

                int id = await _service.RegisterDeliveryPartnerAsync(customer, form);

                await _emailService.SendRegistrationEmailAsync(customer.c_email, customer.c_fname + " " + customer.c_lname, UserType.DeliveryPartner);
                string message =
           $"New Delivery Partner registered: {customer.c_fname} {customer.c_lname} ({customer.c_email})";

                _mq.SendToUser("System", "admin", message);
                _chat.SendToUser("System", "admin", message);
                return Ok(new
                {
                    message = "Delivery partner registered successfully",
                    customerId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("update-status")]
        public async Task<IActionResult> UpdateOrderStatus(
        [FromBody] UpdateOrderStatusRequest req)
        {
            if (req == null || req.BookingId <= 0 || string.IsNullOrWhiteSpace(req.Status))
                return BadRequest(new { message = "Invalid request" });

            bool ok = await _service.UpdateOrderStatusAsync(req.BookingId, req.Status);

            if (!ok)
                return NotFound(new { message = "Order not updated" });

            // Send notifications when order is delivered
            if (string.Equals(req.Status, "Delivered", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // Get order information
                    var orderInfo = await _restaurantService.GetOrderInfo(req.BookingId);
                    if (orderInfo.CustomerId > 0 && orderInfo.RestaurantId > 0)
                    {
                        // Prepare notification messages
                        string customerNotificationMessage = $"🎉 Your order #{orderInfo.OrderId} has been delivered! Enjoy your meal!";
                        string restaurantNotificationMessage = $"🎉 Order #{orderInfo.OrderId} has been delivered successfully!";

                        // Send notification to customer
                        string customerKey = $"customer_{orderInfo.CustomerId}";
                        _mq.SendToUser("System", customerKey, customerNotificationMessage);
                        _chat.SendToUser("System", customerKey, customerNotificationMessage);

                        // Send notification to restaurant
                        string restaurantKey = $"restaurant_{orderInfo.RestaurantId}";
                        _mq.SendToUser("System", restaurantKey, restaurantNotificationMessage);
                        _chat.SendToUser("System", restaurantKey, restaurantNotificationMessage);

                        Console.WriteLine($"Delivery notification sent to customer {orderInfo.CustomerId} and restaurant {orderInfo.RestaurantId} for order {orderInfo.OrderId}");
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the request
                    Console.WriteLine("Notification sending error: " + ex.Message);
                }
            }

            return Ok(new { message = "Order updated successfully" });
        }



        [HttpGet("available-orders")]
        public async Task<IActionResult> GetAvailableOrders()
        {
            try
            {
                var orders = await _profileService.GetAvailableOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("cards")]
        public async Task<IActionResult> GetCards()
        {
            try
            {
                var cards = await _profileService.GetOrderCardSummaryAsync();
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("active-order/{deliveryPartnerId}")]
        public async Task<IActionResult> GetActiveOrder(int deliveryPartnerId)
        {
            try
            {
                var order = await _profileService.GetActiveDeliveryOrderAsync(deliveryPartnerId);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("accept-order")]
        public async Task<IActionResult> AcceptOrderEndpoint([FromBody] AcceptOrderRequest req)
        {
            if (req == null || req.BookingId <= 0 || req.DeliveryPartnerId <= 0)
                return BadRequest(new { message = "Invalid request" });

            try
            {
                // Check if partner can accept
                bool canAccept = await _profileService.CanAcceptOrderAsync(req.DeliveryPartnerId);
                if (!canAccept)
                {
                    var activeOrder = await _profileService.GetActiveDeliveryOrderAsync(req.DeliveryPartnerId);
                    return Conflict(new
                    {
                        message = "You have an active delivery order. Complete it first.",
                        activeOrder = activeOrder
                    });
                }

                bool assigned = await _profileService.AssignOrderToPartnerAsync(req.BookingId, req.DeliveryPartnerId);
                if (assigned)
                {
                    // Send notifications when order is accepted by delivery partner
                    try
                    {
                        // Get order information
                        var orderInfo = await _restaurantService.GetOrderInfo(req.BookingId);
                        if (orderInfo.CustomerId > 0 && orderInfo.RestaurantId > 0)
                        {
                            // Prepare notification messages
                            string customerNotificationMessage = $"🚚 Your order #{orderInfo.OrderId} has been accepted by a delivery partner and is out for delivery!";
                            string restaurantNotificationMessage = $"🚚 Order #{orderInfo.OrderId} has been accepted by delivery partner and is out for delivery!";

                            // Send notification to customer
                            string customerKey = $"customer_{orderInfo.CustomerId}";
                            _mq.SendToUser("System", customerKey, customerNotificationMessage);
                            _chat.SendToUser("System", customerKey, customerNotificationMessage);

                            // Send notification to restaurant
                            string restaurantKey = $"restaurant_{orderInfo.RestaurantId}";
                            _mq.SendToUser("System", restaurantKey, restaurantNotificationMessage);
                            _chat.SendToUser("System", restaurantKey, restaurantNotificationMessage);

                            Console.WriteLine($"Order acceptance notification sent to customer {orderInfo.CustomerId} and restaurant {orderInfo.RestaurantId} for order {orderInfo.OrderId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail the request
                        Console.WriteLine("Order acceptance notification sending error: " + ex.Message);
                    }

                    return Ok(new { message = "Order accepted successfully" });
                }

                return BadRequest(new { message = "Failed to accept order" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class AcceptOrderRequest
        {
            public int BookingId { get; set; }
            public int DeliveryPartnerId { get; set; }
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetProfile(int customerId)
        {
            var data = await _profileService.GetDeliveryBoyProfile(customerId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateCustomerProfile([FromBody] t_customer customer)
        {
            if (customer == null || customer.c_customerid == 0)
                return BadRequest("Invalid customer data");

            // SAME SERVICE — but CUSTOMER update
            bool result = await _profileService.UpdateCustomerProfile(customer);

            if (!result)
                return BadRequest("Profile update failed");

            return Ok(new
            {
                success = true,
                message = "Customer profile updated successfully"
            });
        }


        // ================= UPDATE CUSTOMER IMAGE =================
        [HttpPost("update-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCustomerImage(
    [FromForm] UpdateCustomerImageDto dto)
        {
            if (dto.CustomerId == 0)
                return BadRequest("Invalid customer id");

            // CASE 1: Image file upload
            if (dto.ImageFile != null)
            {
                var profile = await _profileService.GetDeliveryBoyProfile(dto.CustomerId);
                if (profile == null)
                    return NotFound("Delivery profile not found");

                string webRoot = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
                    "MVC", "wwwroot", "Images", "customer"
                );

                Directory.CreateDirectory(webRoot);

                string fileName = (profile.c_email ?? "user") +
                                  Path.GetExtension(dto.ImageFile.FileName);

                string path = Path.Combine(webRoot, fileName);

                using var fs = new FileStream(path, FileMode.Create);
                await dto.ImageFile.CopyToAsync(fs);

                bool updated = await _profileService.UpdateCustomerImage(dto.CustomerId, fileName);
                if (!updated)
                    return BadRequest("Image update failed");

                return Ok(new
                {
                    success = true,
                    message = "Customer image updated successfully",
                    c_image = fileName,
                    c_image_url = "/Images/customer/" + fileName
                });
            }

            // CASE 2: Image path only
            if (string.IsNullOrEmpty(dto.ImagePath))
                return BadRequest("Invalid image path");

            bool result = await _profileService.UpdateCustomerImage(dto.CustomerId, dto.ImagePath);

            if (!result)
                return BadRequest("Image update failed");

            return Ok(new
            {
                success = true,
                message = "Customer image updated successfully"
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto == null ||
                dto.CustomerId <= 0 ||
                string.IsNullOrWhiteSpace(dto.OldPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { success = false, message = "Invalid input data" });
            }

            var user = await _profileService.GetCustomerProfile(dto.CustomerId);
            if (user == null || string.IsNullOrEmpty(user.c_password))
            {
                return BadRequest(new { success = false, message = "User password not found" });
            }

            bool isOldPasswordCorrect =
                BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.c_password);

            if (!isOldPasswordCorrect)
            {
                return Ok(new { success = false, message = "Old password is incorrect" });
            }

            string newHashedPassword =
                BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            bool result =
                await _profileService.UpdatePassword(dto.CustomerId, newHashedPassword);

            return Ok(new
            {
                success = result,
                message = result ? "Password changed successfully" : "Password not updated"
            });
        }

        // ================= DASHBOARD ENDPOINTS =================

        [HttpGet("dashboard-summary/{deliveryPartnerId}")]
        public async Task<IActionResult> GetDashboardSummary(int deliveryPartnerId)
        {
            try
            {
                var summary = await _profileService.GetDashboardSummaryAsync(deliveryPartnerId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("dashboard-chart/{deliveryPartnerId}/{period}")]
        public async Task<IActionResult> GetDashboardChart(int deliveryPartnerId, int period)
        {
            try
            {
                var chartData = await _profileService.GetDashboardChartDataAsync(deliveryPartnerId, period);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("recent-orders/{deliveryPartnerId}")]
        public async Task<IActionResult> GetRecentOrders(int deliveryPartnerId)
        {
            try
            {
                var orders = await _profileService.GetRecentOrdersAsync(deliveryPartnerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}

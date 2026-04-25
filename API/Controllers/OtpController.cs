using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Model;
using Repository.service;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OtpController> _logger;

        public OtpController(IOtpService otpService, IEmailService emailService, ILogger<OtpController> logger)
        {
            _otpService = otpService;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Send OTP to email
        /// </summary>
        [HttpPost("send")]
        public async Task<ActionResult<OtpResponse>> SendOtp()
        {
            var request = await ReadRequestAsync<SendOtpRequest>();

            if (!TryValidateOtpEmail(request?.Email, out var email))
            {
                return BadRequest(new OtpResponse
                {
                    Success = false,
                    Message = "Please enter a valid email address."
                });
            }

            var (success, message) = await _otpService.SendOtpAsync(email);

            if (success)
            {
                return Ok(new OtpResponse
                {
                    Success = true,
                    Message = message,
                    Data = new { expiresIn = "5 minutes" }
                });
            }

            return CreateOtpErrorResponse(message);
        }

        /// <summary>
        /// Verify OTP
        /// </summary>
        [HttpPost("verify")]
        public async Task<ActionResult<OtpResponse>> VerifyOtp()
        {
            var request = await ReadRequestAsync<VerifyOtpRequest>();

            if (!TryValidateOtpEmail(request?.Email, out var email) ||
                string.IsNullOrWhiteSpace(request?.Otp) ||
                request.Otp.Trim().Length != 6)
            {
                return BadRequest(new OtpResponse
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            var (success, message) = await _otpService.VerifyOtpAsync(email, request.Otp.Trim());

            if (success)
            {
                return Ok(new OtpResponse
                {
                    Success = true,
                    Message = message
                });
            }

            return CreateOtpErrorResponse(message);
        }

        /// <summary>
        /// Resend OTP
        /// </summary>
        [HttpPost("resend")]
        public async Task<ActionResult<OtpResponse>> ResendOtp()
        {
            var request = await ReadRequestAsync<SendOtpRequest>();

            if (!TryValidateOtpEmail(request?.Email, out var email))
            {
                return BadRequest(new OtpResponse
                {
                    Success = false,
                    Message = "Please enter a valid email address."
                });
            }

            var (success, message) = await _otpService.ResendOtpAsync(email);

            if (success)
            {
                return Ok(new OtpResponse
                {
                    Success = true,
                    Message = message
                });
            }

            return CreateOtpErrorResponse(message);
        }

        private async Task<T?> ReadRequestAsync<T>() where T : class
        {
            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                var values = form.ToDictionary(x => x.Key, x => x.Value.ToString());
                var json = JsonSerializer.Serialize(values);

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            Request.EnableBuffering();

            if (Request.Body.CanSeek)
            {
                Request.Body.Position = 0;
            }

            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            if (Request.Body.CanSeek)
            {
                Request.Body.Position = 0;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private bool TryValidateOtpEmail(string? rawEmail, out string email)
        {
            email = rawEmail?.Trim() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(email) && new EmailAddressAttribute().IsValid(email);
        }

        private ActionResult<OtpResponse> CreateOtpErrorResponse(string message)
        {
            var response = new OtpResponse
            {
                Success = false,
                Message = message
            };

            if (message.Contains("wait", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, response);
            }

            if (message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("failed to send", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }

            return BadRequest(response);
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Register Customer
        /// </summary>
        [HttpPost("register/customer")]
        public async Task<ActionResult<OtpResponse>> RegisterCustomer([FromForm] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new OtpResponse { Success = false, Message = "Invalid registration data" });
                }

                request.UserType = UserType.Customer;
                var emailSent = await _emailService.SendRegistrationEmailAsync(request.Email, request.Name, UserType.Customer);

                if (!emailSent)
                {
                    return StatusCode(500, new OtpResponse { Success = false, Message = "Registration successful but failed to send confirmation email" });
                }

                return Ok(new OtpResponse
                {
                    Success = true,
                    Message = "Customer registered successfully! Check your email for details.",
                    Data = new { email = request.Email, name = request.Name, userType = "Customer" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering customer: {ex.Message}");
                return StatusCode(500, new OtpResponse { Success = false, Message = "Registration failed" });
            }
        }

        /// <summary>
        /// Register Restaurant
        /// </summary>
        [HttpPost("register/restaurant")]
        public async Task<ActionResult<OtpResponse>> RegisterRestaurant([FromForm] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new OtpResponse { Success = false, Message = "Invalid registration data" });
                }

                if (string.IsNullOrEmpty(request.RestaurantName) || string.IsNullOrEmpty(request.Address))
                {
                    return BadRequest(new OtpResponse { Success = false, Message = "Restaurant name and address are required" });
                }

                request.UserType = UserType.Restaurant;
                var emailSent = await _emailService.SendRegistrationEmailAsync(request.Email, request.Name, UserType.Restaurant, request);

                if (!emailSent)
                {
                    return StatusCode(500, new OtpResponse { Success = false, Message = "Registration successful but failed to send confirmation email" });
                }

                return Ok(new OtpResponse
                {
                    Success = true,
                    Message = "Restaurant registered successfully! Check your email for next steps.",
                    Data = new { email = request.Email, name = request.Name, restaurantName = request.RestaurantName, address = request.Address, userType = "Restaurant" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering restaurant: {ex.Message}");
                return StatusCode(500, new OtpResponse { Success = false, Message = "Registration failed" });
            }
        }

        /// <summary>
        /// Register Delivery Partner
        /// </summary>
        [HttpPost("register/delivery-partner")]
        public async Task<ActionResult<OtpResponse>> RegisterDeliveryPartner([FromForm] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new OtpResponse { Success = false, Message = "Invalid registration data" });
                }

                if (string.IsNullOrEmpty(request.VehicleType) || string.IsNullOrEmpty(request.LicenseNumber))
                {
                    return BadRequest(new OtpResponse { Success = false, Message = "Vehicle type and license number are required" });
                }

                request.UserType = UserType.DeliveryPartner;
                var emailSent = await _emailService.SendRegistrationEmailAsync(request.Email, request.Name, UserType.DeliveryPartner, request);

                if (!emailSent)
                {
                    return StatusCode(500, new OtpResponse { Success = false, Message = "Registration successful but failed to send confirmation email" });
                }

                return Ok(new OtpResponse
                {
                    Success = true,
                    Message = "Delivery Partner registered successfully! Check your email for next steps.",
                    Data = new { email = request.Email, name = request.Name, vehicleType = request.VehicleType, licenseNumber = request.LicenseNumber, userType = "DeliveryPartner" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering delivery partner: {ex.Message}");
                return StatusCode(500, new OtpResponse { Success = false, Message = "Registration failed" });
            }
        }

        /// <summary>
        /// Place Order and Send Confirmation Email
        /// </summary>
        [HttpPost("order/place")]
        public async Task<ActionResult<OrderConfirmationResponse>> PlaceOrder( PlaceOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new OrderConfirmationResponse
                    {
                        Success = false,
                        Message = "Invalid order data"
                    });
                }

                // Validate order items
                if (request.Items == null || !request.Items.Any())
                {
                    return BadRequest(new OrderConfirmationResponse
                    {
                        Success = false,
                        Message = "Order must contain at least one item"
                    });
                }

                // Calculate totals if not provided
                if (request.SubTotal == 0)
                {
                    request.SubTotal = request.Items.Sum(item => item.Total);
                }

                if (request.Total == 0)
                {
                    request.Total = request.SubTotal + request.DeliveryFee + request.Tax;
                }

                // Send order confirmation email
                var emailSent = await _emailService.SendOrderConfirmationEmailAsync(request);

                if (!emailSent)
                {
                    _logger.LogWarning($"Order placed but email failed for order #{request.OrderId}");
                    return Ok(new OrderConfirmationResponse
                    {
                        Success = true,
                        Message = "Order placed successfully but confirmation email failed. You can track your order in the app.",
                        OrderId = request.OrderId,
                        OrderDate = request.OrderDate,
                        TotalAmount = request.Total
                    });
                }

                _logger.LogInformation($"Order #{request.OrderId} placed successfully for {request.CustomerEmail}");

                return Ok(new OrderConfirmationResponse
                {
                    Success = true,
                    Message = "Order placed successfully! Confirmation email sent.",
                    OrderId = request.OrderId,
                    OrderDate = request.OrderDate,
                    TotalAmount = request.Total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error placing order: {ex.Message}");
                return StatusCode(500, new OrderConfirmationResponse
                {
                    Success = false,
                    Message = "Failed to place order. Please try again."
                });
            }
        }
    }
}
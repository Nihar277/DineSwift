using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Repository.service;
using Repository.Model;


namespace Repository.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        private  readonly IPromocodeService _promocodeService;
            
        private object toEmail;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IPromocodeService promocodeService)
        {
            _configuration = configuration;
            _logger = logger;
            _promocodeService = promocodeService;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:From"]));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = "Your OTP Verification Code";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = GetOtpEmailTemplate(otp)
                };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["EmailSettings:Host"],
                    int.Parse(_configuration["EmailSettings:Port"]),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"OTP email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending OTP email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        private string GetOtpEmailTemplate(string otp)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{ 
                            font-family: 'Arial', sans-serif; 
                            background-color: #f4f4f4; 
                            margin: 0; 
                            padding: 0; 
                        }}
                        .email-container {{ 
                            max-width: 600px; 
                            margin: 40px auto; 
                            background: white; 
                            border-radius: 10px; 
                            overflow: hidden;
                            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
                        }}
                        .header {{ 
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                            padding: 30px; 
                            text-align: center; 
                            color: white;
                        }}
                        .content {{ 
                            padding: 40px 30px; 
                        }}
                        .otp-box {{ 
                            background: #f8f9fa; 
                            border: 2px dashed #667eea; 
                            border-radius: 8px; 
                            padding: 20px; 
                            text-align: center; 
                            margin: 30px 0; 
                        }}
                        .otp-code {{ 
                            font-size: 36px; 
                            font-weight: bold; 
                            color: #667eea; 
                            letter-spacing: 8px; 
                            margin: 10px 0;
                            font-family: 'Courier New', monospace;
                        }}
                        .warning {{ 
                            background: #fff3cd; 
                            border-left: 4px solid #ffc107; 
                            padding: 15px; 
                            margin: 20px 0; 
                            border-radius: 4px;
                        }}
                        .footer {{ 
                            background: #f8f9fa; 
                            padding: 20px; 
                            text-align: center; 
                            color: #6c757d; 
                            font-size: 12px; 
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='header'>
                            <h1 style='margin: 0;'>Email Verification</h1>
                        </div>
                        <div class='content'>
                            <h2 style='color: #333;'>Hello!</h2>
                            <p style='color: #666; line-height: 1.6;'>
                                We received a request to verify your email address. 
                                Please use the following OTP code to complete the verification:
                            </p>
                            <div class='otp-box'>
                                <p style='margin: 0; color: #666; font-size: 14px;'>Your OTP Code</p>
                                <div class='otp-code'>{otp}</div>
                                <p style='margin: 0; color: #999; font-size: 12px;'>Valid for 5 minutes</p>
                            </div>
                            <div class='warning'>
                                <strong>⚠️ Security Notice:</strong><br>
                                • Do not share this code with anyone<br>
                                • This code expires in 5 minutes<br>
                                • If you didn't request this, please ignore this email
                            </div>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message, please do not reply.</p>
                            <p>&copy; 2024 Your Company. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        public async Task<bool> SendRegistrationEmailAsync(string toEmail, string name, UserType userType, RegisterRequest details = null)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:From"]));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = $"Welcome to Our Platform - {userType} Registration Successful";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = GetRegistrationEmailTemplate(name, userType, details)
                };

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    _configuration["EmailSettings:Host"],
                    int.Parse(_configuration["EmailSettings:Port"]),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Registration email sent successfully to {toEmail} for {userType}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending registration email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        private string GetRegistrationEmailTemplate(string name, UserType userType, RegisterRequest details)
        {
            var userTypeDisplay = userType switch
            {
                UserType.Customer => "Customer",
                UserType.Restaurant => "Restaurant Partner",
                UserType.DeliveryPartner => "Delivery Partner",
                _ => "User"
            };

            var welcomeMessage = userType switch
            {
                UserType.Customer => "Start exploring delicious food from your favorite restaurants!",
                UserType.Restaurant => "Get ready to showcase your culinary delights to thousands of hungry customers!",
                UserType.DeliveryPartner => "Start earning by delivering orders in your area!",
                _ => "Welcome to our platform!"
            };

            var specificContent = userType switch
            {
                UserType.Customer => GetCustomerSpecificContent(),
                UserType.Restaurant => GetRestaurantSpecificContent(details),
                UserType.DeliveryPartner => GetDeliveryPartnerSpecificContent(details),
                _ => ""
            };

            return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <style>
                body {{ font-family: 'Arial', sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                .email-container {{ max-width: 600px; margin: 40px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                .header {{ background: linear-gradient(135deg, #FF6B6B 0%, #FF8E53 100%); padding: 40px 30px; text-align: center; color: white; }}
                .header h1 {{ margin: 0; font-size: 32px; }}
                .badge {{ display: inline-block; background: rgba(255,255,255,0.3); padding: 8px 20px; border-radius: 20px; margin-top: 10px; font-size: 14px; font-weight: bold; }}
                .content {{ padding: 40px 30px; }}
                .welcome-box {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 10px; text-align: center; margin: 20px 0; }}
                .info-card {{ background: #f8f9fa; border-left: 4px solid #FF6B6B; padding: 20px; margin: 20px 0; border-radius: 5px; }}
                .info-card h3 {{ margin-top: 0; color: #333; }}
                .feature-list {{ list-style: none; padding: 0; }}
                .feature-list li {{ padding: 12px 0; border-bottom: 1px solid #eee; color: #555; }}
                .feature-list li:before {{ content: '✓'; color: #4CAF50; font-weight: bold; margin-right: 10px; }}
                .cta-button {{ display: inline-block; background: #FF6B6B; color: white; padding: 15px 40px; text-decoration: none; border-radius: 25px; font-weight: bold; margin: 20px 0; }}
                .footer {{ background: #333; padding: 30px; text-align: center; color: white; }}
                .footer p {{ margin: 5px 0; font-size: 14px; }}
            </style>
        </head>
        <body>
            <div class='email-container'>
                <div class='header'>
                    <h1>🎉 Welcome DineSwift!</h1>
                    <div class='badge'>{userTypeDisplay}</div>
                </div>
                <div class='content'>
                    <h2 style='color: #333;'>Hi {name}! 👋</h2>
                    <p style='color: #666; font-size: 16px; line-height: 1.8;'>
                        Congratulations! Your registration as a <strong>{userTypeDisplay}</strong> has been completed successfully.
                    </p>
                    <div class='welcome-box'>
                        <h2 style='margin: 0;'>🚀 Let's Get Started!</h2>
                        <p style='margin: 15px 0 0 0; font-size: 16px;'>{welcomeMessage}</p>
                    </div>
                    {specificContent}
                    <div style='text-align: center; margin: 30px 0; color: #fcfafaff;'>
                        <a href='#' class='cta-button'style='color: #fcfafaff;'>Get Started Now</a>
                    </div>
                    <div class='info-card'>
                        <h3>📞 Need Help?</h3>
                        <p style='margin: 5px 0; color: #666;'>
                            Our support team is here to help you 24/7.<br>
                            Email: support@dineswift.com<br>
                            Phone: +1 (555) 123-4567
                        </p>
                    </div>
                </div>
                <div class='footer'>
                    <p style='font-size: 18px; margin-bottom: 15px;'>Connect With Us</p>
                    <p>&copy; 2024 Your Company. All rights reserved.</p>
                    <p style='font-size: 12px; color: #999;'>
                        This email was sent to {toEmail}
                    </p>
                </div>
            </div>
        </body>
        </html>
    ";
        }

        private string GetCustomerSpecificContent()
        {
             Random rnd = new Random();
        int number = rnd.Next(1000, 9999); // 4-digit random number

        string result = "DineSwift_" + number;


        _promocodeService.SendPromoAsync(result);
            return $@"
        <div class='info-card'>
            <h3>🍔 Your Food Journey Starts Now</h3>
            <p>From midnight cravings to weekend feasts — you’re now just 3 taps away from happiness.</p>
            <h4>Choose a restaurant</h4>
            <ul class='feature-list'>
                <li>🔥 Discover popular & trending dishes</li>
                <li>📍 Track your delivery live</li>
                <li>Enjoy every bite 😋</li>
                <li>Enter promo code get free delivery: {result}</li>
            </ul>
        </div>
    ";
        }

        private string GetRestaurantSpecificContent(RegisterRequest details)
        {
            var restaurantInfo = details != null ? $@"
        <div class='info-card'>
            <h3>📋 Your Restaurant Details</h3>
            <p style='margin: 5px 0; color: #666;'>
                <strong>Restaurant Name:</strong> {details.RestaurantName}<br>
                <strong>Address:</strong> {details.Address}<br>
                <strong>Contact Phone:</strong> {details.Phone}
            </p>
        </div>
    " : "";

            return $@"
        {restaurantInfo}
        <div class='info-card'>
            <h3>🍽️ Next Steps</h3>
            <ul class='feature-list'>
                <li>Complete your restaurant profile</li>
                <li>Upload your menu with photos</li>
                <li>Set your operating hours</li>
                <li>Configure delivery zones</li>
                <li>Start receiving orders within 24 hours</li>
            </ul>
        </div>
    ";
        }

        private string GetDeliveryPartnerSpecificContent(RegisterRequest details)
        {
            var deliveryInfo = details != null ? $@"
        <div class='info-card'>
            <h3>🚗 Your Profile</h3>
            <p style='margin: 5px 0; color: #666;'>
                <strong>Vehicle Type:</strong> {details.VehicleType}<br>
                
                <strong>Phone:</strong> {details.Phone}
            </p>
        </div>
    " : "";

            return $@"
        {deliveryInfo}
        <div class='info-card'>
            <h3>📦 Getting Started</h3>
            <ul class='feature-list'>
                <li>Complete your profile verification</li>
                <li>🔔 Get notified instantly when a delivery is assigned</li>
                <li>📍 Follow live pickup & drop locations</li>
                <li>💬 Stay updated with real-time order status</li>
                <li>Deliver orders smoothly and confidently</li>
            </ul>
        </div>
    ";
        }

        public async Task<bool> SendOrderConfirmationEmailAsync(PlaceOrderRequest orderDetails)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:From"]));
                email.To.Add(MailboxAddress.Parse(orderDetails.CustomerEmail));
                email.Subject = $"Order Confirmed #{orderDetails.OrderId} - {orderDetails.RestaurantName}";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = GetOrderConfirmationEmailTemplate(orderDetails)
                };

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    _configuration["EmailSettings:Host"],
                    int.Parse(_configuration["EmailSettings:Port"]),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Order confirmation email sent to {orderDetails.CustomerEmail} for order #{orderDetails.OrderId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending order confirmation email: {ex.Message}");
                return false;
            }
        }

        private string GetOrderConfirmationEmailTemplate(PlaceOrderRequest order)
        {
           var itemsHtml = string.Join("", order.Items.Select(item => $@"
        <tr>
            <td style='padding: 15px; border-bottom: 1px solid #eee;'>
                <div style='font-weight: bold; color: #333;'>{item.ItemName}</div>
                <div style='color: #999; font-size: 14px;'>Qty: {item.Quantity}</div>
            </td>
            <td style='padding: 15px; border-bottom: 1px solid #eee; text-align: right; color: #333;'>
                ₹{item.Price:F2}
            </td>
            <td style='padding: 15px; border-bottom: 1px solid #eee; text-align: right; font-weight: bold; color: #333;'>
                ₹{item.Total:F2}
            </td>
        </tr>
    "));

    return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <style>
                body {{ 
                    font-family: 'Arial', sans-serif; 
                    background-color: #f4f4f4; 
                    margin: 0; 
                    padding: 0; 
                }}
                .email-container {{ 
                    max-width: 650px; 
                    margin: 40px auto; 
                    background: white; 
                    border-radius: 10px; 
                    overflow: hidden; 
                    box-shadow: 0 4px 6px rgba(0,0,0,0.1); 
                }}
                .header {{ 
                    background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); 
                    padding: 30px; 
                    text-align: center; 
                    color: white; 
                }}
                .header h1 {{ 
                    margin: 0; 
                    font-size: 28px; 
                }}
                .success-badge {{ 
                    background: rgba(255,255,255,0.3); 
                    display: inline-block; 
                    padding: 8px 20px; 
                    border-radius: 20px; 
                    margin-top: 10px; 
                    font-size: 14px; 
                }}
                .content {{ 
                    padding: 30px; 
                }}
                .order-id {{ 
                    background: #f8f9fa; 
                    padding: 20px; 
                    border-radius: 8px; 
                    text-align: center; 
                    margin: 20px 0; 
                    border: 2px dashed #4CAF50; 
                }}
                .order-id h2 {{ 
                    margin: 0; 
                    color: #4CAF50; 
                    font-size: 24px; 
                }}
                .info-section {{ 
                    background: #f8f9fa; 
                    padding: 20px; 
                    border-radius: 8px; 
                    margin: 20px 0; 
                }}
                .info-section h3 {{ 
                    margin-top: 0; 
                    color: #333; 
                    font-size: 18px; 
                }}
                .order-table {{ 
                    width: 100%; 
                    border-collapse: collapse; 
                    margin: 20px 0; 
                }}
                .order-table th {{ 
                    background: #4CAF50; 
                    color: white; 
                    padding: 15px; 
                    text-align: left; 
                }}
                .order-table th:last-child {{ 
                    text-align: right; 
                }}
                .totals-section {{ 
                    background: #f8f9fa; 
                    padding: 20px; 
                    border-radius: 8px; 
                    margin: 20px 0; 
                }}
                .total-row {{ 
                    display: flex; 
                    justify-content: space-between; 
                    padding: 10px 0; 
                    color: #666; 
                    font-size: 16px; 
                }}
                .total-row.grand-total {{ 
                    border-top: 2px solid #4CAF50; 
                    margin-top: 10px; 
                    padding-top: 15px; 
                    font-weight: bold; 
                    font-size: 20px; 
                    color: #333; 
                }}
                .delivery-info {{ 
                    background: linear-gradient(135deg, #2196F3 0%, #1976D2 100%); 
                    color: white; 
                    padding: 20px; 
                    border-radius: 8px; 
                    text-align: center; 
                    margin: 20px 0; 
                }}
                .delivery-info h3 {{ 
                    margin: 0 0 10px 0; 
                }}
                .timeline {{ 
                    background: #fff3cd; 
                    border-left: 4px solid #ffc107; 
                    padding: 15px; 
                    border-radius: 4px; 
                    margin: 20px 0; 
                }}
                .track-button {{ 
                    display: inline-block; 
                    background: #4CAF50; 
                    color: white; 
                    padding: 15px 40px; 
                    text-decoration: none; 
                    border-radius: 25px; 
                    font-weight: bold; 
                    margin: 20px 0; 
                }}
                .footer {{ 
                    background: #333; 
                    padding: 30px; 
                    text-align: center; 
                    color: white; 
                }}
                .footer p {{ 
                    margin: 5px 0; 
                    font-size: 14px; 
                }}
            </style>
        </head>
        <body>
            <div class='email-container'>
                <div class='header'>
                    <h1>✓ Order Confirmed!</h1>
                    <div class='success-badge'>Your order is being prepared</div>
                </div>

                <div class='content'>
                    <h2 style='color: #333;'>Hi {order.CustomerName}! 👋</h2>
                    <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                        Thank you for your order! We've received it and {order.RestaurantName} is now preparing your delicious meal.
                    </p>

                    <div class='order-id'>
                        <p style='margin: 0; color: #666; font-size: 14px;'>Order ID</p>
                        <h2>#{order.OrderId}</h2>
                        <p style='margin: 5px 0 0 0; color: #999; font-size: 12px;'>{order.OrderDate:MMM dd, yyyy • hh:mm tt}</p>
                    </div>

                    <div class='delivery-info'>
                        <h3>🚚 Estimated Delivery Time</h3>
                        <p style='font-size: 24px; font-weight: bold; margin: 10px 0;'>{order.EstimatedDeliveryMinutes} Minutes</p>
                        <p style='margin: 0; font-size: 14px;'>We'll notify you when your order is on the way!</p>
                    </div>

                    <div class='info-section'>
                        <h3>📍 Delivery Address</h3>
                        <p style='margin: 0; color: #666; line-height: 1.6;'>{order.DeliveryAddress}</p>
            
                    </div>

                    <div class='info-section'>
                        <h3>🍽️ Order Details from {order.RestaurantName}</h3>
                        <table class='order-table'>
                            <thead>
                                <tr>
                                    <th>Item</th>
                                    <th style='text-align: right;'>Price</th>
                                    <th style='text-align: right;'>Total</th>
                                </tr>
                            </thead>
                            <tbody>
                                {itemsHtml}
                            </tbody>
                        </table>
                    </div>

                    <div class='totals-section'>
                        <div class='total-row'>
                            <span>Subtotal</span>
                            <span>₹{order.SubTotal:F2}</span>
                        </div>
                        <div class='total-row'>
                            <span>Delivery Fee</span>
                            <span>₹{order.DeliveryFee:F2}</span>
                        </div>
                        <div class='total-row'>
                            <span>Tax</span>
                            <span>₹{order.Tax:F2}</span>
                        </div>
                        <div class='total-row grand-total'>
                            <span>Total Amount</span>
                            <span>₹{order.Total:F2}</span>
                        </div>
                        {(string.IsNullOrEmpty(order.PaymentMethod) ? "" : $@"
                        <div style='text-align: center; margin-top: 15px; padding-top: 15px; border-top: 1px solid #ddd;'>
                            <p style='margin: 0; color: #666;'>Payment Method: <strong>{order.PaymentMethod}</strong></p>
                        </div>
                        ")}
                    </div>

                    <div class='timeline'>
                        <h3 style='margin: 0 0 10px 0; color: #333;'>📋 Order Status Timeline</h3>
                        <p style='margin: 5px 0; color: #666;'>✓ Order Placed - {order.OrderDate:hh:mm tt}</p>
                        <p style='margin: 5px 0; color: #666;'>⏳ Being Prepared - In Progress</p>
                        <p style='margin: 5px 0; color: #999;'>🚚 Out for Delivery - Pending</p>
                        <p style='margin: 5px 0; color: #999;'>✓ Delivered - Pending</p>
                    </div>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='#' class='track-button'>Track Your Order</a>
                    </div>

                    <div class='info-section'>
                        <h3>💡 Need Help?</h3>
                        <p style='margin: 5px 0; color: #666;'>
                            Have questions about your order? We're here to help!<br>
                            📧 Email: support@dineswift.com<br>
                            📞 Phone: +91 (555) 123-4567<br>
                            💬 Live Chat: Available 24/7
                        </p>
                    </div>
                </div>

                <div class='footer'>
                    <p style='font-size: 18px; margin-bottom: 10px;'>Thank You for Choosing Us! 🎉</p>
                    <p>&copy; 2024 Your Company. All rights reserved.</p>
                    <p style='font-size: 12px; color: #999; margin-top: 15px;'>
                        This email was sent to {order.CustomerEmail}<br>
                        Order ID: #{order.OrderId}
                    </p>
                </div>
            </div>
        </body>
        </html>
    ";
}
    }


}
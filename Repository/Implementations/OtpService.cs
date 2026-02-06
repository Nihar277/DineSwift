using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Repository.Model;

namespace Repository.service
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        private readonly ILogger<OtpService> _logger;
        private const int OTP_EXPIRY_MINUTES = 5;
        private const int MAX_ATTEMPTS = 3;
        private const int RESEND_COOLDOWN_SECONDS = 60;

        public OtpService(
            IMemoryCache cache,
            IEmailService emailService,
            ILogger<OtpService> logger)
        {
            _cache = cache;
            _emailService = emailService;
            
            _logger = logger;
        }

        public async Task<(bool success, string message)> SendOtpAsync(string email)
        {
            try
            {
                // Check for recent OTP request (rate limiting)
                var rateLimitKey = $"ratelimit_{email}";
                if (_cache.TryGetValue(rateLimitKey, out _))
                {
                    return (false, $"Please wait {RESEND_COOLDOWN_SECONDS} seconds before requesting a new OTP");
                }

                // Generate 6-digit OTP
                var otp = GenerateOtp();

                // Create OTP data
                var otpData = new OtpData
                {
                    Otp = otp,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
                    Attempts = 0
                };

                // Store OTP in cache
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(OTP_EXPIRY_MINUTES));

                _cache.Set(email, otpData, cacheOptions);

                // Set rate limit
                var rateLimitOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(RESEND_COOLDOWN_SECONDS));
                _cache.Set(rateLimitKey, true, rateLimitOptions);

                // Send email
                var emailSent = await _emailService.SendOtpEmailAsync(email, otp);

                if (!emailSent)
                {
                    _cache.Remove(email);
                    _cache.Remove(rateLimitKey);
                    return (false, "Failed to send OTP email. Please try again.");
                }

                _logger.LogInformation($"OTP generated and sent to {email}");
                return (true, "OTP sent successfully to your email");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendOtpAsync: {ex.Message}");
                return (false, "An error occurred while sending OTP");
            }
        }

        public async Task<(bool success, string message)> VerifyOtpAsync(string email, string otp)
        {
            try
            {
                // Check if OTP exists
                if (!_cache.TryGetValue(email, out OtpData otpData))
                {
                    return (false, "OTP not found or has expired");
                }

                // Check if OTP has expired
                if (DateTime.UtcNow > otpData.ExpiresAt)
                {
                    _cache.Remove(email);
                    return (false, "OTP has expired. Please request a new one");
                }

                // Check attempts
                if (otpData.Attempts >= MAX_ATTEMPTS)
                {
                    _cache.Remove(email);
                    return (false, "Maximum verification attempts exceeded. Please request a new OTP");
                }

                // Verify OTP
                if (otpData.Otp == otp)
                {
                    _cache.Remove(email);
                    _logger.LogInformation($"OTP verified successfully for {email}");
                    return (true, "OTP verified successfully");
                }
                else
                {
                    // Increment attempts
                    otpData.Attempts++;
                    _cache.Set(email, otpData);

                    var attemptsLeft = MAX_ATTEMPTS - otpData.Attempts;
                    return (false, $"Invalid OTP. {attemptsLeft} attempt(s) remaining");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in VerifyOtpAsync: {ex.Message}");
                return (false, "An error occurred while verifying OTP");
            }
        }

        public async Task<(bool success, string message)> ResendOtpAsync(string email)
        {
            // Remove existing OTP
            _cache.Remove(email);
            _cache.Remove($"ratelimit_{email}");

            // Send new OTP
            return await SendOtpAsync(email);
        }

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}

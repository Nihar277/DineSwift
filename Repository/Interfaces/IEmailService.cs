using Repository.Model;

namespace Repository.service
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string otp);
        Task<bool> SendRegistrationEmailAsync(string toEmail, string name, UserType userType, RegisterRequest details = null);
        Task<bool> SendOrderConfirmationEmailAsync(PlaceOrderRequest orderDetails);
        
    }
}

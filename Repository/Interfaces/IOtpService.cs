namespace Repository.service
{
    public interface IOtpService
    {
        Task<(bool success, string message)> SendOtpAsync(string email);
        Task<(bool success, string message)> VerifyOtpAsync(string email, string otp);
        Task<(bool success, string message)> ResendOtpAsync(string email);
        
    }
}
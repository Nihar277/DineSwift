namespace Repository.service
{
    public interface IForgotService
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UpdatePasswordAsync(string email, string newPassword);
        
    }
}

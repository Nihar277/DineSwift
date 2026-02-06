namespace Repository.service;

public interface IPasswordRepository
{
    Task<bool> VerifyCurrentPasswordAsync(int customerId, string currentPassword);
    Task<bool> ChangePasswordAsync(int customerId, string newHashedPassword);
    
}

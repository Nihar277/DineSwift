using Repository.Model;
using System.Threading.Tasks;

namespace API.Services;

public interface IProfileService
{
      Task<bool> UpdateProfile(t_customer customer);
      Task<t_customer?> GetProfileById(int customerId);

       Task<bool> SoftDeleteAccount(int customerId);
       
}
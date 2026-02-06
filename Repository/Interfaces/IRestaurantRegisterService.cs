using Repository.Model;
using System.Threading.Tasks;

namespace Repository.service;

public interface IRestaurantRegisterService
{
    public Task<bool> RegisterVendor(t_customer customer, t_restaurant restaurant);
    Task<bool> IsEmailExists(string email);
    

}
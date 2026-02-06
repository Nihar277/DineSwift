using Repository.Model;
using System.Threading.Tasks;

namespace Repository.service;
using System.Threading.Tasks;
public interface ICustomerService
{
    public Task<bool> GetCustomer(string c_email);

    public Task<bool> Register(t_customer customer);

    public Task<List<t_state>> GetStates();

    public Task<List<t_city>> GetCities(int stateId);
    

}
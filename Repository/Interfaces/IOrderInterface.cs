
using System.Threading.Tasks;
using Repository.Model;

namespace API.service;
public interface IOrderInterface
{
    public Task<int> CreateOrderAsync(t_order order);

    public  Task<int> CancelOrderAsync(int c_bookingid);
    

    

}
using Repository.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IRestaurantRepository
    {
        Task<List<t_vm_restaurant>> GetAllRestaurants();
        Task<t_vm_restaurant> GetRestaurantById(int id);

        
    }
}

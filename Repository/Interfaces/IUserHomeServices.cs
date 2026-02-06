using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Model;


namespace Repository.Interfaces
{
    public interface IUserHomeServices
    {
        public Task<List<t_vm_fooditem>> getAllDishes();

        public Task<List<t_vm_fooditem>> getAllDishesforchat();

        public Task<t_vm_fooditem> getDishInfo(int id);
        public Task<List<t_vm_restaurant>> GetAllRestaurants();
        
        public Task<t_cartaddress> GetAddress(int customerId);

        public Task<bool> AddToCart(t_cart cart);

        public Task<List<t_cart>> GetCartItems(int userid);

        Task<bool> DeleteCartItem(int cartId);
        Task<t_cart?> GetCartItemById(int cartId);
        


          Task<bool> UpdateCartQuantity(int cartId, int quantity);

        Task<t_vm_restaurant> GetRestaurantById(int id);


    }
}
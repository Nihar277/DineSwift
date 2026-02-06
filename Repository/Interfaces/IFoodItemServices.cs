using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Model;

namespace Repository.Service
{
    public interface IFoodItemServices
    {
        public Task<bool> FoodItemAdd(t_fooditem fooditem);
        public Task<List<t_fooditem>> GetAllFoodItemByRestaurant(int id);
        public Task<bool> FoodItemUpdate(t_fooditem fooditem);
        public Task<bool> FoodItemDelete(int id);
        public Task<bool> UpdateAvailability(int id, bool isAvailable);

        Task<List<t_fooditem>> GetAllFoodItem();

        public Task<List<t_fooditem>> GetAllDishes();

    }
}
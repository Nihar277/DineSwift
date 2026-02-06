using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Model;

namespace Repository.service
{
    public interface IElasticSearch
    {
        Task addFoodItems(t_fooditem t_Fooditem);
        Task addRestaurant(t_vm_restaurant restaurant);

        Task CreateFoodItemIndexAsync();
        Task CreateRestaurantIndexAsync();
        Task<List<t_fooditem>> GetFooditemByRestaurantAsync(int restaurantId);
        Task<List<t_fooditem>> SearchFoodItemNameAsync(string searchTerm, int restaurantId);
        Task<List<t_vm_restaurant>> SearchRestaurantNameAsync(string searchTerm);
        Task UpdateFoodItemsPartial(t_fooditem searchTerm);
        Task<List<t_fooditem>> SearchAllFoodItemNameAsync(string searchTerm);

    }
}
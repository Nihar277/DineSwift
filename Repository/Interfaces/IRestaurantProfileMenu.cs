using Repository.Model;
using System.Threading.Tasks;

namespace Repository.service;

public interface IRestaurantProfileMenu
{
    Task<t_restaurant?> GetRestaurantProfile(int customerId);
    Task<bool> UpdateRestaurantProfile(t_restaurant restaurant);

    Task<t_restaurant?> GetRestaurantById(int restaurantId);

    Task<DashboardSummaryDto> GetDashboardSummary(int restaurantId);
    Task<List<RecentOrderDto>> GetRecentOrders(int restaurantId);
    

    Task<List<RestaurantOrderDto>> GetRestaurantOrders(int restaurantId);
    Task<bool> UpdateOrderStatus(int bookingId, string status);


    Task<OrderCardSummaryDto> GetOrderCardSummary(int restaurantId);


    // vendor personal detial update

    Task<t_customer?> GetCustomerProfile(int customerId);
    Task<bool> UpdateCustomerProfile(t_customer customer);
    Task<bool> UpdateCustomerImage(int customerId, string imagePath);

    public Task<List<t_state>> GetStates();

    public Task<List<t_cityy>> GetCities(int stateId);

    Task<bool> UpdatePassword(int customerId, string hashedNewPassword);

    Task<int> GetRestaurantIdByCustomer(int customerId);
    Task<int> GetCustomerIdByRestaurantId(int restaurantId);

    Task<List<DashboardChartDto>> GetDashboardChartData(int restaurantId, int days);
    Task<(int CustomerId, string OrderId)> GetOrderCustomerInfo(int bookingId);
    Task<(int CustomerId, int RestaurantId, string OrderId)> GetOrderInfo(int bookingId);
    Task<bool> UpdateRestaurantAvailability(int customerId, string availability);

}

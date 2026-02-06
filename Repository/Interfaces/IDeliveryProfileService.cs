using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Model;

namespace Repository.Interfaces
{
    public interface IDeliveryProfileService
    {
        Task<List<Repository.Model.RestaurantOrderDto>> GetAvailableOrdersAsync();
        Task<Repository.Model.OrderCardSummaryDto> GetOrderCardSummaryAsync();
        Task<Repository.Model.RestaurantOrderDto?> GetActiveDeliveryOrderAsync(int deliveryPartnerId);
        Task<bool> CanAcceptOrderAsync(int deliveryPartnerId);
        Task<bool> AssignOrderToPartnerAsync(int bookingId, int deliveryPartnerId);

        //get delivery boy details 
        Task<Repository.Model.t_deliverydashboard?> GetDeliveryBoyProfile(int customerId);

        //update delivery boy details 
        Task<bool> UpdateCustomerProfile(t_customer customer);
        Task<bool> UpdateCustomerImage(int customerId, string imagePath);
        Task<bool> UpdatePassword(int customerId, string hashedNewPassword);
        Task<t_customer?> GetCustomerProfile(int customerId);

        // Dashboard methods
        Task<Repository.Model.DashboardSummaryDto> GetDashboardSummaryAsync(int deliveryPartnerId);
        Task<IEnumerable<Repository.Model.DashboardChartDataDto>> GetDashboardChartDataAsync(int deliveryPartnerId, int period);
        Task<IEnumerable<Repository.Model.RecentOrderDto>> GetRecentOrdersAsync(int deliveryPartnerId);
    }
}

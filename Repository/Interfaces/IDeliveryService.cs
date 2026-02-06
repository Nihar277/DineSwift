
using Repository.Model;
namespace Repository.service;

public interface IDeliveryService
{
     Task<int> RegisterDeliveryPartnerAsync(
        t_customer customer,
         DeliveryPartnerForm form
    );

     Task<bool> EmailExistsAsync(string email);

     Task<List<t_state>> GetAllStateAsync();
     Task<List<t_city>> GetAllCityAsync(int id);

     // Update order status (used by delivery partner)
     // Overload: update by bookingId + status (used by API controller)
     Task<bool> UpdateOrderStatusAsync(int bookingId, string status);

     // Legacy overload: update using DeliveryOrderVM
     Task<bool> UpdateOrderStatusAsync(
        DeliveryOrderVM order);
     
     // Accept order - assign to partner
     Task<bool> AcceptOrderAsync(int bookingId, int deliveryPartnerId);

     Task<List<DeliveryDashboardChartDto>> GetDeliveryBoyDashboardChartData(
    int deliveryBoyCustomerId,
    int days);
}

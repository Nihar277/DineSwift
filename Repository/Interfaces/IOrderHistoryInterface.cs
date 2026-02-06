using Repository.Model;

namespace API.service
{
    public interface IOrderHistoryInterface
    {
        // 1️⃣ Get all orders of a customer (Order History)
        Task<List<t_order>> GetAllOrdersAsync(int c_customerid);

        // 2️⃣ Get single order details by Booking ID
        Task<t_order?> GetOrderByBookingIdAsync(int c_bookingid);

        // 3️⃣ Get order progress (status) by Booking ID
        Task<string?> GetOrderProgressAsync(int c_bookingid);
        
    }
}

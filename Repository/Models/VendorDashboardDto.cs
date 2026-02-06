using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Model
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }

        public int TotalClients { get; set; }
        public decimal PlatformFees { get; set; }
        

        public decimal RevenueGrowth { get; set; }
        public decimal OrdersGrowth { get; set; }

        // 🔥 NEW
        public decimal ClientsGrowth { get; set; }
        public decimal PlatformFeesGrowth { get; set; }
    }


    public class DashboardChartDto
    {
        public string Label { get; set; }     // Date / Month
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
        public int Clients { get; set; }
        public decimal PlatformFees { get; set; }
    }

    public class RecentOrderDto
    {
        public string? OrderId { get; set; }
        public string? DishName { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class RestaurantOrderDto
    {
        public int BookingId { get; set; }
        public string OrderId { get; set; }

        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }

        public string DishName { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }

        public string OrderStatus { get; set; }
        public DateTime OrderDateTime { get; set; }

        public string FullAddress { get; set; }
        public string FoodImage { get; set; }
    }



    public class OrderCardSummaryDto
    {
        public int Processing { get; set; }
        public int Prepared { get; set; }
        public int OutForDelivery { get; set; }
        public int Delivered { get; set; }
    }



}

namespace MVC.Models
{
    public class DashboardSummaryDto
    {
        public string RestaurantName { get; set; }
        public string Email { get; set; }

        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }

        // 🔥 NEW

        public int TotalClients { get; set; }
        public decimal PlatformFees { get; set; }

        public decimal RevenueGrowth { get; set; }
        public decimal OrdersGrowth { get; set; }

        // 🔥 NEW
        public decimal ClientsGrowth { get; set; }
        public decimal PlatformFeesGrowth { get; set; }
    }

    public class RecentOrderDto
    {
        public string? OrderId { get; set; }
        public string? DishName { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class DashboardChartDto
    {
        public string Label { get; set; }     // Date / Month
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
        public int Clients { get; set; }
        public decimal PlatformFees { get; set; }
    }

    public class RestaurantProfileDto
    {
        public int? c_r_id { get; set; }
        public string? c_r_name { get; set; }
        public string? c_r_email { get; set; }
        public string? c_r_address { get; set; }
        public string? c_r_state { get; set; }
        public string? c_r_city { get; set; }
        public string? c_r_image { get; set; }
        public string? c_r_isavailable { get; set; }
        public string? c_status { get; set; }
        public int? c_customerid { get; set; }
    }

}

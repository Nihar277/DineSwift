using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Repository.Model
{
    public class t_deliverydashboard
    {
        // ================= DELIVERY BOY PROFILE =================

        [Key]
        public int? c_d_id { get; set; }

        [Required]
        [StringLength(50)]
        public string? c_fname { get; set; }

        [Required]
        [StringLength(50)]
        public string? c_lname { get; set; }

        public string? c_gender { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string? c_email { get; set; }

        public string? c_password { get; set; }
        [Required]
        [RegularExpression(@"^[6-9]\d{9}$")]
        public string? c_phonenumber { get; set; }

        [Required]
        [StringLength(300)]
        public string? c_address { get; set; }

        [Required]
        public string? c_state { get; set; }

        [Required]
        public string? c_city { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$")]
        public string? c_pincode { get; set; }

        // ================= DOCUMENT DETAILS =================

        [Required]
        [StringLength(20)]
        public string? c_license_number { get; set; }

        public string? c_profile_image_path { get; set; }
        public string? c_license_image_path { get; set; }
        public string? c_aadhar_image_path { get; set; }

        // ================= FILE UPLOADS =================


        public IFormFile? c_profile_image { get; set; }


        public IFormFile? c_license_image { get; set; }


        public IFormFile? c_aadhar_image { get; set; }

        // ================= STATUS =================

        public string? c_status { get; set; } = "Prepared";     // Pending / Approved
        public string? c_isavailable { get; set; } = "Yes";    // Yes / No

        public int? c_customerid { get; set; }

        public DateTime c_created_at { get; set; } = DateTime.Now;

        // ================= DASHBOARD ORDERS =================
        // This part is NOT stored in DB

        public List<DeliveryOrderVM> Orders { get; set; } = new();
    }

    // ================= ORDER VIEW MODEL =================

    public class DeliveryOrderVM
    {
        public int? c_customerid { get; set; }
        public int c_order_id { get; set; }
        public string? c_restaurant_name { get; set; }
        public string? c_customer_name { get; set; }
        public string? c_delivery_address { get; set; }
        public decimal c_total_amount { get; set; }
        public string? c_delivery_status { get; set; }
        public DateTime c_order_date { get; set; }
        public int c_bookingid { get; set; }
        public int c_revenue { get; set; }
        public string? c_status { get; set; } = "Prepared";

    }
    public class DeliveryDashboardChartDto
    {
        public string Label { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal DeliveryEarnings { get; set; }
    }

    // ================= DASHBOARD DTOs =================

    public class dashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalClients { get; set; }
        public decimal PlatformFees { get; set; }
        public decimal RevenueGrowth { get; set; }
        public decimal OrdersGrowth { get; set; }
        public decimal ClientsGrowth { get; set; }
    }

    public class DashboardChartDataDto
    {
        public string Label { get; set; }
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
        public decimal PlatformFees { get; set; }
        public int Clients { get; set; }
    }

    public class recentOrderDto
    {
        public int OrderId { get; set; }
        public string? DishName { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string? RestaurantName { get; set; }
        public string? CustomerName { get; set; }
    }
    public class UpdateCustomerImageDto
{
    public int CustomerId { get; set; }
    public string? ImagePath { get; set; }
    public IFormFile? ImageFile { get; set; }
}
}

using System.ComponentModel.DataAnnotations;

namespace Repository.Model
{
    public class SendOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class VerifyOtpRequest
    {
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; }
    }

    public class OtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    public class OtpData
    {
        public string Otp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; }
    }

    // Registration Models
public enum UserType
{
    Customer,
    Restaurant,
    DeliveryPartner
}

public class RegisterRequest
{
    [Required]
    public UserType UserType { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Name { get; set; }

    [Phone]
    public string Phone { get; set; }

    // Restaurant specific
    public string RestaurantName { get; set; }
    public string Address { get; set; }
    
    // Delivery Partner specific
    public string VehicleType { get; set; }
    public string LicenseNumber { get; set; }
}

public class OrderItem
{
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
}

public class PlaceOrderRequest
{
    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; }

    [Required]
    public string CustomerName { get; set; }

    [Required]
    public string OrderId { get; set; }

    [Required]
    public string RestaurantName { get; set; }

    [Required]
    public List<OrderItem> Items { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    [Required]
    public string DeliveryAddress { get; set; }

    // public string CustomerPhone { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public int EstimatedDeliveryMinutes { get; set; } = 30;
}

public class OrderConfirmationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
}
}
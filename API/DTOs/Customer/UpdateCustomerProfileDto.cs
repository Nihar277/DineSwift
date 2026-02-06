using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace API.DTOs.Customer
{
    public class UpdateCustomerProfileDto
    {
        [Required(ErrorMessage = "Customer Id is required")]
        public int c_customerid { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name max 50 characters")]
        public string c_fname { get; set; }
        

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name max 50 characters")]
        public string c_lname { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string c_state { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string c_city { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        public string c_pincode { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string c_gender { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(300, ErrorMessage = "Address max 300 characters")]
        public string c_address { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid Indian phone number")]
        public string c_phonenumber { get; set; }

        // Optional image
        public IFormFile? c_imagefile { get; set; }
    }
}


// DTO Class
public class ChangePasswordDto
{
    public int CustomerId { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}
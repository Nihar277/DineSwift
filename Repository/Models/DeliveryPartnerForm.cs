using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Repository.Model
{
    public class DeliveryPartnerForm
    {
        
        // ================= CUSTOMER =================

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string c_fname { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string c_lname { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
        public string c_state { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string c_city { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        public string c_pincode { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string c_gender { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string c_address { get; set; }

        [Required(ErrorMessage = "Profile image is required")]
        public IFormFile c_imagefile { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string c_email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+=\-])[A-Za-z\d@$!%*?&#^()_+=\-]{8,}$",
            ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character"
        )]
        [StringLength(255)]
        public string c_password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("c_password", ErrorMessage = "Password and Confirm Password do not match")]
        [StringLength(255)]
        public string c_confirmpassword { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid phone number")]
        public string c_phonenumber { get; set; }

        // ================= DELIVERY =================

        [Required(ErrorMessage = "Account number is required")]
        [RegularExpression(@"^\d{9,18}$", ErrorMessage = "Account number must be 9 to 18 digits")]
        public long c_d_ac { get; set; }

        [Required(ErrorMessage = "IFSC code is required")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "IFSC must be 11 characters")]
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC format")]
        public string c_d_ifsc { get; set; }

        [Required(ErrorMessage = "Licence image is required")]
        public IFormFile LicenceImageFile { get; set; }

        [Required(ErrorMessage = "Vehicle image is required")]
        public IFormFile VehicleImageFile { get; set; }

        [Required(ErrorMessage = "Aadhar image is required")]
        public IFormFile AadharImageFile { get; set; }
    }
}

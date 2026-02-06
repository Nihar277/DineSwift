using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Repository.Model;
namespace Repository.Model;

public class t_restaurant
{
    [Key]
    public int? c_r_id { get; set; }

    [Required(ErrorMessage = "Restaurant name is required")]
    [StringLength(200)]
    public string? c_r_name { get; set; }


    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is Invalid Format")]
    [StringLength(50)]
    public string? c_r_email { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(200)]
    public string? c_r_address { get; set; }

    [Required(ErrorMessage = "State is required")]
    public string? c_r_state { get; set; }

    [Required(ErrorMessage = "City is required")]
    public string? c_r_city { get; set; }

    // Bank Account Number
    [Required(ErrorMessage = "Account number is required")]
    [Range(1000000000, 999999999999999999,
        ErrorMessage = "Invalid account number")]
    public long? c_r_ac { get; set; }

    // IFSC (Bank Code)
    [Required(ErrorMessage = "IFSC code is required")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "IFSC code must be exactly 11 characters")]
    [RegularExpression(
        @"^[A-Z]{4}0[A-Z0-9]{6}$",
        ErrorMessage = "Invalid IFSC code (format: ABCD0XXXXXX)"
    )]
    public string? c_r_ifsc { get; set; }


    // GST Number
    [Required(ErrorMessage = "GST number is required")]
    [RegularExpression(
        "^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$",
        ErrorMessage = "Invalid GST number"
    )]
    public string? c_r_gst { get; set; }

    public string? c_r_aadhar { get; set; }

    public IFormFile? c_aadharimage { get; set; }
    public string? c_r_image { get; set; }


    public IFormFile? c_resturantimage { get; set; }
    public string? c_r_isavailable { get; set; }


    public string? c_status { get; set; }


    public int? c_customerid { get; set; }

    public DateTime c_created_at { get; set; } = DateTime.Now;
}

public class VendorRegisterVM
{
    public t_customer Customer { get; set; }
    public t_restaurant Restaurant { get; set; }
}



public class VendorRegisterDto
{
    // ================= CUSTOMER =================

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? c_fname { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? c_lname { get; set; }

    [Required(ErrorMessage = "State is required")]
    [StringLength(50)]
    public string? c_state { get; set; }

    [Required(ErrorMessage = "City is required")]
    [StringLength(50)]
    public string? c_city { get; set; }

    [Required(ErrorMessage = "Pincode is required")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
    public string? c_pincode { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public string c_gender { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500)]
    public string? c_address { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    public string? c_email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6)]
    [RegularExpression(
@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+=\-])[A-Za-z\d@$!%*?&#^()_+=\-]{8,}$",
ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character"
)]
    public string? c_password { get; set; }

    [Required(ErrorMessage = "Confirm Password is required")]
    public string? c_confirmpassword { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid phone number")]
    public string? c_phonenumber { get; set; }

    // ================= RESTAURANT =================

    [Required(ErrorMessage = "Restaurant name is required")]
    [StringLength(200)]
    public string? c_r_name { get; set; }

    [Required(ErrorMessage = "Restaurant email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(50)]
    public string? c_r_email { get; set; }

    [Required(ErrorMessage = "Restaurant address is required")]
    [StringLength(200)]
    public string? c_r_address { get; set; }

    [Required(ErrorMessage = "State is required")]
    public string? c_r_state { get; set; }

    [Required(ErrorMessage = "City is required")]
    public string? c_r_city { get; set; }

    [Required(ErrorMessage = "Account number is required")]
    [Range(1000000000, 999999999999999999)]
    public long? c_r_ac { get; set; }

    [Required(ErrorMessage = "IFSC code is required")]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$")]
    public string? c_r_ifsc { get; set; }

    [Required(ErrorMessage = "GST number is required")]
    [RegularExpression(
        "^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$",
        ErrorMessage = "Invalid GST number")]
    public string? c_r_gst { get; set; }

    // ================= FILES =================

    [Required(ErrorMessage = "Customer image is required")]
    public IFormFile? c_image { get; set; }

    [Required(ErrorMessage = "Restaurant image is required")]
    public IFormFile? c_r_image { get; set; }

    [Required(ErrorMessage = "Aadhar image is required")]
    public IFormFile? c_r_aadhar { get; set; }
}



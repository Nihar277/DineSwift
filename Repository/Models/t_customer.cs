using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;


namespace Repository.Model;

public class t_customer
{
    [Key]
    [Column("c_customerid")]
    public int c_customerid { get; set; }
    

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [Column("c_fname")]
    public string c_fname { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [Column("c_lname")]
    public string c_lname { get; set; }

    [Required(ErrorMessage = "State is required")]
    [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
    [Column("c_state")]
    public string c_state { get; set; }

    [Required(ErrorMessage = "City is required")]
    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
    [Column("c_city")]
    public string c_city { get; set; }

    [Required(ErrorMessage = "Pincode is required")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
    [Column("c_pincode")]
    public string c_pincode { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [Column("c_gender")]
    public string c_gender { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [RegularExpression(@"^[A-Za-z0-9\/\-]{1,10},\s[A-Za-z0-9\s\.\-]{3,50},\s[A-Za-z0-9\s\.\-,]{3,60}$", ErrorMessage = "Address must be in the format: HouseNo, StreetName, City")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    [Column("c_address")]
    public string c_address { get; set; }

    [Column("c_image")]
    public string? c_image { get; set; }

    [Column("c_imagefile")]
    public IFormFile? c_imagefile { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    [Column("c_email")]
    public string c_email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [RegularExpression(
@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+=\-])[A-Za-z\d@$!%*?&#^()_+=\-]{8,}$",
ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character"
)]
    [StringLength(255)]
    [Column("c_password")]
    public string c_password { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [RegularExpression(
@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+=\-])[A-Za-z\d@$!%*?&#^()_+=\-]{8,}$",
ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character"
)]
    [StringLength(255)]
    [Column("c_confirmpassword")]
    public string c_confirmpassword { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid phone number")]
    [Column("c_phonenumber")]
    public string c_phonenumber { get; set; }

    [Column("c_role")]
    public string c_role { get; set; } = "c";

    [Column("c_created_time")]
    public DateTime c_created_time { get; set; } = DateTime.Now;

    [Column("c_updated_time")]
    public DateTime c_updated_time { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Status is required")]
    [Column("c_status")]
    public string c_status { get; set; } = "active";
}

public class t_cityy
{
    [Key]
    [Column("cityid")]
    public int cityid { get; set; }

    [Column("cityname")]
    public string cityname { get; set; }


    [Column("stateid")]
    public int stateid { get; set; }
}


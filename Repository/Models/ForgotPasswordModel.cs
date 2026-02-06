using System;
using System.ComponentModel.DataAnnotations;
namespace Repository.Model;

public class ForgotPasswordRequestModel
{
    [Required]
    [EmailAddress]
    
    public string Email { get; set; }
}

public class ForgotPasswordVerifyModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string OTP { get; set; }

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
}

public class ForgotPasswordUpdateModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
}
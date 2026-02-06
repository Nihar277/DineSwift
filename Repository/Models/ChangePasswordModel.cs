using System.ComponentModel.DataAnnotations;

namespace Repository.Model;

public class ChangePasswordModel
{
    // [Required]
    // public int CustomerId { get; set; }
    

    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and contain 1 uppercase letter, 1 digit, and 1 special character"
    )]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("NewPassword", ErrorMessage = "New password and confirm password do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

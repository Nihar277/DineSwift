using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Model
{
    public class login_vm
    {
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        [Column("c_email")]
        public string? c_email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        // [MinLength(6, ErrorMessage = "Password must be at l
        // east 6 characters")]
    //     [RegularExpression(
    // @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+=\-])[A-Za-z\d@$!%*?&#^()_+=\-]{8,}$",
    // ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character"
    // )]
        [StringLength(255)]
        [Column("c_password")]
        public string? c_password { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Repository.Model;

public class t_promocode
{
    [Key]
    [Column("c_id")]
    public int c_id { get; set; }


    [Column("c_customerid")]
    public int c_customerid { get; set; }


    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    [Column("c_email")]
    public string c_email { get; set; }

    [Required(ErrorMessage = "promocode is required")]
    [MinLength(13, ErrorMessage = "promocode must be at least 6 characters")]
    [StringLength(255)]
    [Column("c_promocode")]
    public string c_promocode { get; set; }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Repository.Model;

public class t_promo
{
    [Key]
    [Column("c_id")]
    public int c_id { get; set; }


    [Required(ErrorMessage = "promocode is required")]
    [MinLength(13, ErrorMessage = "promocode must be at least 6 characters")]
    [StringLength(255)]
    [Column("c_promocode")]
    public string c_promocode { get; set; }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;


namespace Repository.Model
{
    public class t_order
    {
        public int c_bookingid { get; set; }

        [Required]
        
        public string c_orderid { get; set; }

        [Required]
        public int c_customerid { get; set; }

        [Required]
        public int c_restaurantid { get; set; }

        [Required]
        public DateTime c_orderdate { get; set; }

        [Required]
        [StringLength(50)]
        public string c_housenumber { get; set; }

        [Required]
        [StringLength(100)]
        public string c_societyname { get; set; }

        [StringLength(100)]
        public string c_landmark { get; set; }

        public int? c_fooditemid {get; set;}

        [Required]
        [StringLength(50)]
        public string c_city { get; set; }

        [Required]
        [StringLength(50)]
        public string c_state { get; set; }

        [Required]
        [Column(TypeName = "numeric(10,2)")]
        public decimal c_totalprice { get; set; }

        public TimeSpan? c_ordertime { get; set; }

        [Required]
        public int c_quantity { get; set; }

        [Required]
        public string c_dishname { get; set; }

        [StringLength(255)]
        public string c_foodimage { get; set; }

        // varchar / character varying (NOT boolean)
        [Required]
        [StringLength(30)]
        public string c_orderstatus { get; set; }

        // Delivery Partner Assignment
        public int? c_delivery_assigned_to { get; set; }  // Null if not assigned, otherwise delivery partner customer ID

        public IFormFile? c_imageurlPicture {get; set;}
    }
}

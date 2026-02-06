using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Repository.Model
{
    public class t_cart
    {

        [Key]
        public int c_cartId{get;set;}
        
        public int c_customerId { get; set; }
        public int c_itemId { get; set; }
        public int c_restaurantId { get; set; }
        public string c_itemname { get; set; }
        public int c_price { get; set; }
        public int c_quantity { get; set; }
        public string c_image { get; set; }

        public IFormFile? c_imageurlPicture{get; set;}  
    }
}
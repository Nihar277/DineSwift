using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Repository.Model
{
    public class t_vm_fooditem
    {
         [Key]
        public int c_itemid{get; set;}
        public int c_restaurantid{get; set;}
        public string c_name{get; set;}
        public string c_description{get; set;}
        public string c_ingredients{get; set;}
        
        public string c_category{get; set;}
        public string c_imageurl{get; set;}
        public IFormFile? c_imageurlPicture{get; set;}
        public int c_price{get; set;}
        public bool c_isavailable{get; set;}
        public string c_type{get; set;}

        public string c_r_name{get;set;}
  }

  internal class KeyAttribute : Attribute
  {
  }
}
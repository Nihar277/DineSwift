using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Repository.Model
{
    public class t_fooditem
    {
        [Key]
        public int c_itemid { get; set; }
        

        [Required(ErrorMessage = "Restaurant Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Restaurant Id")]
        public int c_restaurantid { get; set; }

        [Required(ErrorMessage = "Food name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Food name must be between 2 and 100 characters")]
        public string c_name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Description must be between 10 and 500 characters")]
        public string c_description { get; set; }

        [Required(ErrorMessage = "Ingredients are required")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "Ingredients must be between 3 and 300 characters")]
        public string c_ingredients { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Category must be between 3 and 50 characters")]
        public string c_category { get; set; }

        public string? c_imageurl { get; set; }
        public IFormFile? c_imageurlPicture { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(1, 100000, ErrorMessage = "Price must be between 1 and 100000")]
        public int c_price { get; set; }

        [Required(ErrorMessage = "Availability status is required")]
        public bool c_isavailable { get; set; }

        [Required(ErrorMessage = "Food type is required")]
        [RegularExpression("Veg|Non-Veg", ErrorMessage = "Food type must be Veg or Non-Veg")]
        public string c_type { get; set; }
    }
}
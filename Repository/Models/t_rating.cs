using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Model
{
    public class t_rating
    {
        [Key]
        public int c_ratingid{get; set;}
        [Required(ErrorMessage = "User is required")]
        public int c_userid { get; set; }

        [Required(ErrorMessage = "Restaurant is required")]
        public int c_restaurantid { get; set; }


        [Required(ErrorMessage = "Food item is required")]
        public int c_fooditemid { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int c_rating { get; set; }

        [Required(ErrorMessage = "Review text is required")]
        [StringLength(500, MinimumLength = 5,
            ErrorMessage = "Review must be between 5 and 500 characters")]
        public string c_reviewtext { get; set; }

        [Required]
        public DateOnly c_date { get; set; }
    }
}
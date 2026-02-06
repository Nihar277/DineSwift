using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Model
{
    public class t_vm_restaurant
    {
        [Key]
        public int c_r_id { get; set; }

        public string c_r_name { get; set; }

        public string c_r_email { get; set; }

        public string c_r_address { get; set; }
        

        public string c_r_state { get; set; }

        public string c_r_city { get; set; }

        public string c_r_image { get; set; }

        public string c_r_isavailable { get; set; }

        public string c_status { get; set; }

        public int c_customerid { get; set; }

        public DateTime c_created_at { get; set; }
    }
}
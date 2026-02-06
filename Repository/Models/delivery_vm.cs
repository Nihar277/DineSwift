using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class delivery_vm
    {
        
        public int c_customerid { get; set; }

        public string c_fname { get; set; }

        public string c_lname { get; set; }

        public string c_state { get; set; }

        public string c_city { get; set; }

        public string c_pincode { get; set; }

        public string c_gender { get; set; }

        public string c_address { get; set; }

        public string? c_image { get; set; }

        public string c_email { get; set; }

        public string c_password { get; set; }

        public string c_confirmpassword { get; set; }

        public string c_phonenumber { get; set; }

        public string c_role { get; set; }

        public DateTime c_created_time { get; set; } 

        public DateTime c_updated_time { get; set; } 

        public string c_status { get; set; }


        
        public int c_d_id { get; set; }
        public long c_d_ac { get; set; }
        public string c_d_ifsc { get; set; }
        public string c_d_licence { get; set; }
        public string c_d_vehicle { get; set; }
        public string c_d_aadhar { get; set; }
        public string c_d_status { get; set; }
        public string c_d_isavailable { get; set; }

    }
}
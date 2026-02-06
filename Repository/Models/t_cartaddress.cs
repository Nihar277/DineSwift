using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Repository.Model
{
    public class t_cartaddress
    {
        public string c_housenumber {get; set;}
        

        public string c_societyname {get; set;}

        public string c_landmark {get; set;}

        public string c_city {get; set;}

        public string c_state {get; set;}
    }
}
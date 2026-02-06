using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class orderStats
    {

        public DateOnly date { get; set; }
        public string day { get; set; }
        public int orders { get; set; }
        public double revenue { get; set; }
        public double avg_order_value { get; set; }

    }
}
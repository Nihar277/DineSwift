using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class TodayStats
    {
        public int todayOrders { get; set; }
        public int totalOrders { get; set; }
        public int todayRevenue { get; set; }
        
        public double totalRevenue { get; set; }
        public int activeUsers { get; set; }
    }
}
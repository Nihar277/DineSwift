using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Model;
using Repository.Models;

namespace Repository.service
{
    public interface IAdminInterface
    {
        public Task<List<t_customer>> getAllCustomers();
        public Task<List<delivery_vm>> getAllDeliveryBoy();
        public Task<List<reastaurant_vm>> getAllRestaurant();
        public Task activeRestaurant(int id);
        public Task inactiveRestaurant(int id);
        public Task activeDeliveryBoy(int id);
        public Task inactiveDeliveryBoy(int id);
        public Task activeCustomer(int id);
        public Task inactiveCustomer(int id);
        public Task<TodayStats> getTodayStats();
        public Task<TodayStats> getRevenueDetails();
        public Task<List<orderStats>> getOrderStats(int days);
        public Task<List<AdminStats>> getAdminStats(int days);
        public Task<List<TodayOrderStatus>> GetTodayOrderStatus();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Repository.service;
using Repository.Model;
using Microsoft.AspNetCore.Authorization;
using Repository.Models;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminInterface _adminService;

        public AdminController(IAdminInterface adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        [Route("getAllCustomer")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllCustomers()
        {

            var customers = await _adminService.getAllCustomers();
            if (customers == null)
            {
                return StatusCode(500, "An error occurred while retrieving customers.");
            }
            return Ok(customers);

        }

        [HttpGet]
        [Route("getAllDeliveryBoy")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllDeliveryBoys()
        {

            var customers = await _adminService.getAllDeliveryBoy();
            if (customers == null)
            {
                return StatusCode(500, "An error occurred while retrieving customers.");
            }
            return Ok(customers);

        }


        [HttpGet]
        [Route("getAllRestaurant")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllRestaurant()
        {

            var customers = await _adminService.getAllRestaurant();
            if (customers == null)
            {
                return StatusCode(500, "An error occurred while retrieving customers.");
            }
            return Ok(customers);

        }

        [HttpGet]
        [Route("activeRestaurant")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> activeRestaurant(int id)
        {
            await _adminService.activeRestaurant(id);
            return Ok();
        }

        [HttpGet]
        [Route("inactiveRestaurant")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> inactiveRestaurant(int id)
        {
            await _adminService.inactiveRestaurant(id);
            return Ok();
        }

        [HttpGet]
        [Route("activeDeliveryBoy")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> activeDeliveryBoy(int id)
        {
            await _adminService.activeDeliveryBoy(id);
            return Ok();
        }

        [HttpGet]
        [Route("inactiveDeliveryBoy")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> inactiveDeliveryBoy(int id)
        {
            await _adminService.inactiveDeliveryBoy(id);
            return Ok();
        }

        [HttpGet]
        [Route("activeCustomer")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> activeCustomer(int id)
        {
            await _adminService.activeCustomer(id);
            return Ok();
        }

        [HttpGet]
        [Route("inactiveCustomer")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> inactiveCustomer(int id)
        {
            await _adminService.inactiveCustomer(id);
            return Ok();
        }

        [HttpGet]
        [Route("getTodayStats")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> getTodayStats()
        {
            TodayStats todayStats = await _adminService.getTodayStats();
            todayStats.totalRevenue = todayStats.totalRevenue * 0.1 + todayStats.totalRevenue + todayStats.totalOrders * 10;
            return Ok(todayStats);
        }


         [HttpGet]
        [Route("getCommissionStats")]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetCommissionStats(int id)
        {
            
            return Ok(await _adminService.getAdminStats(id));
        }

        [HttpGet]
        [Route("getRevenueDetails")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> getRevenueDetails()
        {
            TodayStats todayStats = await _adminService.getRevenueDetails();
            double deliveryCommissions = todayStats.todayRevenue * 0.1;
            int vendorCommissions = todayStats.todayRevenue;
            double platformFee = todayStats.todayOrders * 10;
            double todayRevenue =deliveryCommissions + vendorCommissions + platformFee;

            return Ok(new {
                deliveryCommissions = deliveryCommissions,
                vendorCommissions = vendorCommissions,
                platformFee = platformFee,
                todayRevenue = todayRevenue
            });
        }

        [HttpGet]
        [Route("getOrderStats")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> getOrderStats(int days)
        {
            var orderStats = await _adminService.getOrderStats(days);


            return Ok(orderStats);
        }

        [HttpGet]
        [Route("getTodayOrderStatus")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> getTodayOrderStatus()
        {
            var orderStats = await _adminService.GetTodayOrderStatus();

            return Ok(orderStats);
        }
    }

}
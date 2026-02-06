using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MVC.Controllers
{
    // [Route("[controller]")]
    public class UserHomeController : Controller
    {
        private readonly ILogger<UserHomeController> _logger;

        public UserHomeController(ILogger<UserHomeController> logger)
        {
            _logger = logger;

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "customer")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Cart()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "customer")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult RestaurantDetails(int id)
        {
            ViewBag.RestaurantId = id;
            return View();
        }
        public IActionResult NotificationDetails()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
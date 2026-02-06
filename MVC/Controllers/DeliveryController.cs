using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MVC.Controllers
{
    // [Route("[controller]")]
    public class DeliveryController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public DeliveryController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Orders()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "delivery")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Dashboard()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "delivery")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Profile()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "delivery")
            {
                // The view file is named DeliveryProfile.cshtml, so render it explicitly
                return View("DeliveryProfile");
            }
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}
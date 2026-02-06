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
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Test()
        {
            return View();
        }
        
        public IActionResult SetSession(string role, string token, int customerId, string email, string name)
        {
            Console.WriteLine("Setting session values:");
            // Console.WriteLine($"Role: {role}, Token: {token}, CustomerId: {customerId}, Email: {email}");
            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetString("Token", token);
            HttpContext.Session.SetInt32("CustomerId", Convert.ToInt32(customerId));
            HttpContext.Session.SetString("Email", email);
            HttpContext.Session.SetString("Name", name);
            return Ok(new { message = "Session set successfully" });
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
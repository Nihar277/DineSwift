using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace MVC.Controllers
{
    public class DeliveryDashboardController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public DeliveryDashboardController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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
    }
}



using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MVC.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public RestaurantController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult RestaurantProfileUpdate()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "restaurant")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult VendorProfile()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "restaurant")
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

            if (token != null && role == "restaurant")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ChangePassword()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "restaurant")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> MenuItems()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(token) || role != "restaurant")
                return RedirectToAction("Login", "Auth");

            int customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;
            // if (customerId == 0)
            // return RedirectToAction("Login", "Auth");

            ViewBag.CustomerId = customerId;
            ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
            ViewBag.RestaurantId = 0;

            var apiBase = _configuration["ApiBaseUrl"]?.TrimEnd('/');
            var client = _httpClientFactory.CreateClient();

            // 🔑 ADD TOKEN
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"http://localhost:5245/api/restaurant/id/{customerId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                // 🔥 TRY MULTIPLE POSSIBLE KEYS
                if (doc.RootElement.TryGetProperty("restaurantId", out var rid) ||
                    doc.RootElement.TryGetProperty("c_r_id", out rid) ||
                    doc.RootElement.TryGetProperty("c_rid", out rid))
                {
                    ViewBag.RestaurantId = rid.GetInt32();
                }
            }

            // 🚫 Avoid infinite redirect
            if ((int)ViewBag.RestaurantId == 0)
            {
                TempData["Message"] = "Please complete restaurant profile first";
                return RedirectToAction("RestaurantProfileUpdate");
            }

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Orders()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "restaurant")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }

        // View all restaurants (page only, data comes from API)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ViewAllRestaurants()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
            // Explicitly use the existing view file name: ViewAllRestaurant.cshtml
            return View("ViewAllRestaurant");
        }
        public IActionResult NotificationDetails()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}
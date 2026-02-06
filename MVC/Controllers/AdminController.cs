using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    public class AdminController : Controller
    {

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        
        public IActionResult Customer()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "admin")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Restaurant()
        {

            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "admin")
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

            if (token != null && role == "admin")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Delivery()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "admin")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public IActionResult Deshboard2()
        {
            string token = HttpContext.Session.GetString("Token");
            string role = HttpContext.Session.GetString("Role");

            if (token != null && role == "admin")
            {
                return View();
            }
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult NotificationDetails()
    {
        return View();
    }
    }
}

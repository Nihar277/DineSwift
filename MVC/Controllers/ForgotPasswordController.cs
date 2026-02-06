using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    public class ForgotPasswordController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    public class ProfileController : Controller
    {

        public ProfileController()
        {
        }
        
         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            
            string token = HttpContext.Session.GetString("Token");
            if (token != null)
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

      }  
}

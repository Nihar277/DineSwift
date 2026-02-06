using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Implementation;
using Repository.Model;
using Repository.service;
using Repository.Service;


namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodItemController : ControllerBase
    {
        private readonly IFoodItemServices foodItemServices;
        private readonly IAdminInterface _adminService;

        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public FoodItemController(IFoodItemServices foodItem, IWebHostEnvironment env, IConfiguration config, IAdminInterface adminService)
        {
            foodItemServices = foodItem;
            _env = env;
            _config = config;
            _adminService = adminService;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFoodByRestaurant(int id)
        {
            List<t_fooditem> fooditem = await foodItemServices.GetAllFoodItemByRestaurant(id);
            return Ok(fooditem);
        }

        [HttpPost]
        // [Authorize]
        public async Task<IActionResult> AddFoodItem(t_fooditem fooditem)
        {
            if (fooditem.c_imageurlPicture == null || fooditem.c_imageurlPicture.Length == 0)
                return BadRequest("Food image is required");

            if (ContainsInvalidCharacters(fooditem.c_name))
                return BadRequest("Invalid characters in food name.");

            if (!IsSafeImage(fooditem.c_imageurlPicture))
                return BadRequest("Invalid image type or size (Max 2MB).");

            try
            {
                // // Always prefer MVC wwwroot path for images if configured
                // string webRootPath = _config["MvcWwwRootPath"] ?? _env.WebRootPath;
                // if (string.IsNullOrEmpty(webRootPath))
                // {
                //     webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                // }

                // if (!Directory.Exists(webRootPath))
                // {
                //     Directory.CreateDirectory(webRootPath);
                // }

                // string restaurantFolder = Path.Combine(
                //     webRootPath,
                //     "dishes",
                //     fooditem.c_restaurantid.ToString()
                // );

                // if (!Directory.Exists(restaurantFolder))
                // {
                //     Directory.CreateDirectory(restaurantFolder);
                // }

                // string cleanFoodName = fooditem.c_name
                //     .Trim()
                //     .ToLower()
                //     .Replace(" ", "_")
                //     .Replace("/", "_")
                //     .Replace("\\", "_");

                // string extension = Path.GetExtension(fooditem.c_imageurlPicture.FileName);
                // string fileName = cleanFoodName + extension;

                // string fullImagePath = Path.Combine(restaurantFolder, fileName);

                // using (var stream = new FileStream(fullImagePath, FileMode.Create))
                // {
                //     await fooditem.c_imageurlPicture.CopyToAsync(stream);
                // }
                // fooditem.c_imageurl = fileName;


                // 🔹 MVC wwwroot
                string webRoot = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
                    "MVC", "wwwroot"
                );

                string dishDir = Path.Combine(
                    webRoot,
                    "Images",
                    "restaurant",
                    "dishes",
                    fooditem.c_restaurantid.ToString()
                );

                Directory.CreateDirectory(dishDir);

                string cleanName = fooditem.c_name
                    .Trim()
                    .ToLower()
                    .Replace(" ", "_")
                    .Replace("/", "_")
                    .Replace("\\", "_");

                // string extension = Path.GetExtension(fooditem.c_imageurlPicture.FileName);
                string fileName = $"{cleanName}.jpg";

                string fullPath = Path.Combine(dishDir, fileName);

                using var fs = new FileStream(fullPath, FileMode.Create);
                await fooditem.c_imageurlPicture.CopyToAsync(fs);

                fooditem.c_imageurl = fileName;

                bool status = await foodItemServices.FoodItemAdd(fooditem);

                return Ok(new { status = status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        // [Authorize]
        public async Task<IActionResult> UpdateFoodItem(t_fooditem fooditem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (ContainsInvalidCharacters(fooditem.c_name))
                return BadRequest("Invalid characters in food name.");

            // 🔴 Validate image ONLY if user uploads a new one
            if (fooditem.c_imageurlPicture != null)
            {
                if (!IsSafeImage(fooditem.c_imageurlPicture))
                    return BadRequest("Invalid image type or size (Max 2MB).");
            }

            try
            {
                // string webRootPath = _config["MvcWwwRootPath"] ?? _env.WebRootPath;
                // if (string.IsNullOrEmpty(webRootPath))
                // {
                //     webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                // }

                // // 📌 CASE 1: User uploaded a NEW image
                // if (fooditem.c_imageurlPicture != null && fooditem.c_imageurlPicture.Length > 0)
                // {
                //     string restaurantFolder = Path.Combine(
                //         webRootPath,
                //         "dishes",
                //         fooditem.c_restaurantid.ToString()
                //     );

                //     if (!Directory.Exists(restaurantFolder))
                //         Directory.CreateDirectory(restaurantFolder);

                //     string cleanFoodName = fooditem.c_name
                //         .Trim()
                //         .ToLower()
                //         .Replace(" ", "_")
                //         .Replace("/", "_")
                //         .Replace("\\", "_");

                //     string extension = Path.GetExtension(fooditem.c_imageurlPicture.FileName);
                //     string fileName = $"{cleanFoodName}{extension}";
                //     string fullPath = Path.Combine(restaurantFolder, fileName);

                //     // 🗑️ Delete old image only if exists
                //     if (!string.IsNullOrEmpty(fooditem.c_imageurl))
                //     {
                //         string oldImagePath = Path.Combine(restaurantFolder, fooditem.c_imageurl);
                //         if (System.IO.File.Exists(oldImagePath))
                //         {
                //             System.IO.File.Delete(oldImagePath);
                //         }
                //     }

                //     // 💾 Save new image
                //     using (var stream = new FileStream(fullPath, FileMode.Create))
                //     {
                //         await fooditem.c_imageurlPicture.CopyToAsync(stream);
                //     }

                //     // ✅ Update DB image name
                //     fooditem.c_imageurl = fileName;
                // }
                // // 📌 CASE 2: User did NOT upload new image → keep old image
                // // ❌ Do NOTHING here (very important)
                // // fooditem.c_imageurl remains unchanged

                string webRoot = Path.Combine(
           Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
           "MVC", "wwwroot"
       );

                string dishDir = Path.Combine(
                    webRoot,
                    "Images",
                    "restaurant",
                    "dishes",
                    fooditem.c_restaurantid.ToString()
                );

                Directory.CreateDirectory(dishDir);

                // 🔹 NEW IMAGE UPLOADED
                if (fooditem.c_imageurlPicture != null)
                {
                    if (!IsSafeImage(fooditem.c_imageurlPicture))
                        return BadRequest("Invalid image type or size (Max 2MB).");

                    string cleanName = fooditem.c_name
                        .Trim()
                        .ToLower()
                        .Replace(" ", "_")
                        .Replace("/", "_")
                        .Replace("\\", "_");

                    string extension = Path.GetExtension(fooditem.c_imageurlPicture.FileName);
                    string fileName = $"{cleanName}{extension}";

                    string fullPath = Path.Combine(dishDir, fileName);

                    // 🗑 delete old image
                    if (!string.IsNullOrEmpty(fooditem.c_imageurl))
                    {
                        string oldPath = Path.Combine(dishDir, fooditem.c_imageurl);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    using var fs = new FileStream(fullPath, FileMode.Create);
                    await fooditem.c_imageurlPicture.CopyToAsync(fs);

                    fooditem.c_imageurl = fileName;
                }

                bool status = await foodItemServices.FoodItemUpdate(fooditem);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpDelete]
        // [Authorize]
        public async Task<IActionResult> DeleteFoodItem(int id)
        {
            try
            {
                bool status = await foodItemServices.FoodItemDelete(id);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("availability/{id}")]
        // [Authorize]
        public async Task<IActionResult> UpdateAvailability(int id, [FromQuery] bool isAvailable)
        {
            try
            {
                bool status = await foodItemServices.UpdateAvailability(id, isAvailable);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllRestaurant()
        {

            var customers = await foodItemServices.GetAllDishes();
            if (customers == null)
            {
                return StatusCode(500, "An error occurred while retrieving customers.");
            }
            return Ok(customers);

        }


        [HttpGet("cuisines")]
        [AllowAnonymous]
        public IActionResult GetCuisines()
        {
            var cuisines = new List<string>
            {
                "Indian",
                "Chinese",
                "Italian",
                "Mexican",
                "Thai",
                "Japanese",
                "American",
                "Salad",
                "Mediterranean",
                "French",
                "Korean",
                "Middle Eastern",
                "Greek",
                "Spanish",
                "Vietnamese",
                "Fast Food",
                "Street Food",
                "Continental",
                "Seafood",
                "BBQ",
                "Desserts",
                "Juices",
            };
            return Ok(cuisines);
        }

        private bool ContainsInvalidCharacters(string text)
        {
            return Regex.IsMatch(text, @"[^a-zA-Z0-9\s,.-]");
        }

        private bool IsSafeImage(IFormFile file)
        {
            string[] allowedTypes = { "image/jpeg", "image/png", "image/jpg", "image/webp" };
            return allowedTypes.Contains(file.ContentType) && file.Length <= 2 * 1024 * 1024;
        }
    }
}
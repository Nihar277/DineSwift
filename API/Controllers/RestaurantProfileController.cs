using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Repository.service;
using Repository.Model;
using API.DTOs.Customer;

namespace API.Controllers
{
    [ApiController]
    [Route("api/restaurant")]
    public class RestaurantProfileController : ControllerBase
    {
        private readonly IRestaurantProfileMenu _service;


        public RestaurantProfileController(IRestaurantProfileMenu service)
        {
            _service = service;
        }


        // GET PROFILE
        [HttpGet("id/{customerId}")]
        public async Task<IActionResult> GetProfile(int customerId)
        {
        
            var data = await _service.GetRestaurantProfile(customerId);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // UPDATE PROFILE
        // [HttpPut]
        // public async Task<IActionResult> UpdateProfile([FromForm] t_restaurant restaurant)
        // {
        //     Console.WriteLine(restaurant.c_r_aadhar);
        //     Console.WriteLine(restaurant.c_r_image);
        //     Console.WriteLine(restaurant.c_r_name);
        //     Console.WriteLine(restaurant.c_r_email);
        //     Console.WriteLine(restaurant.c_r_address);
        //     Console.WriteLine(restaurant.c_r_ac);
        //     Console.WriteLine(restaurant.c_r_gst);
        //     Console.WriteLine(restaurant.c_r_id);
        //     Console.WriteLine(restaurant.c_r_state);
        //     if (!ModelState.IsValid)
        //         return BadRequest(ModelState);

        //     var old = await _service.GetRestaurantProfile(restaurant.c_customerid ?? 0);

        //     // Restaurant Image
        //     if (restaurant.c_resturantimage != null)
        //     {
        //         var fileName = restaurant.c_r_name + Path.GetExtension(restaurant.c_resturantimage.FileName);

        //         // 🔥 MOVE OUT FROM API → MVC
        //         var mvcRoot = Path.Combine(
        //             Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
        //             "MVC", "wwwroot", "Images", "restaurant", "restaurantimages"
        //         );

        //         if (!Directory.Exists(mvcRoot))
        //             Directory.CreateDirectory(mvcRoot);

        //         var fullPath = Path.Combine(mvcRoot, fileName);

        //         using var fs = new FileStream(fullPath, FileMode.Create);
        //         await restaurant.c_resturantimage.CopyToAsync(fs);

        //         restaurant.c_r_image = fileName;
        //     }

        //     else
        //     {
        //         restaurant.c_r_image = old?.c_r_image;
        //     }

        //     // Aadhar Image
        //     if (restaurant.c_aadharimage != null)

        //     {
        //         var fileName = restaurant.c_r_email + Path.GetExtension(restaurant.c_aadharimage.FileName);

        //         var mvcRoot = Path.Combine(
        //             Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
        //             "MVC", "wwwroot", "Images", "restaurant", "aadhar"
        //         );

        //         if (!Directory.Exists(mvcRoot))
        //             Directory.CreateDirectory(mvcRoot);

        //         var fullPath = Path.Combine(mvcRoot, fileName);

        //         using var fs = new FileStream(fullPath, FileMode.Create);
        //         await restaurant.c_aadharimage.CopyToAsync(fs);

        //         restaurant.c_r_aadhar = fileName;
        //     }

        //     else
        //     {
        //         restaurant.c_r_aadhar = old?.c_r_aadhar;
        //     }

        //     var result = await _service.UpdateRestaurantProfile(restaurant);
        //     return result ? Ok("Updated") : BadRequest("Failed");
        // }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] t_restaurant restaurant)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            Console.WriteLine(restaurant.c_r_id);
            if (restaurant.c_r_id == null)
                return BadRequest("Restaurant id is missing");

            var old = await _service.GetRestaurantById(restaurant.c_r_id.Value);
            if (old == null)
                return NotFound("Restaurant not found");

            // 🔹 MVC wwwroot
            string webRoot = Path.Combine(
                Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
                "MVC", "wwwroot"
            );

            string restaurantDir = Path.Combine(webRoot, "Images", "restaurant", "restaurantimages");
            string aadharDir = Path.Combine(webRoot, "Images", "restaurant", "aadhar");

            Directory.CreateDirectory(restaurantDir);
            Directory.CreateDirectory(aadharDir);

            // 🔹 SAME NAMING AS REGISTER
            string restaurantImageName = $"{restaurant.c_r_email}.jpg";
            string aadharImageName = $"{restaurant.c_r_email}_ad.jpg";

            // ================= RESTAURANT IMAGE =================
            if (restaurant.c_resturantimage != null)
            {
                using var fs = new FileStream(
                    Path.Combine(restaurantDir, restaurantImageName),
                    FileMode.Create
                );
                await restaurant.c_resturantimage.CopyToAsync(fs);

                restaurant.c_r_image = restaurantImageName;
            }
            else
            {
                restaurant.c_r_image = old.c_r_image; // 🔥 preserve
            }

            // ================= AADHAR IMAGE =================
            if (restaurant.c_aadharimage != null)
            {
                using var fs = new FileStream(
                    Path.Combine(aadharDir, aadharImageName),
                    FileMode.Create
                );
                await restaurant.c_aadharimage.CopyToAsync(fs);

                restaurant.c_r_aadhar = aadharImageName;
            }
            else
            {
                restaurant.c_r_aadhar = old.c_r_aadhar; // 🔥 preserve
            }

            bool result = await _service.UpdateRestaurantProfile(restaurant);
            return result ? Ok("Updated") : BadRequest("Failed");
        }


        [HttpGet("summary/by-customer/{customerId}")]
        public async Task<IActionResult> SummaryByCustomer(int customerId)
        {
            int restaurantId = await _service.GetRestaurantIdByCustomer(customerId);

            if (restaurantId == 0)
                return NotFound("Restaurant not found for this customer");

            return Ok(await _service.GetDashboardSummary(restaurantId));
        }

        [HttpGet("recent-orders/by-customer/{customerId}")]
        public async Task<IActionResult> RecentOrdersByCustomer(int customerId)
        {
            int restaurantId = await _service.GetRestaurantIdByCustomer(customerId);

            if (restaurantId == 0)
                return NotFound("Restaurant not found for this customer");

            return Ok(await _service.GetRecentOrders(restaurantId));
        }


        [HttpGet("dashboard-chart/{customerId}/{range}")]
        public async Task<IActionResult> GetDashboardChart(
            int customerId,
            string range)
        {
            int restaurantId = await _service.GetRestaurantIdByCustomer(customerId);

            // int days = range switch
            // {
            //     "7d" => 7,
            //     "30d" => 30,
            //     "6m" => 180,
            //     "1y" => 365,
            //     _ => 7
            // };

            int days = int.TryParse(range, out int d) ? d : range switch
            {
                "7d" => 7,
                "30d" => 30,
                "6m" => 180,
                "1y" => 365,
                _ => 7
            };

            var data = await _service.GetDashboardChartData(restaurantId, days);
            return Ok(data);
        }


        // ================= CUSTOMER PROFILE ENDPOINTS =================

        [HttpGet("states")]
        public async Task<IActionResult> GetStates()
        {
            var list = await _service.GetStates();
            return Ok(list);
        }

        [HttpGet("cities/{stateId}")]
        public async Task<IActionResult> GetCities(int stateId)
        {
            var list = await _service.GetCities(stateId);
            return Ok(list);
        }
        // GET CUSTOMER PROFILE
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCustomerProfile(int customerId)
        {
            var data = await _service.GetCustomerProfile(customerId);
            if (data == null)
                return NotFound(new { success = false, message = "Customer not found" });

            // Don't return password
            data.c_password = "";
            data.c_confirmpassword = "";

            return Ok(new { success = true, data });
        }

        // UPDATE CUSTOMER PROFILE
        //         [HttpPut("customer")]
        //         public async Task<IActionResult> UpdateCustomerProfile([FromForm] t_customer customer)
        //         {

        //             if (customer.c_customerid <= 0)
        //             {
        //                 return BadRequest(new { success = false, message = "CustomerId missing" });
        //             }
        //  ModelState.Remove("c_email");
        //     ModelState.Remove("customer.c_email");

        //     ModelState.Remove("c_password");
        //     ModelState.Remove("customer.c_password");

        //     ModelState.Remove("c_confirmpassword");
        //     ModelState.Remove("customer.c_confirmpassword");

        //             if (!ModelState.IsValid)
        //                 return BadRequest(new { success = false, message = "Validation failed", errors = ModelState });

        //             try
        //             {
        //                 // Get current customer data
        //                 var currentData = await _service.GetCustomerProfile(customer.c_customerid);
        //                 if (currentData == null)
        //                     return NotFound(new { success = false, message = "Customer not found" });

        //                 // Preserve email from existing data (cannot be changed)
        //                 customer.c_email = currentData.c_email;

        //                 // Handle profile image upload
        //                 if (customer.c_imagefile != null && customer.c_imagefile.Length > 0)
        //                 {
        //                     string webRoot = Path.Combine(
        //                         Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
        //                         "MVC", "wwwroot"
        //                     );

        //                     string customerDir = Path.Combine(webRoot, "Images", "customer");
        //                     Directory.CreateDirectory(customerDir);

        //                     // Generate unique filename
        //                     string fileName = customer.c_email + Path.GetExtension(customer.c_imagefile.FileName);
        //                     string fullPath = Path.Combine(customerDir, fileName);

        //                     // Delete old image if exists
        //                     if (!string.IsNullOrEmpty(currentData.c_image))
        //                     {
        //                         string oldImagePath = Path.Combine(customerDir, currentData.c_image);
        //                         if (System.IO.File.Exists(oldImagePath))
        //                         {
        //                             System.IO.File.Delete(oldImagePath);
        //                         }
        //                     }

        //                     // Save new image
        //                     using (var stream = new FileStream(fullPath, FileMode.Create))
        //                     {
        //                         await customer.c_imagefile.CopyToAsync(stream);
        //                     }

        //                     customer.c_image = fileName;

        //                     // Update image in database
        //                     await _service.UpdateCustomerImage(customer.c_customerid, fileName);
        //                 }
        //                 else
        //                 {
        //                     // Keep existing image
        //                     customer.c_image = currentData.c_image;
        //                 }

        //                 // Update customer profile (without password)
        //                 var result = await _service.UpdateCustomerProfile(customer);

        //                 if (result)
        //                 {
        //                     return Ok(new
        //                     {
        //                         success = true,
        //                         message = "Profile updated successfully",
        //                         image = customer.c_image
        //                     });
        //                 }
        //                 else
        //                 {
        //                     return StatusCode(500, new { success = false, message = "Failed to update profile" });
        //                 }
        //             }
        //             catch (Exception ex)
        //             {
        //                 return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
        //             }
        //         }


        [HttpPut("customer")]
        public async Task<IActionResult> UpdateCustomerProfile(
      [FromForm] UpdateCustomerProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    errors = ModelState
                });

            var currentData = await _service.GetCustomerProfile(dto.c_customerid);
            if (currentData == null)
                return NotFound(new { success = false, message = "Customer not found" });

            // Map DTO → Entity
            currentData.c_fname = dto.c_fname;
            currentData.c_lname = dto.c_lname;
            currentData.c_state = dto.c_state;
            currentData.c_city = dto.c_city;
            currentData.c_pincode = dto.c_pincode;
            currentData.c_gender = dto.c_gender;
            currentData.c_address = dto.c_address;
            currentData.c_phonenumber = dto.c_phonenumber;

            // ================= IMAGE =================
            if (dto.c_imagefile != null)
            {
                string webRoot = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
                    "MVC", "wwwroot", "Images", "customer"
                );

                Directory.CreateDirectory(webRoot);

                string fileName = currentData.c_email + Path.GetExtension(dto.c_imagefile.FileName);
                string path = Path.Combine(webRoot, fileName);

                using var fs = new FileStream(path, FileMode.Create);
                await dto.c_imagefile.CopyToAsync(fs);

                currentData.c_image = fileName;
                await _service.UpdateCustomerImage(dto.c_customerid, fileName);
            }

            bool result = await _service.UpdateCustomerProfile(currentData);

            return result
                ? Ok(new { success = true, message = "Profile updated successfully" })
                : BadRequest(new { success = false, message = "Update failed" });
        }

        // ================= RESTAURANT PROFILE ENDPOINTS (existing) =================


        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {

            var user = await _service.GetCustomerProfile(dto.CustomerId);
            if (user == null) return NotFound(new { success = false, message = "User not found" });


            bool isOldPasswordCorrect = BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.c_password);

            if (!isOldPasswordCorrect)
            {
                return Ok(new { success = false, message = "Old Password is Incorrect" });
            }


            string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            bool result = await _service.UpdatePassword(dto.CustomerId, newHashedPassword);

            return Ok(new { success = result, message = result ? "Password successfully change" : "Not updated" });
        }

        // Toggle restaurant availability (on/off)
        [HttpPut("toggle-availability/{customerId}")]
        public async Task<IActionResult> ToggleAvailability(int customerId, [FromBody] ToggleAvailabilityDto dto)
        {
            if (customerId <= 0)
                return BadRequest(new { success = false, message = "Invalid customer ID" });

            if (string.IsNullOrEmpty(dto?.Availability) || (dto.Availability != "yes" && dto.Availability != "no"))
                return BadRequest(new { success = false, message = "Availability must be 'yes' or 'no'" });

            bool result = await _service.UpdateRestaurantAvailability(customerId, dto.Availability);

            if (result)
            {
                return Ok(new
                {
                    success = true,
                    message = "Restaurant availability updated successfully",
                    isAvailable = dto.Availability == "yes"
                });
            }

            return NotFound(new { success = false, message = "Restaurant not found or update failed" });
        }
    }

    public class ToggleAvailabilityDto
    {
        public string Availability { get; set; }
    }

}
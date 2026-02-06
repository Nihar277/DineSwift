using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API.Services;
using Repository.Model;
using Microsoft.AspNetCore.Hosting;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileApiController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IWebHostEnvironment _env;

        public ProfileApiController(IProfileService profileService, IWebHostEnvironment env)
        {
            _profileService = profileService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _profileService.GetProfileById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] CustomerProfileUpdateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                string imageFilename = null;
                string imageUrl = null;

                if (model.c_imagefile != null && model.c_imagefile.Length > 0)
                {
                    // Determine MVC wwwroot path and filename (follow Restaurant/Delivery pattern)
                    var existingProfile = await _profileService.GetProfileById(model.c_customerid);
                    var emailForName = model.c_email ?? existingProfile?.c_email ?? "user";
                    var filename = emailForName + Path.GetExtension(model.c_imagefile.FileName);

                    var mvcRoot = Path.Combine(
                        Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
                        "MVC", "wwwroot", "Images", "customer"
                    );

                    Directory.CreateDirectory(mvcRoot);
                    var filePath = Path.Combine(mvcRoot, filename);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.c_imagefile.CopyToAsync(stream);
                    }

                    imageFilename = filename;
                    imageUrl = "/Images/customer/" + filename;
                }
                else
                {
                    var existingProfile = await _profileService.GetProfileById(model.c_customerid);
                    imageFilename = existingProfile?.c_image;
                    if (!string.IsNullOrEmpty(imageFilename))
                        imageUrl = "/Images/customer/" + imageFilename;
                }

                var customer = new t_customer
                {
                    c_customerid = model.c_customerid,
                    c_fname = model.c_fname,
                    c_lname = model.c_lname,
                    c_state = model.c_state,
                    c_city = model.c_city,
                    c_pincode = model.c_pincode,
                    c_gender = model.c_gender,
                    c_address = model.c_address,
                    c_phonenumber = model.c_phonenumber,
                    c_email = model.c_email,
                    c_image = imageFilename
                };

                bool updated = await _profileService.UpdateProfile(customer);

                return Ok(updated ? new
                {
                    success = true,
                    message = "Profile updated successfully",
                    c_image = imageFilename,
                    c_image_url = imageUrl
                } : new
                {
                    success = false,
                    message = "Profile update failed"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromForm] int customerId)
        {
            bool result = await _profileService.SoftDeleteAccount(customerId);

            return result ? Ok(new
            {
                success = true,
                message = "Account deactivated successfully"
            }) : BadRequest(new
            {
                success = false,
                message = "Account not found"
            });
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.service;
using Repository.Model;
using BCrypt.Net;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]

// [Authorize] // Will work once JWT is enabled
public class PasswordController : ControllerBase
{
    private readonly IPasswordRepository _passwordRepository;

    public PasswordController(IPasswordRepository passwordRepository)
    {
        _passwordRepository = passwordRepository;
    }

     [HttpPost("change/{customerId}")]
        public async Task<IActionResult> ChangePassword(
            int customerId,
            [FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request"
                });
            }

            Console.WriteLine("CustomerId from URL: " + customerId);

            // ✅ Verify current password
            bool isValid = await _passwordRepository
                .VerifyCurrentPasswordAsync(customerId, model.CurrentPassword);

            Console.WriteLine("Is Valid Password: " + isValid);

            if (!isValid)
            {
                return Ok(new
                {
                    success = false,
                    message = "Current password is incorrect"
                });
            }
            

            // ✅ Hash new password
            string hashedPassword =
                BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // ✅ Update password
            bool updated = await _passwordRepository
                .ChangePasswordAsync(customerId, hashedPassword);

            if (!updated)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to update password"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Password changed successfully"
            });
        }

}

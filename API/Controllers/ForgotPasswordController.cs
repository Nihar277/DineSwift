using Microsoft.AspNetCore.Mvc;
using Repository.service;
using Repository.Model;
using System.Linq;
namespace API.Controllers;

[ApiController]
[Route("api/forgot-password")]
public class ForgotPasswordController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly IForgotService _forgotService;

    public ForgotPasswordController(
        IOtpService otpService,
        IForgotService forgotService)
    {
        _otpService = otpService;
        _forgotService = forgotService;
    }

    // STEP 1: Send OTP
    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp(
        [FromBody] ForgotPasswordRequestModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await _forgotService.EmailExistsAsync(request.Email))
            return BadRequest("Email is not registered");

        var (success, message) =
            await _otpService.SendOtpAsync(request.Email);

        if (!success)
            return BadRequest(message);

        return Ok(new { message = "OTP sent to registered email" });
    }

    // STEP 2: Verify OTP + reset password
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] ForgotPasswordVerifyModel request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (otpValid, otpMessage) =
            await _otpService.VerifyOtpAsync(request.Email, request.OTP);

        if (!otpValid)
            return BadRequest(otpMessage);

        bool updated = await _forgotService.UpdatePasswordAsync(
            request.Email,
            request.NewPassword
        );

        if (!updated)
            return StatusCode(500, "Failed to update password");

        return Ok(new { message = "Password reset successful" });
    }

    // STEP 3: Update password only (OTP already verified in step 2)
    [HttpPost("update-password")]
    public async Task<IActionResult> UpdatePassword(
        [FromBody] ForgotPasswordUpdateModel request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();
            return BadRequest(new { message = string.Join(", ", errors), errors = ModelState });
        }

        // Verify email exists
        if (!await _forgotService.EmailExistsAsync(request.Email))
            return BadRequest(new { message = "Email is not registered" });

        bool updated = await _forgotService.UpdatePasswordAsync(
            request.Email,
            request.NewPassword
        );

        if (!updated)
            return StatusCode(500, new { message = "Failed to update password. Email may not exist." });

        return Ok(new { message = "Password reset successful" });
    }
}
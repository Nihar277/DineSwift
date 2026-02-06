using Microsoft.AspNetCore.Mvc;
using Repository.Model;
using Repository.service;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromodcodeController : ControllerBase
    {
        private readonly IPromocodeService _promocodeService;

        public PromodcodeController(IPromocodeService promocodeService)
        {
            _promocodeService = promocodeService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendPromocode([FromForm] t_promocode dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState
                });
            }
            

            // ✅ 1. Check promo exists
            if (!await _promocodeService.VerifyPromoAsync(dto.c_promocode))
            {
                return Conflict(new
                {
                    success = false,
                    message = "Invalid Promocode"
                });
            }

            // ✅ 2. Check usage limit
            if (!await _promocodeService.VerifyPromocodeAsync(dto.c_promocode))
            {
                return Conflict(new
                {
                    success = false,
                    message = "Promocode usage limit exceeded"
                });
            }

            // ✅ 3. Save usage
            var result = await _promocodeService.SendPromocodeAsync(dto);

            if (!result)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send promocode"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Promocode sent successfully"
            });
        }
    }
}

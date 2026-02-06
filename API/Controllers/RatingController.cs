using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Implementation;
using Repository.Model;
using Repository.Service;

namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingServices ratingServices;
        public RatingController(IRatingServices rating)
        {
            ratingServices = rating;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllRatings()
        {
            try
            {
                var ratings = await ratingServices.GetAllRating();

                if (ratings == null || !ratings.Any())
                    return NoContent(); // 204

                return Ok(ratings); // 200
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        // [Authorize]
        public async Task<IActionResult> AddRating([FromBody] t_rating rating)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // 400

            try
            {
                bool status = await ratingServices.AddRating(rating);

                if (!status)
                    return BadRequest("Unable to add rating");

                return CreatedAtAction(
                    nameof(GetAllRatings),
                    new {success=true, message = "Rating added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
            
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteRating(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // 400

            try
            {
                bool status = await ratingServices.DeleteRating(id);

                if (!status)
                    return BadRequest("Unable to delete rating");

                return CreatedAtAction(
                    nameof(GetAllRatings),
                    new { message = "Rating deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Model;
using Repository.service;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElasticSearchApiController : ControllerBase
    {

        private readonly IElasticSearch _elasticService;

        public ElasticSearchApiController(IElasticSearch elasticService)
        {
            _elasticService = elasticService;
        }

        [HttpGet("searrchFoodByName")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFoodByRestaurant(string value, int id)
        {
            // List<t_fooditem> fooditem = await foodItemServices.GetAllFoodItemByRestaurant(id);
            List<t_fooditem> fooditem = await _elasticService.SearchFoodItemNameAsync(value, id);
            return Ok(fooditem);
        }

        [HttpGet("searrchAllFoodByName")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllFoodByRestaurant(string value)
        {
            // List<t_fooditem> fooditem = await foodItemServices.GetAllFoodItemByRestaurant(id);
            List<t_fooditem> fooditem = await _elasticService.SearchAllFoodItemNameAsync(value);
            return Ok(fooditem);
        }

        [HttpGet("searrchAllRestaurantName")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRestaurant(string value)
        {
            // List<t_fooditem> fooditem = await foodItemServices.GetAllFoodItemByRestaurant(id);
            List<t_vm_restaurant> fooditem = await _elasticService.SearchRestaurantNameAsync(value);
            return Ok(fooditem);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Model;
using Repository.Services;
using API.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class userHomeController : ControllerBase
    {
        private readonly IUserHomeServices _userHome;
        private readonly RabbitMqService _mq;
        private readonly IChatService _chat;

        public userHomeController(IUserHomeServices userHome, RabbitMqService mq, IChatService chat)
        {
            _userHome = userHome;
            _mq = mq;
            _chat = chat;
        }

        [HttpGet]
        [Route("allDishes")]
        public async Task<IActionResult> getAllDishes()
        {

            List<t_vm_fooditem> items = new List<t_vm_fooditem>();

            items = await _userHome.getAllDishes();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> getDishInfo(int id)
        {
            t_vm_fooditem item = await _userHome.getDishInfo(id);
            return Ok(item);
        }

        [HttpGet]
        [Route("allRestaurants")]
        public async Task<IActionResult> GetAllRestaurants()
        {

            List<t_vm_restaurant> restaurants = await _userHome.GetAllRestaurants();
            return Ok(restaurants);
        }

        [HttpGet]
        [Route("GetRestaurantById/{id}")]
        public async Task<IActionResult> GetRestaurantById(int id)
        {
            t_vm_restaurant restaurant = await _userHome.GetRestaurantById(id);
            if (restaurant == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Restaurant not found"
                });
            }
            return Ok(restaurant);
        }


        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromForm] t_cart cart)
        {
            if (cart == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid cart data"
                });
            }

            bool result = await _userHome.AddToCart(cart);

            if (!result)
            {
                return Ok(new
                {
                    success = false,
                    message = "Item already in cart"
                });
            }

            // Send notification to customer when item is added to cart
            if (cart.c_customerId > 0)
            {
                string notificationMessage = $"🛒 {cart.c_itemname} added to cart successfully!";
                string customerKey = $"customer_{cart.c_customerId}";
                
                // Send notification via Redis
                _chat.SendToUser("System", customerKey, notificationMessage);
                
                // Send notification via RabbitMQ
                _mq.SendToUser("System", customerKey, notificationMessage);
            }

            return Ok(new
            {
                success = true,
                message = "Item added to cart"
            });
        }

        [HttpGet("GetCartItems")]
        public async Task<IActionResult> GetCartItems(int userid)
        {
            List<t_cart> cartItems = await _userHome.GetCartItems(userid);
            return Ok(cartItems);
        }

        [HttpGet("GetAddress/{userid}")]
        public async Task<IActionResult> GetCartAddress(int userid)
        {
            try
            {
                var address = await _userHome.GetAddress(userid);

                if (address == null)
                {
                    return NotFound(new { message = "Address not found" });
                }

                return Ok(address);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Controller Error (GetAddress): " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpDelete("DeleteCartItem")]
        public async Task<IActionResult> DeleteCartItem(int cartId)
        {
            // Get cart item details before deleting to send notification
            var cartItem = await _userHome.GetCartItemById(cartId);

            if (cartItem == null)
            {
                return NotFound(new { message = "Cart item not found" });
            }

            bool result = await _userHome.DeleteCartItem(cartId);

            if (result)
            {
                // Send notification to customer when item is removed from cart
                if (cartItem.c_customerId > 0)
                {
                    string notificationMessage = $"🗑️ {cartItem.c_itemname} has been removed from your cart.";
                    string customerKey = $"customer_{cartItem.c_customerId}";
                    
                    // Send notification via Redis
                    _chat.SendToUser("System", customerKey, notificationMessage);
                    
                    // Send notification via RabbitMQ
                    _mq.SendToUser("System", customerKey, notificationMessage);
                }

                return Ok(new { message = "Item removed from cart" });
            }

            return BadRequest(new { message = "Failed to remove item" });
        }


[HttpPost("UpdateCartQuantity")]
public async Task<IActionResult> UpdateCartQuantity([FromBody] UpdateCartQuantityRequest request)
{
    if (request.CartId <= 0 || request.Quantity < 1)
        return BadRequest(new { success = false, message = "Invalid cart or quantity" });

    bool result = await _userHome.UpdateCartQuantity(request.CartId, request.Quantity);

    if (result)
        return Ok(new { success = true, message = "Quantity updated" });

    return BadRequest(new { success = false, message = "Failed to update quantity" });
}

public class UpdateCartQuantityRequest
{
    public int CartId { get; set; }
    public int Quantity { get; set; }
}





    }
}
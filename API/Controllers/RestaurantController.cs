using Microsoft.AspNetCore.Mvc;
using Repository.service;
using Repository.Model;
using Repository.Interfaces;
using System.Text.RegularExpressions;
using API.Services;


namespace API.Controllers
{
    [ApiController]
    [Route("api/restaurant")]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantRegisterService _restaurantService;
        private readonly IEmailService _emailService;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly RabbitMqService _mq;
        private readonly IChatService _chat;



        public RestaurantController(IRestaurantRegisterService restaurantService, IEmailService emailService, RabbitMqService mq, IChatService chat, IRestaurantRepository restaurantRepository)
        {
            _restaurantService = restaurantService;
            _emailService = emailService;
            _restaurantRepository = restaurantRepository;
            _mq = mq;
            _chat = chat;
        }

        [HttpPost("vendor/register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterVendor([FromForm] VendorRegisterDto dto)
        {
            // 🔴 Extra manual validation (optional but safe)
            if (!Regex.IsMatch(dto.c_r_ifsc, @"^[A-Z]{4}0[A-Z0-9]{6}$"))
                ModelState.AddModelError("c_r_ifsc", "Invalid IFSC code");

            if (!Regex.IsMatch(dto.c_r_gst,
                @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$"))
                ModelState.AddModelError("c_r_gst", "Invalid GST number");

            if (dto.c_password != dto.c_confirmpassword)
            {
                ModelState.AddModelError("c_confirmpassword", "Passwords do not match");
            }

            // 🔴 ModelState error response
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }


            // 🔴 Email already exists
            if (await _restaurantService.IsEmailExists(dto.c_email))
            {
                return Conflict(new
                {
                    success = false,
                    message = "Email already exists"
                });
            }

            // 🔹 MVC wwwroot path
            string webRoot = Path.Combine(
                Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
                "MVC",
                "wwwroot"
            );

            // 🔹 Create folders safely
            string customerDir = Path.Combine(webRoot, "Images", "customer");
            string restaurantDir = Path.Combine(webRoot, "Images", "restaurant", "restaurantimages");
            string aadharDir = Path.Combine(webRoot, "Images", "restaurant", "aadhar");

            Directory.CreateDirectory(customerDir);
            Directory.CreateDirectory(restaurantDir);
            Directory.CreateDirectory(aadharDir);

            // 🔹 File names (ONLY NAME stored in DB)
            string customerImageName = $"{dto.c_email}.jpg";
            string restaurantImageName = $"{dto.c_r_email}.jpg";
            string aadharImageName = $"{dto.c_r_email}_ad.jpg";

            // 🔹 Save files
            using (var fs = new FileStream(Path.Combine(customerDir, customerImageName), FileMode.Create))
                await dto.c_image!.CopyToAsync(fs);

            using (var fs = new FileStream(Path.Combine(restaurantDir, restaurantImageName), FileMode.Create))
                await dto.c_r_image!.CopyToAsync(fs);

            using (var fs = new FileStream(Path.Combine(aadharDir, aadharImageName), FileMode.Create))
                await dto.c_r_aadhar!.CopyToAsync(fs);

            // 🔹 Create CUSTOMER
            var customer = new t_customer
            {
                c_fname = dto.c_fname,
                c_lname = dto.c_lname,
                c_state = dto.c_state,
                c_city = dto.c_city,
                c_pincode = dto.c_pincode,
                c_gender = dto.c_gender,
                c_address = dto.c_address,
                c_email = dto.c_email,
                c_password = BCrypt.Net.BCrypt.HashPassword(dto.c_password),
                c_phonenumber = dto.c_phonenumber,
                c_image = customerImageName
            };

            // 🔹 Create RESTAURANT
            var restaurant = new t_restaurant
            {
                c_r_name = dto.c_r_name,
                c_r_email = dto.c_r_email,
                c_r_address = dto.c_r_address,
                c_r_state = dto.c_r_state,
                c_r_city = dto.c_r_city,
                c_r_ac = dto.c_r_ac,
                c_r_ifsc = dto.c_r_ifsc,
                c_r_gst = dto.c_r_gst,
                c_r_image = restaurantImageName,
                c_r_aadhar = aadharImageName
            };

            // 🔹 Register with TRANSACTION
            bool result = await _restaurantService.RegisterVendor(customer, restaurant);

            if (!result)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Vendor registration failed"
                });
            }

            // 🔔 SEND NOTIFICATION HERE
            string message = $"🍽️ New restaurant registered: {dto.c_r_name} ({dto.c_r_email})";

            _mq.SendToUser("System", "admin", message);
            _chat.SendToUser("System", "admin", message);
            var request1 = new RegisterRequest();
            request1.UserType = UserType.Customer;
            var emailSent = await _emailService.SendRegistrationEmailAsync(dto.c_email, dto.c_fname + " " + dto.c_lname, UserType.Customer);

            var request = new RegisterRequest();

            request.UserType = UserType.Restaurant;
            var emailSent1 = await _emailService.SendRegistrationEmailAsync(dto.c_r_email, dto.c_r_name, UserType.Restaurant, request);


            return Ok(new
            {
                success = true,
                message = "Vendor registered successfully"
            });
        }

        // ===============================
        // PUBLIC RESTAURANT QUERIES
        // ===============================

        // GET: api/restaurant/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<t_vm_restaurant>>> GetAllRestaurants()
        {
            var restaurants = await _restaurantRepository.GetAllRestaurants();

            if (restaurants == null || restaurants.Count == 0)
            {
                return NotFound("No restaurants found.");
            }

            return Ok(restaurants);
        }

        // GET: api/restaurant/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<t_vm_restaurant>> GetRestaurantById(int id)
        {
            var restaurant = await _restaurantRepository.GetRestaurantById(id);

            if (restaurant == null)
            {
                return NotFound($"Restaurant with id {id} not found.");
            }

            return Ok(restaurant);

        }
    }
}

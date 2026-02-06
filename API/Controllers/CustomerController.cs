using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System;
using System.IO;
using System.Threading.Tasks;
using Repository.service;
using Repository.Model;
using Repository.Services;
using API.Services;
namespace API.Controllers;



[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IEmailService _emailService;

    private readonly RabbitMqService _mq;
        private readonly IChatService _chat;

    public CustomerController(ICustomerService customerService, IEmailService emailService,RabbitMqService mq, IChatService chat)
    {
        _customerService = customerService;
        _mq = mq;
            _chat = chat;
        _emailService = emailService;

    }


    // ✅ ONLY ONE REGISTER METHOD
    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Register([FromForm] t_customer dto)
    {
        if (await _customerService.GetCustomer(dto.c_email))
        {
            return Conflict(new
            {
                success = false,
                message = "Email already registered"
            });
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Validation failed",
                errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
            });
        }
        string imageFileName = "default.png";

        if (dto.c_imagefile != null && dto.c_imagefile.Length > 0)
        {
            string path = Path.Combine(
           Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
           "MVC",
           "wwwroot",
           "Images/customer"
       );

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            imageFileName = dto.c_email + Path.GetExtension(dto.c_imagefile.FileName);

            using var stream = new FileStream(
                Path.Combine(path, imageFileName),
                FileMode.Create
            );
            await dto.c_imagefile.CopyToAsync(stream);
        }


        dto.c_password = BCrypt.Net.BCrypt.HashPassword(dto.c_password);

        dto.c_image = imageFileName;


        bool result = await _customerService.Register(dto);

        if (!result)
            return StatusCode(500, "Registration failed");


        var request = new RegisterRequest();
        request.UserType = UserType.Customer;
        var emailSent = await _emailService.SendRegistrationEmailAsync(dto.c_email, dto.c_fname +" "+ dto.c_lname, UserType.Customer);
             string message =
            $"New customer registered: {dto.c_fname} {dto.c_lname} ({dto.c_email})";

        _mq.SendToUser("System", "admin", message);
        _chat.SendToUser("System", "admin", message);
      
        return Ok(new
        {
            success = true,
            message = "Registered successfully",
            imageUrl = $"{Request.Scheme}://{Request.Host}/customer_image/{imageFileName}"
        });
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetStates()
    {
        var list = await _customerService.GetStates();
        return Ok(list);
    }

    [HttpGet("cities/{stateId}")]
    public async Task<IActionResult> GetCities(int stateId)
    {
        var list = await _customerService.GetCities(stateId);
        return Ok(list);
    }

    [HttpGet]
    [Route("checkEmailExists")]
    public async Task<IActionResult> checkEmailExists(string email)
    {
        bool check = await _customerService.GetCustomer(email);
        if (check == false)
        {
            return Ok(new { message = "Email does not exist", exists = false });
        }
        else
        {
            return BadRequest(new { message = "Email already exists", exists = true });
        }
    }
}

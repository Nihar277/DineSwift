using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Repository.Service;
using Repository.Model;
using API.Services;


namespace API.controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;
        private readonly RabbitMqService _mq;
        private readonly IChatService _chat;

        public AuthApiController(IAuthService authService, IConfiguration config, RabbitMqService mq, IChatService chat)
        {
            _authService = authService;
            _config = config;
            _mq = mq;
            _chat = chat;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromForm] login_vm login_Vm)
        {
            if (ModelState.IsValid)
            {
                t_customer customer = await _authService.Login(login_Vm);
                string role = "";


                if (customer == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }
                else
                {
                    if (customer.c_role == "a")
                    {
                        role = "admin";
                    }
                    else if (customer.c_role == "c")
                    {
                        role = "customer";
                    }
                    else if (customer.c_role == "d")
                    {
                        role = "delivery";
                    }
                    else
                    {
                        role = "restaurant";
                    }

                    var claims = new[]
                    {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim("CustomerId", customer.c_customerid.ToString()),
                            new Claim("fName", customer.c_fname),
                            new Claim("lName", customer.c_lname),
                            new Claim("email", customer.c_email),
                            new Claim(ClaimTypes.Name, customer.c_email),
                            new Claim(ClaimTypes.NameIdentifier, customer.c_customerid.ToString()),
                            new Claim(ClaimTypes.Role, role)
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(1),
                    signingCredentials: signIn
                    );
                    var message = $"User {customer.c_fname} {customer.c_lname} with role {role} has logged in.";
                    _mq.SendToUser("System", "admin", message);
                    _chat.SendToUser("System", "admin", message);
                    return Ok(new { message = "Login Succesfully", userData = customer, role = role, token = new JwtSecurityTokenHandler().WriteToken(token) });
                }
            }
            else
            {
                return BadRequest(new { message = "Invalid Model" });
            }
        }

        [HttpGet]
        [Authorize]
        [Route("CheckAuth")]
        public async Task<IActionResult> getstring()
        {
            return Ok("Welcome string");
        }

        [HttpGet]
        [Route("checkEmailExists")]
        public async Task<IActionResult> checkEmailExists(string email)
        {
            int check = await _authService.checkEmailExists(email);
            if (check == 0)
            {
                return Ok(new { message = "Email does not exist", exists = false });
            }   
            else
            {
                return BadRequest(new { message = "Email already exists", exists = true });
            }
        }
    }
}
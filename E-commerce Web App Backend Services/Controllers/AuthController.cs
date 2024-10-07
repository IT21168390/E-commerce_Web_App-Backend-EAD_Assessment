using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace E_commerce_Web_App_Backend_Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDTO loginDTO)
        {
            try
            {
                var token = _authService.Authenticate(loginDTO);
                if (token == null)
                {
                    return Unauthorized(new { Message = "Invalid email or password." });
                }
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred during login.", Details = ex.Message });
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterDTO registerDTO)
        {
            try
            {
                // Validate input
                if (registerDTO == null)
                {
                    return BadRequest(new { Message = "Invalid user data." });
                }

                // Register the user
                var user = _authService.Register(registerDTO);
                if (user == null)
                {
                    return BadRequest(new { Message = "User registration failed." });
                }

                return Ok(new { Message = "User registered successfully.", User = user });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred during registration.", Details = ex.Message });
            }
        }
    }
}

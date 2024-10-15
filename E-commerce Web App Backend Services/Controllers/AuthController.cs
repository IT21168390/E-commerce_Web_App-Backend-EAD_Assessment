using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace E_commerce_Web_App_Backend_Services.Controllers
{

    /// <summary>
    /// Handles authentication operations including user login and registration.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;


        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service used for login and registration.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        /// <summary>
        /// Authenticates a user and returns a JWT token upon successful login.
        /// </summary>
        /// <param name="loginDTO">The user login data transfer object containing email and password.</param>
        /// <returns>An IActionResult containing the JWT token or an unauthorized message.</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDTO loginDTO)
        {
            try
            {
                AuthResponseDTO authResult = _authService.Authenticate(loginDTO);
                if (authResult == null)
                {
                    return Unauthorized(new { Message = "Invalid email or password." });
                }
                // Return the token, userId, and name
                return Ok(new { Token = authResult.Token, userId = authResult.UserId, name = authResult.UserName });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred during login.", Details = ex.Message });
            }
        }



        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="registerDTO">The user registration data transfer object containing user details.</param>
        /// <returns>An IActionResult indicating the success or failure of the registration.</returns>
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

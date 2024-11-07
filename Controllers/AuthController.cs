using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Top10MediaApi.Models.Users;
using Top10MediaApi.Services;
using System;
using System.Threading.Tasks;
using Serilog;

namespace Top10MediaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                Log.Information("Registering user with username: {@Username}", registerDto.Username);

                var user = await _userService.RegisterUserAsync(registerDto.Username, registerDto.Password, registerDto.Email);
                return Ok(new { user.Id, user.Username });
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Validation failed during registration for username: {@Username}", registerDto.Username);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred while registering user with username: {@Username}", registerDto.Username);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                Log.Information("User attempting login with username: {@Username}", loginDto.Username);

                var user = await _userService.ValidateUserAsync(loginDto.Username, loginDto.Password);
                if (user == null)
                {
                    Log.Warning("Invalid login attempt for username: {@Username}", loginDto.Username);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                Log.Information("User {@Username} logged in successfully", user.Username);
                // Here, you would generate and return a JWT (this will be covered in the next steps)
                return Ok(new { message = "Login successful" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred during login for username: {@Username}", loginDto.Username);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
    }
}

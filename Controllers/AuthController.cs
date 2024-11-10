using Microsoft.AspNetCore.Mvc;
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
        private readonly JWTGenerator _jwtGenerator;

        public AuthController(UserService userService, JWTGenerator jwtGenerator)
        {
            _userService = userService;
            _jwtGenerator = jwtGenerator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data" });
            }

            try
            {
                Log.Information("Registering user");

                var user = await _userService.RegisterUserAsync(registerDto.Username, registerDto.Password, registerDto.Email);
                return Ok(new { user.Id, user.Username });
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation failed during registration: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected error while registering user: {Message}", ex.Message);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data" });
            }

            try
            {
                Log.Information("User attempting login");

                var user = await _userService.ValidateUserAsync(loginDto.Username, loginDto.Password);
                if (user == null)
                {
                    Log.Warning("Invalid login attempt");
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                var token = _jwtGenerator.GenerateJwtToken(user.Id.ToString());
                return Ok(new { message = "Login successful", token });
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected error during login: {Message}", ex.Message);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
    }
}

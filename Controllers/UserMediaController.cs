using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Top10MediaApi.Models.Users;
using Top10MediaApi.Services;
using System.Security.Claims;
using Serilog;

namespace Top10MediaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensure users are authenticated before accessing these endpoints
    public class UserMediaController : ControllerBase
    {
        private readonly UserMediaService _userMediaService;

        public UserMediaController(UserMediaService userMediaService)
        {
            _userMediaService = userMediaService;
        }

        // Helper method to get the userId from the JWT claims (if using JWT)
        private int GetUserIdFromClaims()
        {
            // Retrieve the user ID from the claims. Adjust as per your claim settings.
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddMediaToUserList([FromBody] UserMediaDto mediaDto)
        {
            if (mediaDto == null || mediaDto.MediaId <= 0)
            {
                return BadRequest(new { message = "Invalid media data provided" });
            }

            int userId = GetUserIdFromClaims();
            try
            {
                await _userMediaService.AddMediaToUserListAsync(userId, mediaDto.MediaId, mediaDto.MediaType);
                Log.Information($"User with ID {userId} added media item with ID {mediaDto.MediaId} to their list."); // Log successful addition
                return Ok(new { message = "Media item added to user's list" });
            }
            catch (InvalidOperationException ex)
            {
                // Specific handling for media not existing in main list
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while adding media to user {userId}'s list."); // Log error details
                return StatusCode(500, new { message = "An error occurred while adding media to the list" });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetUserMediaList()
        {
            try
            {
                int userId = GetUserIdFromClaims();
                var mediaList = await _userMediaService.GetUserMediaListAsync(userId);

                return Ok(mediaList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the media list" });
            }
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveMediaFromUserList([FromBody] UserMediaDto mediaDto)
        {
            if (mediaDto == null || mediaDto.MediaId <= 0)
                return BadRequest(new { message = "Invalid media data provided" });

            try
            {
                int userId = GetUserIdFromClaims();
                await _userMediaService.RemoveMediaFromUserListAsync(userId, mediaDto.MediaId, mediaDto.MediaType);

                return Ok(new { message = "Media item removed from user's list" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing the media from the list" });
            }
        }
    }
}

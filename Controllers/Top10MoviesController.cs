using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Top10MediaApi.Services;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class Top10MoviesController : ControllerBase
{
    private readonly TmdbService _tmdbService;
    private readonly MoviesService _moviesService;
    private readonly ILogger<Top10MoviesController> _logger;

    public Top10MoviesController(TmdbService tmdbService, MoviesService moviesService, ILogger<Top10MoviesController> logger)
    {
        _logger = logger;
        _tmdbService = tmdbService;
        _moviesService = moviesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTop10Movies()
    {
        try
        {
            var top10Movies = await _tmdbService.GetTop10MoviesAsync();
            _logger.LogCritical("Successfully fetched top 10 movies: {MovieTitles}", top10Movies.Select(m => m.Title)); 
            return Ok(top10Movies);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "An error occurred while fetching top 10 movies.");
            return StatusCode(500, $"Error: {e.Message}");
        }
    }

    [HttpPost("reset-top10-movies")]
    public async Task<IActionResult> ResetTop10Movies()
    {
        _logger.LogInformation("Starting reset of top 10 movies.");

        try
        {
            await _moviesService.ClearMoviesAsync();
            _logger.LogInformation("Cleared old top 10 movies from the database.");

            var movies = await _tmdbService.GetTop10MoviesAsync();
            _logger.LogInformation("Fetched new top 10 movies for reset.");

            await _moviesService.SaveMoviesAsync(movies);
            _logger.LogInformation("Saved new top 10 movies to the database.");

            return Ok("Top 10 movies have been updated in the database.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while resetting top 10 movies.");
            return StatusCode(500, "An error occurred while resetting top 10 movies.");
        }
    }
}

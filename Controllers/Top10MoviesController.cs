using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using Top10MediaApi.Services;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class Top10MoviesController : ControllerBase
{
    private readonly TmdbService _tmdbService;
    private readonly MoviesService _moviesService;

    public Top10MoviesController(TmdbService tmdbService, MoviesService moviesService)
    {
        _tmdbService = tmdbService;
        _moviesService = moviesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTop10Movies()
    {
        try
        {
            var top10Movies = await _tmdbService.GetTop10MoviesAsync();
            Log.Information("Successfully fetched top 10 movies: {@MovieTitles}", top10Movies.Select(m => m.Title));
            return Ok(top10Movies);
        }
        catch (HttpRequestException e)
        {
            Log.Error(e, "An error occurred while fetching top 10 movies.");
            return StatusCode(500, $"Error: {e.Message}");
        }
    }

    [HttpPost("reset-top10-movies")]
    public async Task<IActionResult> ResetTop10Movies()
    {
        Log.Information("Starting reset of top 10 movies.");

        try
        {
            await _moviesService.ClearMoviesAsync();
            Log.Information("Cleared old top 10 movies from the database.");

            var movies = await _tmdbService.GetTop10MoviesAsync();
            Log.Information("Fetched new top 10 movies for reset.");

            await _moviesService.SaveMoviesAsync(movies);
            Log.Information("Saved new top 10 movies to the database.");

            return Ok("Top 10 movies have been updated in the database.");
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while resetting top 10 movies.");
            return StatusCode(500, "An error occurred while resetting top 10 movies.");
        }
    }
}

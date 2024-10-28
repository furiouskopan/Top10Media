using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            return Ok(top10Movies);
        }
        catch (HttpRequestException e)
        {
            return StatusCode(500, $"Error: {e.Message}");
        }
    }

    [HttpPost("reset-top10-movies")]
    public async Task<IActionResult> ResetTop10Movies()
    {
        await _moviesService.ClearMoviesAsync();
        var movies = await _tmdbService.GetTop10MoviesAsync();
        await _moviesService.SaveMoviesAsync(movies);

        return Ok("Top 10 movies have been updated in the database.");
    }
}

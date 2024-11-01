using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Top10MediaApi.Services;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class Top10TvShowsController : ControllerBase
{
    private readonly TmdbService _tmdbService;
    private readonly TvShowsService _tvShowsService;
    private readonly ILogger<Top10TvShowsController> _logger;


    public Top10TvShowsController(TmdbService tmdbService, TvShowsService tvShowsService, ILogger<Top10TvShowsController> logger)
    {
        _tmdbService = tmdbService;
        _tvShowsService = tvShowsService;
        _logger = logger;   
    }

    [HttpGet]
    public async Task<IActionResult> GetTop10TvShows()
    {
        _logger.LogInformation("Starting to fetch top 10 TV shows.");

        try
        {
            var top10TvShows = await _tmdbService.GetTop10TvShowsAsync();
            _logger.LogInformation("Successfully fetched top 10 TV shows: {TvShowTitles}", top10TvShows.Select(tv => tv.Title));
            return Ok(top10TvShows);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "An error occurred while fetching top 10 TV shows.");
            return StatusCode(500, $"Error: {e.Message}");
        }
    }

    [HttpPost("reset-top10-tvshows")]
    public async Task<IActionResult> ResetTop10TvShows()
    {
        _logger.LogInformation("Starting reset of top 10 TV shows.");

        try
        {
            await _tvShowsService.ClearTvShowsAsync();
            _logger.LogInformation("Cleared old top 10 TV shows from the database.");

            var tvShows = await _tmdbService.GetTop10TvShowsAsync();
            _logger.LogInformation("Fetched new top 10 TV shows for reset.");

            await _tvShowsService.SaveTvShowsAsync(tvShows);
            _logger.LogInformation("Saved new top 10 TV shows to the database.");

            return Ok("Top 10 TV shows have been updated in the database.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while resetting top 10 TV shows.");
            return StatusCode(500, "An error occurred while resetting top 10 TV shows.");
        }
    }
}

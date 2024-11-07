using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Top10MediaApi.Services;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class Top10TvShowsController : ControllerBase
{
    private readonly TmdbService _tmdbService;
    private readonly TvShowsService _tvShowsService;
    private readonly ILogger<Top10TvShowsController> _logger;


    public Top10TvShowsController(TmdbService tmdbService, TvShowsService tvShowsService)
    {
        _tmdbService = tmdbService;
        _tvShowsService = tvShowsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTop10TvShows()
    {
        Log.Information("Starting to fetch top 10 TV shows.");

        try
        {
            var top10TvShows = await _tmdbService.GetTop10TvShowsAsync();
            Log.Information("Successfully fetched top 10 TV shows: {TvShowTitles}", top10TvShows.Select(tv => tv.Title));
            return Ok(top10TvShows);
        }
        catch (HttpRequestException e)
        {
            Log.Error(e, "An error occurred while fetching top 10 TV shows.");
            return StatusCode(500, $"Error: {e.Message}");
        }
    }

    [HttpPost("reset-top10-tvshows")]
    public async Task<IActionResult> ResetTop10TvShows()
    {
        Log.Information("Starting reset of top 10 TV shows.");

        try
        {
            await _tvShowsService.ClearTvShowsAsync();
            Log.Information("Cleared old top 10 TV shows from the database.");

            var tvShows = await _tmdbService.GetTop10TvShowsAsync();
            Log.Information("Fetched new top 10 TV shows for reset.");

            await _tvShowsService.SaveTvShowsAsync(tvShows);
            Log.Information("Saved new top 10 TV shows to the database.");

            return Ok("Top 10 TV shows have been updated in the database.");
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while resetting top 10 TV shows.");
            return StatusCode(500, "An error occurred while resetting top 10 TV shows.");
        }
    }
}

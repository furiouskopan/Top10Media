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

    public Top10TvShowsController(TmdbService tmdbService, TvShowsService tvShowsService)
    {
        _tmdbService = tmdbService;
        _tvShowsService = tvShowsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTop10TvShows()
    {
        try
        {
            var top10TvShows = await _tmdbService.GetTop10TvShowsAsync();
            return Ok(top10TvShows);
        }
        catch (HttpRequestException e)
        {
            return StatusCode(500, $"Error: {e.Message}");
        }
    }

    [HttpPost("reset-top10-tvshows")]
    public async Task<IActionResult> ResetTop10TvShows()
    {
        await _tvShowsService.ClearTvShowsAsync();
        var tvShows = await _tmdbService.GetTop10TvShowsAsync();
        await _tvShowsService.SaveTvShowsAsync(tvShows);

        return Ok("Top 10 TV shows have been updated in the database.");
    }
}

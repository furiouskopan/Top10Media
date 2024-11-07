using Microsoft.AspNetCore.Mvc;
using Top10MediaApi.Services;
using Microsoft.Extensions.Logging;
using Serilog;

[ApiController]
[Route("api/[controller]")]
public class Top10GamesController : ControllerBase
{
    private readonly RawgService _rawgService;
    private readonly GamesService _gamesService;

    public Top10GamesController(RawgService rawgService, GamesService gamesService)
    {
        _rawgService = rawgService;
        _gamesService = gamesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTop10Games()
    {
        Log.Information("Starting to fetch top 10 games.");

        try
        {
            var top10Games = await _rawgService.GetTop10GamesAsync();
            Log.Information("Successfully fetched top 10 games: {@GameTitles}", top10Games.Select(game => game.Title));
            return Ok(top10Games);
        }
        catch (HttpRequestException e)
        {
            Log.Error(e, "An error occurred while fetching top 10 games.");
            return StatusCode(500, $"Error: {e.Message}");
        }
    }

    [HttpPost("reset-top10-games")]
    public async Task<IActionResult> ResetTop10Games()
    {
        Log.Information("Starting reset of top 10 games.");

        try
        {
            // Clear previous top 10 games (assuming ClearGamesAsync exists in RawgService)
            await _gamesService.ClearGamesAsync();
            Log.Information("Cleared old top 10 games from the database.");

            // Fetch and save new top 10 games
            var games = await _rawgService.GetTop10GamesAsync();
            Log.Information("Fetched new top 10 games for reset.");

            await _gamesService.SaveGamesAsync(games);
            Log.Information("Saved new top 10 games to the database.");

            return Ok("Top 10 games have been updated in the database.");
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while resetting top 10 games.");
            return StatusCode(500, "An error occurred while resetting top 10 games.");
        }
    }
}
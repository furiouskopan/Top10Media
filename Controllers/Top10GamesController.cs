using Microsoft.AspNetCore.Mvc;
using Top10MediaApi.Services;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class Top10GamesController : ControllerBase
{
    private readonly RawgService _rawgService;
    private readonly GamesService _gamesService;
    private readonly ILogger<Top10GamesController> _logger;

    public Top10GamesController(RawgService rawgService, GamesService gamesService, ILogger<Top10GamesController> logger)
    {
        _rawgService = rawgService;
        _logger = logger;
        _gamesService = gamesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTop10Games()
    {
        _logger.LogInformation("Starting to fetch top 10 games.");

        try
        {
            var top10Games = await _rawgService.GetTop10GamesAsync();
            _logger.LogInformation("Successfully fetched top 10 games: {GameTitles}", top10Games.Select(game => game.Title));
            return Ok(top10Games);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "An error occurred while fetching top 10 games.");
            return StatusCode(500, $"Error: {e.Message}");
        }
    }

    [HttpPost("reset-top10-games")]
    public async Task<IActionResult> ResetTop10Games()
    {
        _logger.LogInformation("Starting reset of top 10 games.");

        try
        {
            // Clear previous top 10 games (assuming ClearGamesAsync exists in RawgService)
            await _gamesService.ClearGamesAsync();
            _logger.LogInformation("Cleared old top 10 games from the database.");

            // Fetch and save new top 10 games
            var games = await _rawgService.GetTop10GamesAsync();
            _logger.LogInformation("Fetched new top 10 games for reset.");

            await _gamesService.SaveGamesAsync(games);
            _logger.LogInformation("Saved new top 10 games to the database.");

            return Ok("Top 10 games have been updated in the database.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while resetting top 10 games.");
            return StatusCode(500, "An error occurred while resetting top 10 games.");
        }
    }
}

using Microsoft.Extensions.Logging;
using System.Text.Json;
using Top10MediaApi.Models;

namespace Top10MediaApi.Services
{
    public class RawgService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _rawgApiKey;
        private readonly ILogger<RawgService> _logger;

        public RawgService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RawgService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _rawgApiKey = configuration["RawgApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "RAWGApiKey is not configured.");
            _logger = logger;
        }

        public async Task<List<GameDTO>> GetTop10GamesAsync()
        {
            _logger.LogInformation("Fetching top 10 popular games from RAWG.");

            //var currentMonth = DateTime.UtcNow.ToString("yyyy-MM");
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-30);
            var rawgUrl = $"https://api.rawg.io/api/games?key={_rawgApiKey}&dates={startDate:yyyy-MM-dd},{endDate:yyyy-MM-dd}&ordering=-added&page_size=10";

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(rawgUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch games from RAWG. Status code: {StatusCode}", response.StatusCode);
                throw new HttpRequestException("Error fetching data from RAWG");
            }

            _logger.LogInformation("Successfully fetched game data from RAWG.");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var gameData = JsonDocument.Parse(jsonResponse);

            var games = new List<GameDTO>();

            foreach (var game in gameData.RootElement.GetProperty("results").EnumerateArray())
            {
                var title = game.GetProperty("name").GetString() ?? "Unknown Title";
                var releaseDate = DateTime.TryParse(game.GetProperty("released").GetString(), out var date)
                    ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                    : (DateTime?)null;
                var popularity = game.TryGetProperty("added", out var addedProp) ? (double)addedProp.GetInt32()
                        : (game.TryGetProperty("metacritic", out var metacriticProp) ? metacriticProp.GetDouble() : 0.0);
                var genres = game.GetProperty("genres").EnumerateArray()
                    .Select(genre => genre.GetProperty("name").GetString())
                    .ToList();

                // Fetch detailed game description if not present in the main response
                var overview = "No Description";
                if (!game.TryGetProperty("description_raw", out var descProp))
                {
                    var gameId = game.GetProperty("id").GetInt32();
                    var detailUrl = $"https://api.rawg.io/api/games/{gameId}?key={_rawgApiKey}";
                    var detailResponse = await client.GetAsync(detailUrl);

                    if (detailResponse.IsSuccessStatusCode)
                    {
                        var detailJsonResponse = await detailResponse.Content.ReadAsStringAsync();
                        var gameDetailData = JsonDocument.Parse(detailJsonResponse);
                        overview = gameDetailData.RootElement.TryGetProperty("description_raw", out var descDetailProp)
                                   ? descDetailProp.GetString() ?? "No Description"
                                   : "No Description";
                    }
                    else
                    {
                        _logger.LogWarning("Failed to fetch detailed description for game: {Title}", title);
                    }
                }
                else
                {
                    overview = descProp.GetString();
                }

                games.Add(new GameDTO
                {
                    Title = title,
                    Overview = overview,
                    ReleaseDate = releaseDate,
                    Popularity = popularity,
                    Genres = genres
                });
            }

            _logger.LogInformation("Successfully parsed {Count} games from RAWG response.", games.Count);

            return games;
        }
    }
}

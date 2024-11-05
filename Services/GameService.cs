using Top10MediaApi.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Top10MediaApi.Services
{
    public class GamesService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GamesService> _logger;

        public GamesService(AppDbContext context, ILogger<GamesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SaveGamesAsync(List<GameDTO> games)
        {
            _logger.LogInformation("Saving games to the database.");

            foreach (var gameDto in games)
            {
                var dbGame = new Game
                {
                    Title = gameDto.Title,
                    Overview = gameDto.Overview,
                    ReleaseDate = gameDto.ReleaseDate.HasValue
                      ? DateTime.SpecifyKind(gameDto.ReleaseDate.Value, DateTimeKind.Utc)
                      : DateTime.UtcNow,
                    Popularity = gameDto.Popularity,
                    Genres = gameDto.Genres // Directly assign genres
                };

                _context.Games.Add(dbGame);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Games have been successfully saved.");
        }
        public async Task ClearGamesAsync()
        {
            _context.Games.RemoveRange(_context.Games);
            await _context.SaveChangesAsync();
            _logger.LogInformation("All games have been cleared from the database.");
        }
    }
}

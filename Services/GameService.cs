using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Top10MediaApi.Models.Games;

namespace Top10MediaApi.Services
{
    public class GamesService
    {
        private readonly AppDbContext _context;

        public GamesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveGamesAsync(List<GameDTO> games)
        {
            Log.Information("Saving games to the database.");

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
            Log.Information("Games have been successfully saved.");
        }
        public async Task ClearGamesAsync()
        {
            _context.Games.RemoveRange(_context.Games);
            await _context.SaveChangesAsync();
            Log.Information("All games have been cleared from the database.");
        }
    }
}

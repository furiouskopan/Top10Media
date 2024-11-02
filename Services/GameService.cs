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

            var dbGames = games.Select(g => new Game
            {
                Title = g.Title,
                Overview = g.Overview,
                ReleaseDate = g.ReleaseDate ?? DateTime.MinValue, // Provide a default value for null dates
                Popularity = g.Popularity,
                GameGenres = g.Genres.Select(genreName => new GameGenre
                {
                    Genre = GetOrAddGenre(genreName)
                }).ToList()
            }).ToList();

            _context.Games.AddRange(dbGames);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Games have been successfully saved.");
        }

        public async Task ClearGamesAsync()
        {
            _context.Games.RemoveRange(_context.Games);
            await _context.SaveChangesAsync();
            _logger.LogInformation("All games have been cleared from the database.");
        }

        private Genre GetOrAddGenre(string genreName)
        {
            var genre = _context.Genres.SingleOrDefault(g => g.Name == genreName);
            if (genre == null)
            {
                genre = new Genre { Name = genreName };
                _context.Genres.Add(genre);
            }
            return genre;
        }
    }
}

using Top10MediaApi.Models;
using Serilog;

namespace Top10MediaApi.Services
{
    public class TvShowsService
    {
        private readonly AppDbContext _context;

        public TvShowsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveTvShowsAsync(List<TvShowDTO> tvShows)
        {
            Log.Information("Saving {TvShowCount} TV shows to the database.", tvShows.Count);

            var dbTvShows = tvShows.Select(t => new TvShow
            {
                Title = t.Title,
                Overview = t.Overview,
                ReleaseDate = t.ReleaseDate,
                Popularity = t.Popularity,
                TvShowGenres = t.Genres.Select(genreName => new TvShowGenre
                {
                    Genre = GetOrAddGenre(genreName)
                }).ToList()
            }).ToList();

            _context.TvShows.AddRange(dbTvShows);

            try
            {
                await _context.SaveChangesAsync();
                Log.Information("Successfully saved {TvShowCount} TV shows to the database.", tvShows.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while saving TV shows: {ErrorMessage}", ex.Message);
                throw; // Re-throw the exception for handling by the caller
            }
        }

        public async Task ClearTvShowsAsync()
        {
            Log.Information("Clearing all TV shows from the database.");

            _context.TvShows.RemoveRange(_context.TvShows);

            try
            {
                await _context.SaveChangesAsync();
                Log.Information("Successfully cleared all TV shows from the database.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while clearing TV shows: {ErrorMessage}", ex.Message);
                throw; // Re-throw the exception for handling by the caller
            }
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
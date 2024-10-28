using Top10MediaApi.Models;

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
            await _context.SaveChangesAsync();
        }

        public async Task ClearTvShowsAsync()
        {
            _context.TvShows.RemoveRange(_context.TvShows);
            await _context.SaveChangesAsync();
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

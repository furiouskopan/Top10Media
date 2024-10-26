using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Top10MediaApi.Models;

[ApiController]
[Route("api/[controller]")]
public class Top10MoviesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _tmdbApiKey;
    private readonly AppDbContext _context;

    public Top10MoviesController(IHttpClientFactory httpClientFactory, IConfiguration configuration, AppDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _tmdbApiKey = configuration["TmdbApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "TmdbApiKey is not configured.");
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTop10Movies()
    {
        string tmdbUrl = $"https://api.themoviedb.org/3/movie/popular?api_key={_tmdbApiKey}&language=en-US&page=1";
        var client = _httpClientFactory.CreateClient();

        try
        {
            var response = await client.GetAsync(tmdbUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var movieData = JsonDocument.Parse(jsonResponse);

                var top10Movies = movieData.RootElement.GetProperty("results")
                                    .EnumerateArray()
                                    .Take(10)
                                    .Select(movie => new MovieDTO
                                    {
                                        Title = movie.GetProperty("title").GetString(),
                                        Overview = movie.GetProperty("overview").GetString(),
                                        ReleaseDate = DateTime.SpecifyKind(
                                            DateTime.Parse(movie.GetProperty("release_date").GetString()),
                                            DateTimeKind.Utc
                                        ),
                                        Popularity = movie.GetProperty("popularity").GetDouble(),
                                        Genres = movie.GetProperty("genre_ids")
                                                      .EnumerateArray()
                                                      .Select(genreId => GetGenreName(genreId.GetInt32()))
                                                      .ToList()
                                    }).ToList();

                // Add movies to the database
                _context.Movies.AddRange(top10Movies.Select(m => new Movie
                {
                    Title = m.Title,
                    Overview = m.Overview,
                    ReleaseDate = m.ReleaseDate,
                    Popularity = m.Popularity,
                    MovieGenres = m.Genres.Select(genreName => new MovieGenre
                    {
                        Genre = _context.Genres.FirstOrDefault(g => g.Name == genreName)
                    }).ToList()
                }));

                // Save the changes
                await _context.SaveChangesAsync();

                return Ok(top10Movies);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error fetching data from TMDb");
            }
        }
        catch (HttpRequestException e)
        {
            return StatusCode(500, $"Error: {e.Message}");
        }
    }

    [HttpPost("reset-top10-movies")]
    public async Task ResetTop10Movies()
    {
        // Clear existing movies
        _context.Movies.RemoveRange(_context.Movies);
        await _context.SaveChangesAsync();

        // Fetch new movies from TMDb API
        await GetTop10Movies();
    }


    // Helper method to either fetch or add a genre
    private Genre GetOrAddGenre(int genreId)
    {
        // Check if the genre is already being tracked
        var trackedGenre = _context.ChangeTracker.Entries<Genre>()
            .FirstOrDefault(e => e.Entity.Id == genreId)?.Entity;

        if (trackedGenre != null)
        {
            return trackedGenre;
        }

        // Try to find the genre in the database
        var genre = _context.Genres.SingleOrDefault(g => g.Id == genreId);

        if (genre == null)
        {
            // If genre is not found, create a new one
            genre = new Genre
            {
                Id = genreId,
                Name = GetGenreName(genreId)
            };

            _context.Genres.Add(genre);  // Add the new genre to the context
        }

        return genre;
    }

    private string GetGenreName(int genreId)
    {
        // Try to get the genre name from the dictionary, or return "Unknown" if not found
        return genreDictionary.TryGetValue(genreId, out var genreName) ? genreName : "Unknown";
    }
    private readonly Dictionary<int, string> genreDictionary = new Dictionary<int, string>
    {
        { 28, "Action" },
        { 12, "Adventure" },
        { 16, "Animation" },
        { 35, "Comedy" },
        { 80, "Crime" },
        { 99, "Documentary" },
        { 18, "Drama" },
        { 10751, "Family" },
        { 14, "Fantasy" },
        { 36, "History" },
        { 27, "Horror" },
        { 10402, "Music" },
        { 9648, "Mystery" },
        { 10749, "Romance" },
        { 878, "Science Fiction" },
        { 10770, "TV Movie" },
        { 53, "Thriller" },
        { 10752, "War" },
        { 37, "Western" }
    };
}

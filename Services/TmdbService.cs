using System.Text.Json;
using Top10MediaApi.Models;
using Microsoft.Extensions.Logging;

namespace Top10MediaApi.Services
{
    public class TmdbService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _tmdbApiKey;
        private readonly Dictionary<int, string> _genreDictionary;
        private readonly ILogger<TmdbService> _logger;

        public TmdbService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<TmdbService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _tmdbApiKey = Environment.GetEnvironmentVariable("TmdbApiKey");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _genreDictionary = new Dictionary<int, string>
            {
                { 28, "Action" }, { 12, "Adventure" }, { 16, "Animation" }, { 35, "Comedy" },
                { 80, "Crime" }, { 99, "Documentary" }, { 18, "Drama" }, { 10751, "Family" },
                { 14, "Fantasy" }, { 36, "History" }, { 27, "Horror" }, { 10402, "Music" },
                { 9648, "Mystery" }, { 10749, "Romance" }, { 878, "Science Fiction" },
                { 10770, "TV Movie" }, { 53, "Thriller" }, { 10752, "War" }, { 37, "Western" }
            };
        }

        public async Task<List<MovieDTO>> GetTop10MoviesAsync()
        {
            _logger.LogInformation("Fetching top 10 movies from TMDb.");

            var tmdbUrl = $"https://api.themoviedb.org/3/movie/popular?api_key={_tmdbApiKey}&language=en-US&page=1";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(tmdbUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch movies from TMDb. Status code: {StatusCode}", response.StatusCode);
                throw new HttpRequestException("Error fetching data from TMDb");
            }

            _logger.LogInformation("Successfully fetched movie data from TMDb.");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var movieData = JsonDocument.Parse(jsonResponse);

            var movies = movieData.RootElement.GetProperty("results")
                .EnumerateArray()
                .Take(10)
                .Select(movie => new MovieDTO
                {
                    Title = movie.GetProperty("title").GetString(),
                    Overview = movie.GetProperty("overview").GetString(),
                    ReleaseDate = DateTime.SpecifyKind(
                        DateTime.Parse(movie.GetProperty("release_date").GetString()),
                        DateTimeKind.Utc),
                    Popularity = movie.GetProperty("popularity").GetDouble(),
                    Genres = movie.GetProperty("genre_ids")
                        .EnumerateArray()
                        .Select(genreId => _genreDictionary.TryGetValue(genreId.GetInt32(), out var genre) ? genre : "Unknown")
                        .ToList()
                }).ToList();

            _logger.LogInformation("Successfully parsed {Count} movies from TMDb response.", movies.Count);

            return movies;
        }

        public async Task<List<TvShowDTO>> GetTop10TvShowsAsync()
        {
            _logger.LogInformation("Fetching top 10 TV shows from TMDb.");

            var tmdbUrl = $"https://api.themoviedb.org/3/tv/popular?api_key={_tmdbApiKey}&language=en-US&page=1";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(tmdbUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch TV shows from TMDb. Status code: {StatusCode}", response.StatusCode);
                throw new HttpRequestException("Error fetching data from TMDb");
            }

            _logger.LogInformation("Successfully fetched TV show data from TMDb.");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tvShowData = JsonDocument.Parse(jsonResponse);

            var tvShows = tvShowData.RootElement.GetProperty("results")
                .EnumerateArray()
                .Take(10)
                .Select(tvShow => new TvShowDTO
                {
                    Title = tvShow.GetProperty("name").GetString(),
                    Overview = tvShow.GetProperty("overview").GetString(),
                    ReleaseDate = DateTime.SpecifyKind(
                        DateTime.Parse(tvShow.GetProperty("first_air_date").GetString()),
                        DateTimeKind.Utc),
                    Popularity = tvShow.GetProperty("popularity").GetDouble(),
                    Genres = tvShow.GetProperty("genre_ids")
                        .EnumerateArray()
                        .Select(genreId => _genreDictionary.TryGetValue(genreId.GetInt32(), out var genre) ? genre : "Unknown")
                        .ToList()
                }).ToList();

            _logger.LogInformation("Successfully parsed {Count} TV shows from TMDb response.", tvShows.Count);

            return tvShows;
        }
    }
}

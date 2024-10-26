using System.Text.Json;
using Top10MediaApi.Models;

namespace Top10MediaApi.Services
{
    public class TmdbService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _tmdbApiKey;
        private readonly Dictionary<int, string> _genreDictionary;

        public TmdbService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _tmdbApiKey = configuration["TmdbApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "TmdbApiKey is not configured.");

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
            var tmdbUrl = $"https://api.themoviedb.org/3/movie/popular?api_key={_tmdbApiKey}&language=en-US&page=1";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(tmdbUrl);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Error fetching data from TMDb");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var movieData = JsonDocument.Parse(jsonResponse);

            return movieData.RootElement.GetProperty("results")
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
        }
    }
}

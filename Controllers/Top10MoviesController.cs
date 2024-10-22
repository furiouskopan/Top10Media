using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Top10MediaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Top10MoviesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _tmdbApiKey;

        public Top10MoviesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _tmdbApiKey = configuration["TmdbApiKey"];
        }

        [HttpGet]
        public async Task<IActionResult> GetTop10Movies()
        {
            // Base URL for fetching the top-rated movies of the month from TMDb
            string tmdbUrl = $"https://api.themoviedb.org/3/movie/popular?api_key={_tmdbApiKey}&language=en-US&page=1";

            // Create an HTTP client instance
            var client = _httpClientFactory.CreateClient();

            try
            {
                // Send a GET request to TMDb API
                var response = await client.GetAsync(tmdbUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Parse the JSON response
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var movieData = JsonDocument.Parse(jsonResponse);

                    // Extract the top 10 movies
                    var top10Movies = movieData.RootElement.GetProperty("results")
                                        .EnumerateArray()
                                        .Take(10)
                                        .Select(movie => new
                                        {
                                            Title = movie.GetProperty("title").GetString(),
                                            Overview = movie.GetProperty("overview").GetString(),
                                            ReleaseDate = movie.GetProperty("release_date").GetString(),
                                            Popularity = movie.GetProperty("popularity").GetDouble()
                                        });

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
    }
}

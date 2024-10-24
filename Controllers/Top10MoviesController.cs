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
        _tmdbApiKey = configuration["TmdbApiKey"];
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
                                    .Select(movie => new Movie
                                    {
                                        Title = movie.GetProperty("title").GetString(),
                                        Overview = movie.GetProperty("overview").GetString(),
                                        ReleaseDate = DateTime.Parse(movie.GetProperty("release_date").GetString()),
                                        Popularity = movie.GetProperty("popularity").GetDouble(),
                                        Genres = movie.GetProperty("genre_ids").EnumerateArray()
                                                    .Select(g => new MovieGenre { Id = g.GetInt32(), Name = "GenreName" }).ToList()
                                    });

                // Save each movie to the database
                foreach (var movie in top10Movies)
                {
                    if (!_context.Movies.Any(m => m.Title == movie.Title))
                    {
                        _context.Movies.Add(movie);
                    }
                }
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
}

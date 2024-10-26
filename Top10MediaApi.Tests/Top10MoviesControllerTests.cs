using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Top10MediaApi.Models;
using Top10MediaApi.Services;
using Xunit;

public class Top10MoviesControllerTests
{
    [Fact]
    public async Task GetTop10Movies_ReturnsOkResult()
    {
        // Arrange
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockConfiguration = new Mock<IConfiguration>();

        // Mock the TMDB API Key
        mockConfiguration.Setup(c => c["TmdbApiKey"]).Returns("dummy_api_key");

        // Set up HttpMessageHandler
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetMockTmdbResponse(), Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        // Instantiate TmdbService with mocks
        var tmdbService = new TmdbService(mockHttpClientFactory.Object, mockConfiguration.Object);

        // Create the MoviesService (assuming you have a proper setup here)
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestMovieDb")
            .Options;
        var context = new AppDbContext(options);
        SeedGenres(context);

        var moviesService = new MoviesService(context);
        var controller = new Top10MoviesController(tmdbService, moviesService);

        // Act
        var result = await controller.GetTop10Movies();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var movies = Assert.IsType<List<MovieDTO>>(okResult.Value);
        Assert.Equal(1, movies.Count); // Adjust based on the mock data you provided
    }

    private string GetMockTmdbResponse()
    {
        return @"{
            ""results"": [
                {
                    ""title"": ""Movie 1"",
                    ""overview"": ""Overview for Movie 1"",
                    ""release_date"": ""2023-01-01"",
                    ""popularity"": 10.0,
                    ""genre_ids"": [28, 12]
                }
            ]
        }";
    }

    private void SeedGenres(AppDbContext context)
    {
        var genres = new List<Genre>
        {
            new Genre { Name = "Action" },
            new Genre { Name = "Adventure" },
            new Genre { Name = "Animation" },
            new Genre { Name = "Comedy" },
            new Genre { Name = "Drama" },
            new Genre { Name = "Horror" }
        };

        context.Genres.AddRange(genres);
        context.SaveChanges();
    }
}

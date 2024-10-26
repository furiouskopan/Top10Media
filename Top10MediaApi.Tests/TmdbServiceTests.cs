using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Top10MediaApi.Models;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using Moq.Protected;
using System.Text;
using Top10MediaApi.Services;

namespace Top10MediaApi.Tests
{
    public class TmdbServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly TmdbService _tmdbService;

        public TmdbServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(c => c["TmdbApiKey"]).Returns("dummy_api_key");
            var client = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            _tmdbService = new TmdbService(_mockHttpClientFactory.Object, mockConfiguration.Object);
        }

        [Fact]
        public virtual async Task GetTop10MoviesAsync_ReturnsListOfMovies()
        {
            // Arrange
            var mockResponse = new
            {
                results = new[]
                {
                    new
                    {
                        title = "The Wild Robot",
                        overview = "After a shipwreck, an intelligent robot called Roz is stranded on an uninhabited island.",
                        release_date = "2024-09-12",
                        popularity = 5173.119,
                        genre_ids = new[] { 28, 16 }
                    },
                    new
                    {
                        title = "Alien: Romulus",
                        overview = "A group of young space colonizers come face to face with a terrifying life form.",
                        release_date = "2024-08-13",
                        popularity = 3297.462,
                        genre_ids = new[] { 878, 53 }
                    }
                }
            };

            var json = JsonSerializer.Serialize(mockResponse);
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            // Act
            var movies = await _tmdbService.GetTop10MoviesAsync();

            // Assert
            Assert.Equal(2, movies.Count);
            Assert.Equal("The Wild Robot", movies[0].Title);
            Assert.Equal("Alien: Romulus", movies[1].Title);
        }

        [Fact]
        public async Task GetTop10MoviesAsync_ApiFailure_ThrowsHttpRequestException()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _tmdbService.GetTop10MoviesAsync());
        }
    }
}

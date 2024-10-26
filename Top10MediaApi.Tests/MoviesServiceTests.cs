using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Top10MediaApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Top10MediaApi.Services;

namespace Top10MediaApi.Tests
{
    public class MoviesServiceTests
    {
        private readonly MoviesService _moviesService;
        private readonly AppDbContext _context;

        public MoviesServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestMoviesDb")
                .Options;

            _context = new AppDbContext(options);
            _moviesService = new MoviesService(_context);
        }

        [Fact]
        public async Task SaveMoviesAsync_SavesMoviesToDatabase()
        {
            // Arrange
            var movies = new List<MovieDTO>
            {
                new MovieDTO
                {
                    Title = "The Wild Robot",
                    Overview = "After a shipwreck, an intelligent robot called Roz is stranded on an uninhabited island.",
                    ReleaseDate = new DateTime(2024, 9, 12),
                    Popularity = 5173.119,
                    Genres = new List<string> { "Action", "Animation" }
                },
                new MovieDTO
                {
                    Title = "Alien: Romulus",
                    Overview = "A group of young space colonizers come face to face with a terrifying life form.",
                    ReleaseDate = new DateTime(2024, 8, 13),
                    Popularity = 3297.462,
                    Genres = new List<string> { "Science Fiction", "Thriller" }
                }
            };

            // Act
            await _moviesService.SaveMoviesAsync(movies);
            var savedMovies = await _context.Movies.ToListAsync();

            // Assert
            Assert.Equal(2, savedMovies.Count);
            Assert.Equal("The Wild Robot", savedMovies[0].Title);
            Assert.Equal("Alien: Romulus", savedMovies[1].Title);
        }

        [Fact]
        public async Task ClearMoviesAsync_RemovesAllMovies()
        {
            // Arrange
            await _moviesService.SaveMoviesAsync(new List<MovieDTO>
            {
                new MovieDTO { Title = "The Wild Robot", Overview = "", ReleaseDate = DateTime.UtcNow, Popularity = 0, Genres = new List<string>() },
                new MovieDTO { Title = "Alien: Romulus", Overview = "", ReleaseDate = DateTime.UtcNow, Popularity = 0, Genres = new List<string>() }
            });

            // Act
            await _moviesService.ClearMoviesAsync();
            var movieCount = await _context.Movies.CountAsync();

            // Assert
            Assert.Equal(0, movieCount);
        }
    }
}

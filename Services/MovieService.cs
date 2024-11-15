﻿using Serilog;
using Top10MediaApi.Models;
using Top10MediaApi.Models.Movies;

namespace Top10MediaApi.Services
{
    public class MoviesService
    {
        private readonly AppDbContext _context;

        public MoviesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveMoviesAsync(List<MovieDTO> movies)
        {
            Log.Information("Saving movies to the database.");
            var dbMovies = movies.Select(m => new Movie
            {
                Title = m.Title,
                Overview = m.Overview,
                ReleaseDate = m.ReleaseDate,
                Popularity = m.Popularity,
                MovieGenres = m.Genres.Select(genreName => new MovieGenre
                {
                    Genre = GetOrAddGenre(genreName)
                }).ToList()
            }).ToList();

            _context.Movies.AddRange(dbMovies);
            await _context.SaveChangesAsync();

            Log.Warning("Movies have been successfully saved.");
        }

        public async Task ClearMoviesAsync()
        {
            _context.Movies.RemoveRange(_context.Movies);
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

using Microsoft.EntityFrameworkCore;
using Top10MediaApi.Models;
using Top10MediaApi.Models.Games;
using Top10MediaApi.Models.Movies;
using Top10MediaApi.Models.Users;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<MovieGenre> MovieGenres { get; set; } // Join table for Movie-Genre relationship
    public DbSet<TvShow> TvShows { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<TvShowGenre> TvShowGenres { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserMediaList> UserMediaLists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the many-to-many relationship between Movies and Genres
        modelBuilder.Entity<MovieGenre>()
            .HasKey(mg => new { mg.MovieId, mg.GenreId });

        modelBuilder.Entity<MovieGenre>()
            .HasOne(mg => mg.Movie)
            .WithMany(m => m.MovieGenres)
            .HasForeignKey(mg => mg.MovieId);

        modelBuilder.Entity<MovieGenre>()
            .HasOne(mg => mg.Genre)
            .WithMany(g => g.MovieGenres)
            .HasForeignKey(mg => mg.GenreId);

        modelBuilder.Entity<UserMediaList>()
            .HasKey(um => um.Id);

        modelBuilder.Entity<UserMediaList>()
            .HasOne(um => um.User)
            .WithMany(u => u.UserMediaList)
            .HasForeignKey(um => um.UserId);
    }
}

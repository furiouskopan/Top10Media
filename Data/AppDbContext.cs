using Microsoft.EntityFrameworkCore;
using Top10MediaApi.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<MovieGenre> MovieGenres { get; set; } // Join table for Movie-Genre relationship
    public DbSet<TvShow> TvShows { get; set; }
    public DbSet<TvShowGenre> TvShowGenres { get; set; }

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

        // Optionally for TV Shows:
        // modelBuilder.Entity<TvShowGenre>()
        //    .HasKey(tg => new { tg.TvShowId, tg.GenreId });
    }
}

namespace Top10MediaApi.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Relationship to Movies
        public ICollection<MovieGenre> MovieGenres { get; set; } // Join table for many-to-many relationship
    }
}

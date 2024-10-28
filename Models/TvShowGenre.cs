namespace Top10MediaApi.Models
{
    public class TvShowGenre
    {
        public int Id { get; set; }
        public int TvShowId { get; set; }
        public TvShow TvShow { get; set; }

        public int GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}

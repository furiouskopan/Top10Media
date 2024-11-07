namespace Top10MediaApi.Models
{
    public class TvShow
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double Popularity { get; set; }

        public List<TvShowGenre> TvShowGenres { get; set; }
    }
}

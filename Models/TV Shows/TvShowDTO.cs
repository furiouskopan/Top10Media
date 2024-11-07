namespace Top10MediaApi.Models
{
    public class TvShowDTO
    {
        public string Title { get; set; }
        public string Overview { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double Popularity { get; set; }
        public List<string> Genres { get; set; }
    }
}

namespace Top10MediaApi.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Director { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Overview { get; set; }  
        public double Popularity { get; set; }  
        public ICollection<MovieGenre> Genres { get; set; } = new List<MovieGenre>();  
    }
}

namespace Top10MediaApi.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Overview { get; set; }
        public double Popularity { get; set; }
        public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
    }
}

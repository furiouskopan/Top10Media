namespace Top10MediaApi.Models.Games
{
    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Overview { get; set; }
        public double Popularity { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
    }
}

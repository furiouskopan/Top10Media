﻿namespace Top10MediaApi.Models
{
    public class GameDTO
    {
        public string Title { get; set; }
        public string Overview { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public double Popularity { get; set; }
        public List<string> Genres { get; set; }
    }
}
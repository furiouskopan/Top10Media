using Microsoft.AspNetCore.Mvc.Formatters;

namespace Top10MediaApi.Models.Users
{
    public class UserMediaList
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MediaId { get; set; }
        public MediaType MediaType { get; set; }

        // Navigation properties
        public User User { get; set; }
    }
    public enum MediaType
    {
        Movie,
        TVShow,
        Game
    }
}

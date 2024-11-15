﻿using static Top10MediaApi.Models.Users.UserMediaList;
namespace Top10MediaApi.Models.Users
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } = "User";
        public List<UserMediaList> UserMediaList { get; set; } = new List<UserMediaList>();
    }
}

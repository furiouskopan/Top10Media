using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Top10MediaApi.Models.Users;
using Serilog;

namespace Top10MediaApi.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> RegisterUserAsync(string username, string password, string email)
        {
            try
            {
                // Check if user already exists
                if (_context.Users.Any(u => u.Username == username))
                {
                    Log.Warning("User with username '{Username}' already exists.", username);
                    throw new ArgumentException("User already exists");
                }

                // Hash the password
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                // Create the user
                var user = new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    Email = email,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                Log.Information("Successfully registered user: {Username}", user.Username);
                return user;
            }
            catch (ArgumentException)
            {
                throw; // re-throw the validation exception
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while registering user: {ErrorMessage}", ex.Message);
                throw new Exception("An unexpected error occurred during user registration");
            }
        }

        public async Task<User> ValidateUserAsync(string username, string password)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    Log.Information("Invalid login attempt for username: {Username}", username);
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while validating user: {ErrorMessage}", ex.Message);
                throw new Exception("An unexpected error occurred during user validation");
            }
        }
    }
}
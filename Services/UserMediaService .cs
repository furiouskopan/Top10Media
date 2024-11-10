using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Top10MediaApi.Models.Users;

namespace Top10MediaApi.Services
{
    public class UserMediaService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserMediaService> _logger;

        public UserMediaService(AppDbContext context, ILogger<UserMediaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddMediaToUserListAsync(int userId, int mediaId, MediaType mediaType)
        {
            try
            {
                // Validate input
                if (userId <= 0 || mediaId <= 0)
                {
                    _logger.LogWarning("Invalid userId or mediaId provided: userId={UserId}, mediaId={MediaId}", userId, mediaId);
                    throw new ArgumentException("Invalid userId or mediaId");
                }

                // Check if the media exists in the main list/database
                bool mediaExistsInMain = await MediaExistsInMainList(mediaId, mediaType);

                if (!mediaExistsInMain)
                {
                    _logger.LogWarning("Media item (ID: {MediaId}, Type: {MediaType}) does not exist in the main list", mediaId, mediaType);
                    throw new InvalidOperationException("The specified media item does not exist");
                }

                // Prevent duplicate entries
                bool mediaExistsInUserList = await _context.UserMediaLists
                    .AnyAsync(um => um.UserId == userId && um.MediaId == mediaId && um.MediaType == mediaType);

                if (mediaExistsInUserList)
                {
                    _logger.LogInformation("Media item (ID: {MediaId}, Type: {MediaType}) already exists for user {UserId}", mediaId, mediaType, userId);
                    return; // Optionally, throw an exception or return a specific result
                }

                // Create a new UserMediaList entry
                var mediaItem = new UserMediaList
                {
                    UserId = userId,
                    MediaId = mediaId,
                    MediaType = mediaType,
                };

                _context.UserMediaLists.Add(mediaItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Media item (ID: {MediaId}, Type: {MediaType}) added for user {UserId}", mediaId, mediaType, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding media item (ID: {MediaId}, Type: {MediaType}) for user {UserId}", mediaId, mediaType, userId);
                throw;
            }
        }
        public async Task<bool> MediaExistsInMainList(int mediaId, MediaType mediaType)
        {
            switch (mediaType)
            {
                case MediaType.Movie:
                    return await _context.Movies.AnyAsync(m => m.Id == mediaId);
                case MediaType.Game:
                    return await _context.Games.AnyAsync(g => g.Id == mediaId);
                case MediaType.TVShow:
                    return await _context.TvShows.AnyAsync(t => t.Id == mediaId);
                default:
                    return false;
            }
        }

        public async Task<List<UserMediaList>> GetUserMediaListAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid userId provided: {UserId}", userId);
                    throw new ArgumentException("Invalid userId");
                }

                var userMediaList = await _context.UserMediaLists
                    .Where(um => um.UserId == userId)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} media items for user {UserId}", userMediaList.Count, userId);

                return userMediaList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving media list for user {UserId}", userId);
                throw;
            }
        }

        public async Task RemoveMediaFromUserListAsync(int userId, int mediaId, MediaType mediaType)
        {
            try
            {
                if (userId <= 0 || mediaId <= 0)
                {
                    _logger.LogWarning("Invalid userId or mediaId provided: userId={UserId}, mediaId={MediaId}", userId, mediaId);
                    throw new ArgumentException("Invalid userId or mediaId");
                }

                var userMedia = await _context.UserMediaLists
                    .FirstOrDefaultAsync(um => um.UserId == userId && um.MediaId == mediaId && um.MediaType == mediaType);

                if (userMedia != null)
                {
                    _context.UserMediaLists.Remove(userMedia);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Media item (ID: {MediaId}, Type: {MediaType}) removed for user {UserId}", mediaId, mediaType, userId);
                }
                else
                {
                    _logger.LogWarning("Media item (ID: {MediaId}, Type: {MediaType}) not found for user {UserId}", mediaId, mediaType, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing media item (ID: {MediaId}, Type: {MediaType}) for user {UserId}", mediaId, mediaType, userId);
                throw;
            }
        }
    }
}

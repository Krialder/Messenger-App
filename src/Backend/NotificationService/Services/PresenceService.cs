using StackExchange.Redis;

namespace NotificationService.Services
{
    /// <summary>
    /// Redis-based presence management service
    /// Tracks user online/offline status
    /// </summary>
    public class PresenceService : IPresenceService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<PresenceService> _logger;
        private const string PRESENCE_KEY_PREFIX = "presence:";
        private const int PRESENCE_EXPIRATION_SECONDS = 300; // 5 minutes

        public PresenceService(IConnectionMultiplexer redis, ILogger<PresenceService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task SetUserOnlineAsync(string userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = GetPresenceKey(userId);
                
                // Set presence with expiration
                await db.StringSetAsync(key, "online", TimeSpan.FromSeconds(PRESENCE_EXPIRATION_SECONDS));
                
                _logger.LogDebug("User {UserId} set to online", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user {UserId} online", userId);
                throw;
            }
        }

        public async Task SetUserOfflineAsync(string userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = GetPresenceKey(userId);
                
                // Remove presence key
                await db.KeyDeleteAsync(key);
                
                _logger.LogDebug("User {UserId} set to offline", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user {UserId} offline", userId);
                throw;
            }
        }

        public async Task<bool> IsUserOnlineAsync(string userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = GetPresenceKey(userId);
                
                return await db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking online status for user {UserId}", userId);
                return false;
            }
        }

        public async Task<Dictionary<string, bool>> GetOnlineStatusAsync(List<string> userIds)
        {
            var result = new Dictionary<string, bool>();
            
            try
            {
                var db = _redis.GetDatabase();
                
                foreach (var userId in userIds)
                {
                    var key = GetPresenceKey(userId);
                    result[userId] = await db.KeyExistsAsync(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online status for users");
            }
            
            return result;
        }

        private string GetPresenceKey(string userId)
        {
            return $"{PRESENCE_KEY_PREFIX}{userId}";
        }
    }

    /// <summary>
    /// Interface for presence service
    /// </summary>
    public interface IPresenceService
    {
        Task SetUserOnlineAsync(string userId);
        Task SetUserOfflineAsync(string userId);
        Task<bool> IsUserOnlineAsync(string userId);
        Task<Dictionary<string, bool>> GetOnlineStatusAsync(List<string> userIds);
    }
}

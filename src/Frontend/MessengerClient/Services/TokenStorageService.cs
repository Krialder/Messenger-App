using System.Threading.Tasks;

namespace MessengerClient.Services
{
    /// <summary>
    /// Service for managing authentication tokens in local storage
    /// Separate from IAuthApiService to avoid Refit warnings
    /// </summary>
    public interface ITokenStorageService
    {
        Task StoreTokensAsync(string accessToken, string refreshToken);
        Task<string?> GetStoredTokenAsync();
        Task ClearStoredTokensAsync();
    }

    /// <summary>
    /// Implementation using LocalStorageService for secure token persistence
    /// </summary>
    public class TokenStorageService : ITokenStorageService
    {
        private readonly LocalStorageService _localStorage;

        public TokenStorageService(LocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task StoreTokensAsync(string accessToken, string refreshToken)
        {
            await _localStorage.SaveTokenAsync(accessToken);
            // RefreshToken storage could be added here if needed
            await Task.CompletedTask;
        }

        public async Task<string?> GetStoredTokenAsync()
        {
            return await _localStorage.GetTokenAsync();
        }

        public async Task ClearStoredTokensAsync()
        {
            await _localStorage.ClearTokenAsync();
        }
    }
}

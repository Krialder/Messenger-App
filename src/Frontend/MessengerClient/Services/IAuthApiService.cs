using System.Threading.Tasks;
using Refit;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    public interface IAuthApiService
    {
        [Post("/api/auth/register")]
        Task<RegisterResponse> RegisterAsync([Body] RegisterRequest request);

        [Post("/api/auth/login")]
        Task<LoginResponse> LoginAsync([Body] LoginRequest request);

        [Post("/api/auth/verify-mfa")]
        Task<LoginResponse> VerifyMfaAsync([Body] VerifyMfaRequest request);

        [Post("/api/mfa/enable-totp")]
        Task<EnableTotpResponse> SetupMfaAsync([Header("Authorization")] string authorization);

        [Post("/api/auth/refresh")]
        Task<TokenResponse> RefreshTokenAsync([Body] RefreshTokenRequest request);

        [Post("/api/auth/logout")]
        Task LogoutAsync([Header("Authorization")] string authorization);
    }

    // Concrete implementation will handle token storage
    public static class AuthApiServiceExtensions
    {
        private static readonly LocalStorageService _localStorage = new(null!); // TODO: Proper DI

        public static async Task StoreTokensAsync(this IAuthApiService service, string accessToken, string refreshToken)
        {
            await _localStorage.SaveTokenAsync(accessToken);
        }

        public static async Task<string?> GetStoredTokenAsync(this IAuthApiService service)
        {
            return await _localStorage.GetTokenAsync();
        }

        public static async Task ClearStoredTokensAsync(this IAuthApiService service)
        {
            await _localStorage.ClearTokenAsync();
        }
    }
}

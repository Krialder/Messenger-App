using System.Threading.Tasks;
using Refit;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    /// <summary>
    /// Refit interface for authentication API calls
    /// Note: Token storage methods moved to ITokenStorageService
    /// </summary>
    public interface IAuthApiService
    {
        // Refit API calls
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
}

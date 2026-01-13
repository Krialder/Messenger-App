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

        [Post("/api/auth/mfa/setup")]
        Task<EnableTotpResponse> SetupMfaAsync([Header("Authorization")] string authorization);

        [Post("/api/auth/mfa/verify")]
        Task<LoginResponse> VerifyMfaAsync([Body] VerifyMfaRequest request);

        [Post("/api/auth/refresh")]
        Task<TokenResponse> RefreshTokenAsync([Body] RefreshTokenRequest request);
    }
}

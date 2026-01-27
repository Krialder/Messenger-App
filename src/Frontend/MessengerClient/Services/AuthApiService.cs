using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MessengerClient.Services;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly IAuthApiService _refitClient;
        private readonly LocalStorageService _localStorage;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5001/api";

        public AuthApiService(IAuthApiService refitClient, LocalStorageService localStorage)
        {
            _refitClient = refitClient;
            _localStorage = localStorage;
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            return await _refitClient.RegisterAsync(request);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/auth/login", request);
                
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 900;
                    throw new RateLimitException($"Zu viele Login-Versuche. Bitte warten Sie {retryAfter} Sekunden.");
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<LoginResponse>() 
                    ?? throw new InvalidOperationException("Login response is null");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Login fehlgeschlagen: {ex.Message}", ex);
            }
        }

        public async Task<LoginResponse> VerifyMfaAsync(VerifyMfaRequest request)
        {
            return await _refitClient.VerifyMfaAsync(request);
        }

        public async Task<EnableTotpResponse> SetupMfaAsync(string authorization)
        {
            return await _refitClient.SetupMfaAsync(authorization);
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            return await _refitClient.RefreshTokenAsync(request);
        }

        public async Task LogoutAsync(string authorization)
        {
            await _refitClient.LogoutAsync(authorization);
            await ClearStoredTokensAsync();
        }

        // Token management implementation
        public async Task StoreTokensAsync(string accessToken, string refreshToken)
        {
            // Store access token
            await _localStorage.SaveTokenAsync(accessToken);
            
            // Store refresh token separately in secure storage
            // Using LocalStorage with encryption (Layer 2)
            await _localStorage.SaveRefreshTokenAsync(refreshToken);
        }

        public async Task<string?> GetStoredTokenAsync()
        {
            return await _localStorage.GetTokenAsync();
        }

        public async Task ClearStoredTokensAsync()
        {
            await _localStorage.ClearTokenAsync();
            await _localStorage.ClearRefreshTokenAsync();
        }
    }

    public class RateLimitException : Exception
    {
        public RateLimitException(string message) : base(message) { }
    }
}

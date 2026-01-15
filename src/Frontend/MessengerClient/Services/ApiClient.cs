using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerClient.Services
{
    public interface IApiClient
    {
        Task<LoginResponse?> LoginAsync(string username, string password);
        Task<MfaResponse?> VerifyMfaAsync(string sessionToken, string code);
        Task<bool> SendMessageAsync(SendMessageRequest request);
        Task<List<MessageDto>?> GetMessageHistoryAsync(Guid contactId, int page = 1);
    }

    public class ApiClient : IApiClient
    {
        // PSEUDO CODE: HTTP API Client Service

        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:5001/api";

        public ApiClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            // PSEUDO CODE:
            // 1. POST /api/auth/login
            // 2. Send: { username, password }
            // 3. Receive: { access_token, refresh_token, mfa_required }
            // 4. Handle errors (401, 500)

            var request = new { username, password };
            var response = await _httpClient.PostAsJsonAsync("/auth/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>();
            }
            
            throw new HttpRequestException($"Login failed: {response.StatusCode}");
        }

        public async Task<MfaResponse?> VerifyMfaAsync(string sessionToken, string code)
        {
            // PSEUDO CODE:
            // 1. POST /api/auth/verify-mfa
            // 2. Send: { session_token, code, method_type: "totp" }
            // 3. Receive: { access_token, refresh_token }

            var request = new { session_token = sessionToken, code, method_type = "totp" };
            var response = await _httpClient.PostAsJsonAsync("/auth/verify-mfa", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MfaResponse>();
            }
            
            throw new HttpRequestException($"MFA verification failed: {response.StatusCode}");
        }

        public async Task<bool> SendMessageAsync(SendMessageRequest request)
        {
            // PSEUDO CODE:
            // 1. POST /api/messages/send
            // 2. Headers: Authorization: Bearer {jwt_token}
            // 3. Send: { recipient_id, encrypted_content, nonce, ephemeral_public_key }
            // 4. Receive: { message_id }

            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("/messages/send", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<MessageDto>?> GetMessageHistoryAsync(Guid contactId, int page = 1)
        {
            // PSEUDO CODE:
            // 1. GET /api/messages/history/{contactId}?page={page}
            // 2. Headers: Authorization: Bearer {jwt_token}
            // 3. Receive: { messages: [...], total_count, page }

            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"/messages/history/{contactId}?page={page}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<MessageHistoryResponse>();
                return result?.Messages;
            }
            
            return null;
        }

        private void AddAuthorizationHeader()
        {
            // PSEUDO CODE:
            // 1. Get JWT token from secure storage
            // 2. Add to request headers: Authorization: Bearer {token}

            // var token = _tokenStorage.GetAccessToken();
            // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // DTOs
    public record LoginResponse(string? AccessToken, string? RefreshToken, bool MfaRequired, string? SessionToken);
    public record MfaResponse(string AccessToken, string RefreshToken);
    public record SendMessageRequest(Guid RecipientId, byte[] EncryptedContent, byte[] Nonce, byte[] EphemeralPublicKey);
    public record MessageDto(Guid Id, byte[] EncryptedContent, byte[] Nonce, DateTime CreatedAt);
    public record MessageHistoryResponse(List<MessageDto> Messages, int TotalCount, int Page);
}

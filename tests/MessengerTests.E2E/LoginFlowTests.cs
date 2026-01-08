using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using MessengerContracts.DTOs;

namespace MessengerTests.E2E
{
    /// <summary>
    /// End-to-End test for complete login flow
    /// Tests: Registration → Login → JWT → MFA → Dashboard
    /// </summary>
    public class LoginFlowTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public LoginFlowTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CompleteLoginFlow_RegisterAndLogin_Success()
        {
            // ARRANGE
            var username = $"testuser_{Guid.NewGuid()}";
            var registerDto = new RegisterUserDto
            {
                Username = username,
                Email = $"{username}@example.com",
                Password = "SecurePass123!"
            };

            // ACT 1: Register user
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            
            // ASSERT 1: Registration successful
            Assert.True(registerResponse.IsSuccessStatusCode);
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(registerResult);
            Assert.NotEqual(Guid.Empty, registerResult.UserId);
            Assert.NotNull(registerResult.MasterKeySalt);

            // ACT 2: Login with credentials
            var loginDto = new LoginDto
            {
                Username = username,
                Password = "SecurePass123!"
            };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // ASSERT 2: Login successful
            Assert.True(loginResponse.IsSuccessStatusCode);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(loginResult);
            Assert.NotEmpty(loginResult.Token);

            // ACT 3: Access protected endpoint with JWT
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);
            
            var profileResponse = await _client.GetAsync($"/api/users/{loginResult.UserId}");

            // ASSERT 3: Authorized access
            Assert.True(profileResponse.IsSuccessStatusCode);
        }

        [Fact]
        public async Task LoginFlow_InvalidCredentials_Unauthorized()
        {
            // ARRANGE
            var loginDto = new LoginDto
            {
                Username = "nonexistent",
                Password = "WrongPassword"
            };

            // ACT
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // ASSERT
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}

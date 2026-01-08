using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using MessengerContracts.DTOs;

namespace MessengerTests.E2E
{
    /// <summary>
    /// End-to-End test for complete message flow
    /// Tests: Alice sends encrypted message → Server stores → Bob receives and decrypts
    /// </summary>
    public class MessageFlowTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public MessageFlowTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CompleteMessageFlow_AliceToBob_Success()
        {
            // ARRANGE: Create Alice and Bob
            var aliceToken = await RegisterAndLoginUser("alice");
            var bobToken = await RegisterAndLoginUser("bob");

            // Extract user IDs (PSEUDO: parse JWT claims)
            var aliceId = Guid.NewGuid(); // PSEUDO: extract from aliceToken
            var bobId = Guid.NewGuid();   // PSEUDO: extract from bobToken

            // ACT 1: Alice sends encrypted message to Bob
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", aliceToken);

            var messageDto = new MessageDto
            {
                SenderId = aliceId,
                RecipientId = bobId,
                EncryptedContent = "ENCRYPTED_CONTENT_BASE64", // PSEUDO: encrypted with Bob's public key
                Timestamp = DateTime.UtcNow,
                Status = MessageStatus.Sent
            };

            var sendResponse = await _client.PostAsJsonAsync("/api/messages", messageDto);

            // ASSERT 1: Message sent successfully
            Assert.True(sendResponse.IsSuccessStatusCode);
            var sentMessage = await sendResponse.Content.ReadFromJsonAsync<MessageDto>();
            Assert.NotNull(sentMessage);
            Assert.NotEqual(Guid.Empty, sentMessage.Id);

            // ACT 2: Bob retrieves messages
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bobToken);

            var messagesResponse = await _client.GetAsync($"/api/messages/{aliceId}");

            // ASSERT 2: Bob receives message
            Assert.True(messagesResponse.IsSuccessStatusCode);
            var messages = await messagesResponse.Content.ReadFromJsonAsync<List<MessageDto>>();
            Assert.NotNull(messages);
            Assert.Contains(messages, m => m.Id == sentMessage.Id);

            // ACT 3: Bob marks message as read
            var readResponse = await _client.PatchAsync($"/api/messages/{sentMessage.Id}/read", null);

            // ASSERT 3: Message marked as read
            Assert.True(readResponse.IsSuccessStatusCode);

            // ACT 4: Alice checks message status
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", aliceToken);

            var statusResponse = await _client.GetAsync($"/api/messages/{sentMessage.Id}");
            var updatedMessage = await statusResponse.Content.ReadFromJsonAsync<MessageDto>();

            // ASSERT 4: Message status is Read
            Assert.NotNull(updatedMessage);
            Assert.Equal(MessageStatus.Read, updatedMessage.Status);
        }

        private async Task<string> RegisterAndLoginUser(string username)
        {
            // PSEUDO: Register user
            var registerDto = new RegisterUserDto
            {
                Username = username,
                Email = $"{username}@example.com",
                Password = "TestPass123!"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            Assert.True(registerResponse.IsSuccessStatusCode);

            // PSEUDO: Login user
            var loginDto = new LoginDto
            {
                Username = username,
                Password = "TestPass123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            
            return loginResult?.Token ?? throw new Exception("Login failed");
        }
    }
}

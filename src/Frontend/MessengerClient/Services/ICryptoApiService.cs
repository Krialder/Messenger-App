using Refit;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    public interface ICryptoApiService
    {
        [Post("/api/crypto/keys/generate")]
        Task<KeyPairResponse> GenerateKeyPairAsync([Header("Authorization")] string authorization);

        [Post("/api/crypto/layer1/encrypt")]
        Task<EncryptedMessageDto> EncryptTransportAsync([Header("Authorization")] string authorization, [Body] EncryptRequest request);

        [Post("/api/crypto/layer1/decrypt")]
        Task<DecryptResponse> DecryptTransportAsync([Header("Authorization")] string authorization, [Body] DecryptRequest request);

        [Post("/api/crypto/group/encrypt")]
        Task<GroupMessageEncryptionResult> EncryptGroupMessageAsync([Header("Authorization")] string authorization, [Body] GroupEncryptRequest request);

        [Post("/api/crypto/group/decrypt")]
        Task<DecryptResponse> DecryptGroupMessageAsync([Header("Authorization")] string authorization, [Body] GroupDecryptRequest request);
    }
}

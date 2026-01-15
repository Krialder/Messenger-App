using System;
using System.Threading.Tasks;
using Refit;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    public interface ICryptoApiService
    {
        [Post("/api/crypto/keypair")]
        Task<KeyPairResponse> GenerateKeyPairAsync([Header("Authorization")] string authorization);

        [Post("/api/crypto/encrypt")]
        Task<EncryptResponse> EncryptAsync([Body] EncryptRequest request, [Header("Authorization")] string authorization);

        [Post("/api/crypto/decrypt")]
        Task<DecryptResponse> DecryptAsync([Body] DecryptRequest request, [Header("Authorization")] string authorization);

        [Post("/api/crypto/group/encrypt")]
        Task<GroupEncryptResponse> EncryptGroupMessageAsync([Body] GroupEncryptRequest request, [Header("Authorization")] string authorization);

        [Post("/api/crypto/group/decrypt")]
        Task<GroupDecryptResponse> DecryptGroupMessageAsync([Body] GroupDecryptRequest request, [Header("Authorization")] string authorization);
    }

    // Supporting DTOs
    public record KeyPairResponse(
        string PublicKey,  // Base64 encoded
        string PrivateKey  // Base64 encoded
    );

    public record EncryptRequest(
        string Plaintext,
        string RecipientPublicKey
    );

    public record EncryptResponse(
        string EncryptedData,  // Base64 encoded
        string Nonce          // Base64 encoded
    );

    public record DecryptRequest(
        string EncryptedData,  // Base64 encoded
        string SenderPublicKey,
        string RecipientPrivateKey
    );

    public record DecryptResponse(
        string Plaintext
    );

    public record GroupEncryptRequest(
        string Plaintext,
        Guid GroupId
    );

    public record GroupEncryptResponse(
        string EncryptedData,
        Guid GroupId
    );

    public record GroupDecryptRequest(
        string EncryptedData,
        Guid GroupId,
        string UserPrivateKey
    );

    public record GroupDecryptResponse(
        string Plaintext
    );
}

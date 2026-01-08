using System;
using System.Threading.Tasks;
using MessengerContracts.DTOs;

namespace MessengerContracts.Interfaces
{
    /// <summary>
    /// Interface for encryption services
    /// Defines contract for Layer 1, 2, and 3 encryption
    /// </summary>
    public interface ICryptoService
    {
        // Layer 1: E2E Transport Encryption
        Task<EncryptedMessage> EncryptMessageAsync(string plaintext, byte[] recipientPublicKey);
        Task<string> DecryptMessageAsync(EncryptedMessage encryptedMessage, byte[] privateKey);
        
        // Layer 2: Local Storage Encryption
        Task<string> EncryptLocalDataAsync(string plaintext, byte[] masterKey);
        Task<string> DecryptLocalDataAsync(string encryptedData, byte[] masterKey);
        
        // Layer 3: Display Encryption (Optional)
        Task<string> EncryptForDisplayAsync(string plaintext, byte[] deviceKey);
        Task<string> DecryptForDisplayAsync(string encryptedData, byte[] deviceKey, string pin);
    }

    /// <summary>
    /// Encrypted message structure for Layer 1
    /// </summary>
    public class EncryptedMessage
    {
        public byte[] Ciphertext { get; set; } = Array.Empty<byte>();
        public byte[] Nonce { get; set; } = Array.Empty<byte>();
        public byte[] Tag { get; set; } = Array.Empty<byte>();
        public byte[] EphemeralPublicKey { get; set; } = Array.Empty<byte>();
    }
}

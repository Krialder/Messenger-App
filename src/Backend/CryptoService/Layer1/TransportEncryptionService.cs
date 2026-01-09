using System.Security.Cryptography;
using System.Text;
using MessengerContracts.DTOs;
using MessengerContracts.Interfaces;
using Sodium;
using SodiumKeyPair = Sodium.KeyPair;
using DtoKeyPair = MessengerContracts.DTOs.KeyPair;

namespace CryptoService.Layer1;

/// <summary>
/// Implementation of Layer 1 end-to-end transport encryption using ChaCha20-Poly1305 and X25519.
/// Provides forward secrecy through ephemeral key pairs for each message.
/// </summary>
public class TransportEncryptionService : ITransportEncryptionService
{
    private const int NonceSize = 24; // SecretBox requires 24 bytes
    private const int TagSize = 16;
    private const int PublicKeySize = 32;
    private const int PrivateKeySize = 32;

    /// <summary>
    /// Encrypts plaintext for a recipient using ChaCha20-Poly1305 with ephemeral key exchange.
    /// Performance target: &lt; 10ms.
    /// </summary>
    public async Task<EncryptedMessageDto> EncryptAsync(string plaintext, byte[] recipientPublicKey)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            throw new ArgumentException("Plaintext cannot be null or empty.", nameof(plaintext));
        }

        if (recipientPublicKey == null || recipientPublicKey.Length != PublicKeySize)
        {
            throw new ArgumentException($"Recipient public key must be {PublicKeySize} bytes.", nameof(recipientPublicKey));
        }

        return await Task.Run(() =>
        {
            byte[] ephemeralPrivateKey = null!;
            byte[] ephemeralPublicKey = null!;
            byte[] sharedSecret = null!;

            try
            {
                // Generate ephemeral key pair for forward secrecy
                SodiumKeyPair ephemeralKeyPair = PublicKeyBox.GenerateKeyPair();
                ephemeralPublicKey = ephemeralKeyPair.PublicKey;
                ephemeralPrivateKey = ephemeralKeyPair.PrivateKey;

                // Perform X25519 key exchange
                sharedSecret = ScalarMult.Mult(ephemeralPrivateKey, recipientPublicKey);

                // Generate random nonce (24 bytes for SecretBox)
                byte[] nonce = new byte[NonceSize];
                RandomNumberGenerator.Fill(nonce);

                // Convert plaintext to bytes
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

                // Encrypt using ChaCha20-Poly1305 via SecretBox
                byte[] ciphertext = SecretBox.Create(plaintextBytes, nonce, sharedSecret);

                // Extract tag (last 16 bytes) and ciphertext
                byte[] tag = new byte[TagSize];
                byte[] actualCiphertext = new byte[ciphertext.Length - TagSize];

                Array.Copy(ciphertext, 0, actualCiphertext, 0, actualCiphertext.Length);
                Array.Copy(ciphertext, actualCiphertext.Length, tag, 0, TagSize);

                return new EncryptedMessageDto
                {
                    Nonce = nonce,
                    Ciphertext = actualCiphertext,
                    Tag = tag,
                    EphemeralPublicKey = ephemeralPublicKey
                };
            }
            finally
            {
                // Securely erase ephemeral private key and shared secret (forward secrecy)
                if (ephemeralPrivateKey != null)
                {
                    CryptographicOperations.ZeroMemory(ephemeralPrivateKey);
                }

                if (sharedSecret != null)
                {
                    CryptographicOperations.ZeroMemory(sharedSecret);
                }
            }
        });
    }

    /// <summary>
    /// Decrypts an encrypted message using the recipient's private key.
    /// Performance target: &lt; 10ms.
    /// </summary>
    public async Task<string> DecryptAsync(EncryptedMessageDto encryptedMessage, byte[] privateKey)
    {
        if (encryptedMessage == null)
        {
            throw new ArgumentNullException(nameof(encryptedMessage));
        }

        if (privateKey == null || privateKey.Length != PrivateKeySize)
        {
            throw new ArgumentException($"Private key must be {PrivateKeySize} bytes.", nameof(privateKey));
        }

        if (encryptedMessage.Nonce.Length != NonceSize)
        {
            throw new ArgumentException($"Nonce must be {NonceSize} bytes.", nameof(encryptedMessage));
        }

        if (encryptedMessage.Tag.Length != TagSize)
        {
            throw new ArgumentException($"Tag must be {TagSize} bytes.", nameof(encryptedMessage));
        }

        if (encryptedMessage.EphemeralPublicKey.Length != PublicKeySize)
        {
            throw new ArgumentException($"Ephemeral public key must be {PublicKeySize} bytes.", nameof(encryptedMessage));
        }

        return await Task.Run(() =>
        {
            byte[] sharedSecret = null!;

            try
            {
                // Perform X25519 key exchange with ephemeral public key
                sharedSecret = ScalarMult.Mult(privateKey, encryptedMessage.EphemeralPublicKey);

                // Reconstruct ciphertext with tag
                byte[] ciphertextWithTag = new byte[encryptedMessage.Ciphertext.Length + encryptedMessage.Tag.Length];
                Array.Copy(encryptedMessage.Ciphertext, 0, ciphertextWithTag, 0, encryptedMessage.Ciphertext.Length);
                Array.Copy(encryptedMessage.Tag, 0, ciphertextWithTag, encryptedMessage.Ciphertext.Length, encryptedMessage.Tag.Length);

                // Decrypt using ChaCha20-Poly1305
                byte[] plaintextBytes = SecretBox.Open(ciphertextWithTag, encryptedMessage.Nonce, sharedSecret);

                return Encoding.UTF8.GetString(plaintextBytes);
            }
            catch (CryptographicException)
            {
                throw new CryptographicException("Decryption failed. Invalid ciphertext, key, or authentication tag.");
            }
            finally
            {
                // Securely erase shared secret
                if (sharedSecret != null)
                {
                    CryptographicOperations.ZeroMemory(sharedSecret);
                }
            }
        });
    }

    /// <summary>
    /// Generates a new X25519 key pair.
    /// </summary>
    public Task<DtoKeyPair> GenerateKeyPairAsync()
    {
        return Task.Run(() =>
        {
            SodiumKeyPair keyPair = PublicKeyBox.GenerateKeyPair();

            return new DtoKeyPair
            {
                PublicKey = keyPair.PublicKey,
                PrivateKey = keyPair.PrivateKey
            };
        });
    }

    /// <summary>
    /// Rotates the cryptographic key for a user.
    /// This would typically involve generating a new key pair and storing it in KeyManagementService.
    /// </summary>
    public async Task RotateKeyAsync(Guid userId)
    {
        // Generate new key pair
        DtoKeyPair newKeyPair = await GenerateKeyPairAsync();

        // TODO: Store new public key in KeyManagementService
        // TODO: Securely store encrypted private key
        // TODO: Revoke old key

        // For now, just ensure the private key is zeroed
        CryptographicOperations.ZeroMemory(newKeyPair.PrivateKey);

        await Task.CompletedTask;
    }
}

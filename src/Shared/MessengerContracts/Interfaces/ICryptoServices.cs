namespace MessengerContracts.Interfaces;

using MessengerContracts.DTOs;

/// <summary>
/// Service for Layer 1 end-to-end transport encryption using ChaCha20-Poly1305 and X25519.
/// </summary>
public interface ITransportEncryptionService
{
    /// <summary>
    /// Encrypts plaintext for a recipient using their public key.
    /// </summary>
    /// <param name="plaintext">The message to encrypt.</param>
    /// <param name="recipientPublicKey">The recipient's 32-byte X25519 public key.</param>
    /// <returns>Encrypted message with nonce, ciphertext, tag, and ephemeral public key.</returns>
    Task<EncryptedMessageDto> EncryptAsync(string plaintext, byte[] recipientPublicKey);

    /// <summary>
    /// Decrypts an encrypted message using the recipient's private key.
    /// </summary>
    /// <param name="encryptedMessage">The encrypted message.</param>
    /// <param name="privateKey">The recipient's 32-byte X25519 private key.</param>
    /// <returns>Decrypted plaintext.</returns>
    Task<string> DecryptAsync(EncryptedMessageDto encryptedMessage, byte[] privateKey);

    /// <summary>
    /// Generates a new X25519 key pair.
    /// </summary>
    /// <returns>A new key pair with public and private keys.</returns>
    Task<KeyPair> GenerateKeyPairAsync();

    /// <summary>
    /// Rotates the cryptographic key for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>Task representing the key rotation operation.</returns>
    Task RotateKeyAsync(Guid userId);
}

/// <summary>
/// Service for Layer 2 local storage encryption using AES-256-GCM and Argon2id.
/// </summary>
public interface ILocalStorageEncryptionService
{
    /// <summary>
    /// Derives a master key from a password and user salt using Argon2id.
    /// </summary>
    /// <param name="password">User password.</param>
    /// <param name="userSalt">32-byte unique salt for the user.</param>
    /// <returns>32-byte master key.</returns>
    Task<byte[]> DeriveMasterKeyAsync(string password, byte[] userSalt);

    /// <summary>
    /// Encrypts plaintext for local storage using AES-256-GCM.
    /// </summary>
    /// <param name="plaintext">Data to encrypt.</param>
    /// <param name="masterKey">32-byte master key.</param>
    /// <returns>Base64-encoded encrypted data (nonce || ciphertext || tag).</returns>
    Task<string> EncryptLocalDataAsync(string plaintext, byte[] masterKey);

    /// <summary>
    /// Decrypts locally stored data using AES-256-GCM.
    /// </summary>
    /// <param name="encryptedData">Base64-encoded encrypted data.</param>
    /// <param name="masterKey">32-byte master key.</param>
    /// <returns>Decrypted plaintext.</returns>
    Task<string> DecryptLocalDataAsync(string encryptedData, byte[] masterKey);

    /// <summary>
    /// Stores a private key encrypted with the master key.
    /// </summary>
    /// <param name="privateKey">The private key to store.</param>
    /// <param name="masterKey">32-byte master key.</param>
    /// <returns>Task representing the storage operation.</returns>
    Task StorePrivateKeyAsync(byte[] privateKey, byte[] masterKey);

    /// <summary>
    /// Loads and decrypts a private key using the master key.
    /// </summary>
    /// <param name="masterKey">32-byte master key.</param>
    /// <returns>Decrypted private key.</returns>
    Task<byte[]> LoadPrivateKeyAsync(byte[] masterKey);
}

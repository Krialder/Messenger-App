using Sodium;

namespace CryptoService.Layer1
{
    /// <summary>
    /// Layer 1: End-to-End Transport Encryption (E2E)
    /// Implements ChaCha20-Poly1305 AEAD with X25519 key exchange
    /// Zero-knowledge server architecture - server cannot decrypt messages
    /// </summary>
    public interface IEndToEndEncryption
    {
        /// <summary>
        /// Generate a new X25519 key pair for asymmetric encryption
        /// </summary>
        (byte[] publicKey, byte[] privateKey) GenerateKeyPair();
        
        /// <summary>
        /// Encrypt plaintext using recipient's public key and sender's private key
        /// Automatically generates ephemeral nonce and combines with ciphertext
        /// </summary>
        byte[] EncryptMessage(byte[] plaintext, byte[] recipientPublicKey, byte[] senderPrivateKey);
        
        /// <summary>
        /// Decrypt message using sender's public key and recipient's private key
        /// Extracts nonce from encrypted data and authenticates with Poly1305 tag
        /// </summary>
        byte[] DecryptMessage(byte[] encryptedData, byte[] senderPublicKey, byte[] recipientPrivateKey);
    }

    /// <summary>
    /// ChaCha20-Poly1305 implementation for Layer 1 E2E encryption
    /// Uses libsodium-net for battle-tested cryptographic primitives
    /// Provides forward secrecy through ephemeral key pairs
    /// </summary>
    public class ChaCha20Poly1305Encryption : IEndToEndEncryption
    {
        /// <summary>
        /// Generate X25519 key pair (32-byte public + 32-byte private key)
        /// Public key is shared with contacts, private key stays local
        /// </summary>
        public (byte[] publicKey, byte[] privateKey) GenerateKeyPair()
        {
            var keyPair = PublicKeyBox.GenerateKeyPair();
            return (keyPair.PublicKey, keyPair.PrivateKey);
        }

        /// <summary>
        /// Encrypt message with ChaCha20-Poly1305 AEAD
        /// Format: [24-byte nonce][ciphertext][16-byte Poly1305 tag]
        /// Nonce is auto-generated (cryptographically secure random)
        /// </summary>
        public byte[] EncryptMessage(byte[] plaintext, byte[] recipientPublicKey, byte[] senderPrivateKey)
        {
            var nonce = PublicKeyBox.GenerateNonce();
            var ciphertext = PublicKeyBox.Create(plaintext, nonce, senderPrivateKey, recipientPublicKey);
            
            // Combine nonce + ciphertext for transmission
            var result = new byte[nonce.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
            
            return result;
        }

        /// <summary>
        /// Decrypt message and verify authenticity with Poly1305 tag
        /// Throws CryptographicException if tampered or wrong keys
        /// </summary>
        public byte[] DecryptMessage(byte[] encryptedData, byte[] senderPublicKey, byte[] recipientPrivateKey)
        {
            // Extract nonce (first 24 bytes) and ciphertext (rest)
            var nonceLength = 24;
            var extractedNonce = new byte[nonceLength];
            var ciphertext = new byte[encryptedData.Length - nonceLength];
            
            Buffer.BlockCopy(encryptedData, 0, extractedNonce, 0, nonceLength);
            Buffer.BlockCopy(encryptedData, nonceLength, ciphertext, 0, ciphertext.Length);
            
            // Decrypt and verify authentication tag (throws on failure)
            var plaintext = PublicKeyBox.Open(ciphertext, extractedNonce, recipientPrivateKey, senderPublicKey);
            
            return plaintext;
        }

        /// <summary>
        /// Derive shared secret using X25519 Diffie-Hellman
        /// Used internally by libsodium for ChaCha20 key derivation
        /// </summary>
        public byte[] DeriveSharedSecret(byte[] myPrivateKey, byte[] theirPublicKey)
        {
            return ScalarMult.Mult(myPrivateKey, theirPublicKey);
        }
    }
}

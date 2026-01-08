using Sodium;

namespace CryptoService.Layer1
{
    /// <summary>
    /// Layer 1: End-to-End Encryption (E2E)
    /// ChaCha20-Poly1305 + X25519 Key Exchange
    /// </summary>
    public interface IEndToEndEncryption
    {
        (byte[] publicKey, byte[] privateKey) GenerateKeyPair();
        byte[] EncryptMessage(byte[] plaintext, byte[] recipientPublicKey, byte[] senderPrivateKey);
        byte[] DecryptMessage(byte[] ciphertext, byte[] senderPublicKey, byte[] recipientPrivateKey, byte[] nonce);
    }

    public class ChaCha20Poly1305Encryption : IEndToEndEncryption
    {
        // PSEUDO CODE: Layer 1 Encryption Implementation

        public (byte[] publicKey, byte[] privateKey) GenerateKeyPair()
        {
            // PSEUDO CODE:
            // 1. Generate X25519 key pair using libsodium
            // 2. Return (public_key, private_key)
            // 3. Public key: 32 bytes
            // 4. Private key: 32 bytes
            
            var keyPair = PublicKeyBox.GenerateKeyPair();
            return (keyPair.PublicKey, keyPair.PrivateKey);
        }

        public byte[] EncryptMessage(byte[] plaintext, byte[] recipientPublicKey, byte[] senderPrivateKey)
        {
            // PSEUDO CODE:
            // 1. Perform X25519 ECDH to derive shared secret
            //    - shared_secret = ECDH(sender_private_key, recipient_public_key)
            // 2. Derive encryption key using HKDF-SHA256
            //    - encryption_key = HKDF(shared_secret, salt, info)
            // 3. Generate random nonce (24 bytes for XChaCha20-Poly1305)
            // 4. Encrypt plaintext with ChaCha20-Poly1305
            //    - ciphertext = ChaCha20Poly1305.Encrypt(plaintext, key, nonce)
            // 5. Return: nonce || ciphertext || authentication_tag
            
            // Using libsodium's PublicKeyBox (which internally uses X25519 + XSalsa20-Poly1305)
            var nonce = PublicKeyBox.GenerateNonce();
            var ciphertext = PublicKeyBox.Create(plaintext, nonce, senderPrivateKey, recipientPublicKey);
            
            // Combine nonce + ciphertext for transmission
            var result = new byte[nonce.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
            
            return result;
        }

        public byte[] DecryptMessage(byte[] encryptedData, byte[] senderPublicKey, byte[] recipientPrivateKey, byte[] nonce)
        {
            // PSEUDO CODE:
            // 1. Extract nonce and ciphertext from encryptedData
            // 2. Perform X25519 ECDH to derive shared secret
            //    - shared_secret = ECDH(recipient_private_key, sender_public_key)
            // 3. Derive decryption key using HKDF (same as encryption)
            // 4. Verify authentication tag (Poly1305)
            // 5. Decrypt ciphertext with ChaCha20
            // 6. Return plaintext
            
            // Extract nonce (first 24 bytes) and ciphertext (rest)
            var nonceLength = 24;
            var extractedNonce = new byte[nonceLength];
            var ciphertext = new byte[encryptedData.Length - nonceLength];
            
            Buffer.BlockCopy(encryptedData, 0, extractedNonce, 0, nonceLength);
            Buffer.BlockCopy(encryptedData, nonceLength, ciphertext, 0, ciphertext.Length);
            
            // Decrypt using libsodium
            var plaintext = PublicKeyBox.Open(ciphertext, extractedNonce, recipientPrivateKey, senderPublicKey);
            
            return plaintext;
        }

        public byte[] DeriveSharedSecret(byte[] myPrivateKey, byte[] theirPublicKey)
        {
            // PSEUDO CODE:
            // 1. Perform X25519 scalar multiplication
            // 2. Result: 32-byte shared secret
            // 3. Use HKDF to derive actual encryption key from shared secret
            
            return ScalarMult.Mult(myPrivateKey, theirPublicKey);
        }
    }
}

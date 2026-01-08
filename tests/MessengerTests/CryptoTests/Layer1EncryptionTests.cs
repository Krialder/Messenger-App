using Xunit;
using CryptoService.Layer1;

namespace MessengerTests.CryptoTests
{
    public class Layer1EncryptionTests
    {
        // PSEUDO CODE: Unit Tests for Layer 1 Encryption

        [Fact]
        public void GenerateKeyPair_ShouldReturn_32ByteKeys()
        {
            // PSEUDO CODE:
            // 1. Generate key pair
            // 2. Assert public key is 32 bytes
            // 3. Assert private key is 32 bytes

            var encryption = new ChaCha20Poly1305Encryption();
            var (publicKey, privateKey) = encryption.GenerateKeyPair();

            Assert.Equal(32, publicKey.Length);
            Assert.Equal(32, privateKey.Length);
        }

        [Fact]
        public void EncryptDecrypt_RoundTrip_ShouldSucceed()
        {
            // PSEUDO CODE:
            // 1. Generate sender and recipient key pairs
            // 2. Encrypt message from sender to recipient
            // 3. Decrypt message at recipient
            // 4. Assert decrypted = original plaintext

            var encryption = new ChaCha20Poly1305Encryption();
            
            var (senderPublic, senderPrivate) = encryption.GenerateKeyPair();
            var (recipientPublic, recipientPrivate) = encryption.GenerateKeyPair();
            
            var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, this is a secret message!");
            
            var ciphertext = encryption.EncryptMessage(plaintext, recipientPublic, senderPrivate);
            
            // Extract nonce from ciphertext
            var nonce = new byte[24];
            System.Buffer.BlockCopy(ciphertext, 0, nonce, 0, 24);
            
            var decrypted = encryption.DecryptMessage(ciphertext, senderPublic, recipientPrivate, nonce);
            
            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void Encrypt_WithDifferentKeys_ShouldProduceDifferentCiphertext()
        {
            // PSEUDO CODE:
            // 1. Encrypt same message with different recipient keys
            // 2. Assert ciphertexts are different

            var encryption = new ChaCha20Poly1305Encryption();
            
            var (senderPublic, senderPrivate) = encryption.GenerateKeyPair();
            var (recipient1Public, _) = encryption.GenerateKeyPair();
            var (recipient2Public, _) = encryption.GenerateKeyPair();
            
            var plaintext = System.Text.Encoding.UTF8.GetBytes("Same message");
            
            var ciphertext1 = encryption.EncryptMessage(plaintext, recipient1Public, senderPrivate);
            var ciphertext2 = encryption.EncryptMessage(plaintext, recipient2Public, senderPrivate);
            
            Assert.NotEqual(ciphertext1, ciphertext2);
        }

        [Fact]
        public void Decrypt_WithWrongKey_ShouldThrowException()
        {
            // PSEUDO CODE:
            // 1. Encrypt message
            // 2. Try to decrypt with wrong private key
            // 3. Assert exception is thrown

            var encryption = new ChaCha20Poly1305Encryption();
            
            var (senderPublic, senderPrivate) = encryption.GenerateKeyPair();
            var (recipientPublic, recipientPrivate) = encryption.GenerateKeyPair();
            var (_, wrongPrivateKey) = encryption.GenerateKeyPair();
            
            var plaintext = System.Text.Encoding.UTF8.GetBytes("Secret");
            var ciphertext = encryption.EncryptMessage(plaintext, recipientPublic, senderPrivate);
            
            var nonce = new byte[24];
            System.Buffer.BlockCopy(ciphertext, 0, nonce, 0, 24);
            
            Assert.Throws<Exception>(() => 
                encryption.DecryptMessage(ciphertext, senderPublic, wrongPrivateKey, nonce));
        }
    }
}

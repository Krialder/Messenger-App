using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using Xunit;
using CryptoService.Layer1;
using CryptoService.Layer2;
using CryptoService.Services;
using MessengerContracts.DTOs;

namespace MessengerTests.IntegrationTests
{
    /// <summary>
    /// Integration tests for end-to-end encryption pipeline.
    /// Tests the complete flow: Layer 2 → Layer 1 → Server → Layer 1 → Layer 2
    /// </summary>
    public class EndToEndEncryptionTests
    {
        private readonly TransportEncryptionService _layer1Service;
        private readonly LocalStorageEncryptionService _layer2Service;
        private readonly GroupEncryptionService _groupService;

        public EndToEndEncryptionTests()
        {
            _layer1Service = new TransportEncryptionService();
            _layer2Service = new LocalStorageEncryptionService();
            _groupService = new GroupEncryptionService();
        }

        [Fact]
        public async Task FullEncryptionPipeline_UserAToUserB_Success()
        {
            // Arrange
            var plaintext = "Hello, this is a secure message!";

            // User A (sender)
            var userAKeyPair = await _layer1Service.GenerateKeyPairAsync();
            var userAPassword = "UserA_SecurePassword123!";
            var userASalt = new byte[32];
            RandomNumberGenerator.Fill(userASalt);
            var userAMasterKey = await _layer2Service.DeriveMasterKeyAsync(userAPassword, userASalt);

            // User B (recipient)
            var userBKeyPair = await _layer1Service.GenerateKeyPairAsync();
            var userBPassword = "UserB_SecurePassword456!";
            var userBSalt = new byte[32];
            RandomNumberGenerator.Fill(userBSalt);
            var userBMasterKey = await _layer2Service.DeriveMasterKeyAsync(userBPassword, userBSalt);

            // Act

            // Step 1: User A encrypts locally (Layer 2)
            var layer2Encrypted = await _layer2Service.EncryptLocalDataAsync(plaintext, userAMasterKey);

            // Step 2: User A encrypts for transport to User B (Layer 1)
            var layer1Encrypted = await _layer1Service.EncryptAsync(layer2Encrypted, userBKeyPair.PublicKey);

            // Simulate server storage (only encrypted data)
            var storedData = layer1Encrypted;

            // Step 3: User B decrypts transport (Layer 1)
            var layer1Decrypted = await _layer1Service.DecryptAsync(storedData, userBKeyPair.PrivateKey);

            // Step 4: User B decrypts locally (Layer 2)
            // NOTE: In real scenario, User B would use their own master key for their local storage
            // For this test, we decrypt what User A encrypted (simulating User A reading their sent message)
            var finalPlaintext = await _layer2Service.DecryptLocalDataAsync(layer1Decrypted, userAMasterKey);

            // Assert
            Assert.Equal(plaintext, finalPlaintext);
            Assert.NotEqual(plaintext, layer2Encrypted);
            Assert.NotEqual(layer2Encrypted, Convert.ToBase64String(layer1Encrypted.Ciphertext));
        }

        [Fact]
        public async Task GroupMessage_EncryptDecrypt_MultipleMembers()
        {
            // Arrange
            var plaintext = "Group message to all members!";
            var memberCount = 5;

            // Generate member key pairs
            var members = new List<(KeyPair keyPair, MemberPublicKey pubKey)>();
            for (int i = 0; i < memberCount; i++)
            {
                var keyPair = await _layer1Service.GenerateKeyPairAsync();
                members.Add((
                    keyPair,
                    new MemberPublicKey
                    {
                        UserId = Guid.NewGuid(),
                        PublicKey = keyPair.PublicKey
                    }
                ));
            }

            // Act - Encrypt for group
            var encryptionResult = await _groupService.EncryptForGroupAsync(
                plaintext,
                members.Select(m => m.pubKey).ToList());

            // Assert
            Assert.NotNull(encryptionResult);
            Assert.NotEmpty(encryptionResult.EncryptedContent);
            Assert.Equal(memberCount, encryptionResult.EncryptedKeys.Count);

            // Verify each member can decrypt
            for (int i = 0; i < memberCount; i++)
            {
                var encryptedKey = encryptionResult.EncryptedKeys[i];
                var memberPrivateKey = members[i].keyPair.PrivateKey;

                var decrypted = await _groupService.DecryptGroupMessageAsync(
                    encryptionResult.EncryptedContent,
                    encryptionResult.Nonce,
                    encryptionResult.Tag,
                    encryptedKey.EncryptedGroupKey,
                    memberPrivateKey);

                Assert.Equal(plaintext, decrypted);
            }
        }

        [Fact]
        public async Task Layer1_ForwardSecrecy_DifferentNoncesEachTime()
        {
            // Arrange
            var plaintext = "Test message";
            var recipientKeyPair = await _layer1Service.GenerateKeyPairAsync();

            // Act - Encrypt same message twice
            var encrypted1 = await _layer1Service.EncryptAsync(plaintext, recipientKeyPair.PublicKey);
            var encrypted2 = await _layer1Service.EncryptAsync(plaintext, recipientKeyPair.PublicKey);

            // Assert - Different nonces and ephemeral keys (forward secrecy)
            Assert.NotEqual(encrypted1.Nonce, encrypted2.Nonce);
            Assert.NotEqual(encrypted1.EphemeralPublicKey, encrypted2.EphemeralPublicKey);
            Assert.NotEqual(encrypted1.Ciphertext, encrypted2.Ciphertext);

            // But both should decrypt to same plaintext
            var decrypted1 = await _layer1Service.DecryptAsync(encrypted1, recipientKeyPair.PrivateKey);
            var decrypted2 = await _layer1Service.DecryptAsync(encrypted2, recipientKeyPair.PrivateKey);
            Assert.Equal(plaintext, decrypted1);
            Assert.Equal(plaintext, decrypted2);
        }

        [Fact]
        public async Task Layer1_TamperedCiphertext_ThrowsException()
        {
            // Arrange
            var plaintext = "Secret message";
            var keyPair = await _layer1Service.GenerateKeyPairAsync();
            var encrypted = await _layer1Service.EncryptAsync(plaintext, keyPair.PublicKey);

            // Act - Tamper with ciphertext
            encrypted.Ciphertext[0] ^= 0xFF;

            // Assert
            await Assert.ThrowsAsync<CryptographicException>(async () =>
                await _layer1Service.DecryptAsync(encrypted, keyPair.PrivateKey));
        }

        [Fact]
        public async Task Layer1_TamperedTag_ThrowsException()
        {
            // Arrange
            var plaintext = "Secret message";
            var keyPair = await _layer1Service.GenerateKeyPairAsync();
            var encrypted = await _layer1Service.EncryptAsync(plaintext, keyPair.PublicKey);

            // Act - Tamper with tag
            encrypted.Tag[0] ^= 0xFF;

            // Assert
            await Assert.ThrowsAsync<CryptographicException>(async () =>
                await _layer1Service.DecryptAsync(encrypted, keyPair.PrivateKey));
        }

        [Fact]
        public async Task Layer2_MasterKeyDerivation_SameSaltSameKey()
        {
            // Arrange
            var password = "UserPassword123!";
            var salt = new byte[32];
            RandomNumberGenerator.Fill(salt);

            // Act
            var masterKey1 = await _layer2Service.DeriveMasterKeyAsync(password, salt);
            var masterKey2 = await _layer2Service.DeriveMasterKeyAsync(password, salt);

            // Assert - Same password + same salt → same key
            Assert.Equal(masterKey1, masterKey2);
        }

        [Fact]
        public async Task Layer2_MasterKeyDerivation_DifferentSaltDifferentKey()
        {
            // Arrange
            var password = "UserPassword123!";
            var salt1 = new byte[32];
            var salt2 = new byte[32];
            RandomNumberGenerator.Fill(salt1);
            RandomNumberGenerator.Fill(salt2);

            // Act
            var masterKey1 = await _layer2Service.DeriveMasterKeyAsync(password, salt1);
            var masterKey2 = await _layer2Service.DeriveMasterKeyAsync(password, salt2);

            // Assert - Same password, different salts → different keys
            Assert.NotEqual(masterKey1, masterKey2);
        }

        [Fact]
        public async Task Layer2_EncryptDecrypt_RoundTrip()
        {
            // Arrange
            var plaintext = "Local storage data";
            var password = "TestPassword123!";
            var salt = new byte[32];
            RandomNumberGenerator.Fill(salt);
            var masterKey = await _layer2Service.DeriveMasterKeyAsync(password, salt);

            // Act
            var encrypted = await _layer2Service.EncryptLocalDataAsync(plaintext, masterKey);
            var decrypted = await _layer2Service.DecryptLocalDataAsync(encrypted, masterKey);

            // Assert
            Assert.Equal(plaintext, decrypted);
            Assert.NotEqual(plaintext, encrypted);
        }

        [Fact]
        public async Task Layer2_TamperedData_ThrowsException()
        {
            // Arrange
            var plaintext = "Secret data";
            var password = "TestPassword123!";
            var salt = new byte[32];
            RandomNumberGenerator.Fill(salt);
            var masterKey = await _layer2Service.DeriveMasterKeyAsync(password, salt);
            var encrypted = await _layer2Service.EncryptLocalDataAsync(plaintext, masterKey);

            // Act - Tamper with encrypted data
            var tamperedBytes = Convert.FromBase64String(encrypted);
            tamperedBytes[20] ^= 0xFF; // Flip a bit in the middle
            var tamperedEncrypted = Convert.ToBase64String(tamperedBytes);

            // Assert
            await Assert.ThrowsAsync<CryptographicException>(async () =>
                await _layer2Service.DecryptLocalDataAsync(tamperedEncrypted, masterKey));
        }

        [Fact]
        public async Task GroupEncryption_KeyRotation_NewKeysDifferent()
        {
            // Arrange & Act
            var key1 = await _groupService.GenerateNewGroupKeyAsync();
            var key2 = await _groupService.GenerateNewGroupKeyAsync();
            var key3 = await _groupService.GenerateNewGroupKeyAsync();

            // Assert - All keys should be different (random)
            Assert.NotEqual(key1, key2);
            Assert.NotEqual(key2, key3);
            Assert.NotEqual(key1, key3);

            // All keys should be 32 bytes (AES-256)
            Assert.Equal(32, key1.Length);
            Assert.Equal(32, key2.Length);
            Assert.Equal(32, key3.Length);
        }

        [Fact]
        public async Task GroupEncryption_Performance_100Members()
        {
            // Arrange
            var plaintext = "Performance test message for group encryption";
            var memberPublicKeys = new List<MemberPublicKey>();

            for (int i = 0; i < 100; i++)
            {
                var keyPair = await _layer1Service.GenerateKeyPairAsync();
                memberPublicKeys.Add(new MemberPublicKey
                {
                    UserId = Guid.NewGuid(),
                    PublicKey = keyPair.PublicKey
                });
            }

            // Act
            var startTime = DateTime.UtcNow;
            var result = await _groupService.EncryptForGroupAsync(plaintext, memberPublicKeys);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Assert
            Assert.True(duration < 2000, $"Encryption for 100 members took {duration}ms, expected < 2000ms");
            Assert.Equal(100, result.EncryptedKeys.Count);
        }

        [Fact]
        public async Task NoPlaintextLeak_EncryptedDataDoesNotContainPlaintext()
        {
            // Arrange
            var plaintext = "TOP SECRET MESSAGE 123456";
            var keyPair = await _layer1Service.GenerateKeyPairAsync();

            // Act
            var encrypted = await _layer1Service.EncryptAsync(plaintext, keyPair.PublicKey);

            // Convert ciphertext to string for searching
            var ciphertextString = Convert.ToBase64String(encrypted.Ciphertext);

            // Assert - Plaintext should NOT appear in ciphertext
            Assert.DoesNotContain("TOP SECRET", ciphertextString);
            Assert.DoesNotContain("MESSAGE", ciphertextString);
            Assert.DoesNotContain("123456", ciphertextString);
        }
    }
}

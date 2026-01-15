using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using CryptoService.Layer1;
using CryptoService.Layer2;

namespace MessengerTests.Performance
{
    /// <summary>
    /// Performance benchmarks for cryptographic operations
    /// Ensures encryption/decryption meets performance targets
    /// </summary>
    public class CryptoPerformanceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TransportEncryptionService _layer1Service;
        private readonly LocalStorageEncryptionService _layer2Service;

        public CryptoPerformanceTests(ITestOutputHelper output)
        {
            _output = output;
            _layer1Service = new TransportEncryptionService();
            _layer2Service = new LocalStorageEncryptionService();
        }

        /// <summary>
        /// Layer 1: E2E Transport Encryption Performance
        /// Target: < 100ms per message (avg over 1000 messages)
        /// </summary>
        [Fact]
        public async Task Layer1_Encryption_AverageUnder100ms()
        {
            // Arrange
            var keyPair = await _layer1Service.GenerateKeyPairAsync();
            string plaintext = "Test message for performance benchmark";
            int iterations = 1000;

            // Act
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                await _layer1Service.EncryptAsync(plaintext, keyPair.PublicKey);
            }
            sw.Stop();

            // Calculate
            double avgMs = sw.ElapsedMilliseconds / (double)iterations;
            _output.WriteLine($"Layer 1 Encryption: {avgMs:F2}ms avg ({iterations} iterations)");

            // Assert
            Assert.True(avgMs < 100, $"Average encryption time {avgMs:F2}ms exceeds target of 100ms");
        }

        /// <summary>
        /// Layer 1: E2E Transport Decryption Performance
        /// Target: < 100ms per message
        /// </summary>
        [Fact]
        public async Task Layer1_Decryption_AverageUnder100ms()
        {
            // Arrange
            var keyPair = await _layer1Service.GenerateKeyPairAsync();
            string plaintext = "Test message for performance benchmark";
            var encrypted = await _layer1Service.EncryptAsync(plaintext, keyPair.PublicKey);
            int iterations = 1000;

            // Act
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                await _layer1Service.DecryptAsync(encrypted, keyPair.PrivateKey);
            }
            sw.Stop();

            // Calculate
            double avgMs = sw.ElapsedMilliseconds / (double)iterations;
            _output.WriteLine($"Layer 1 Decryption: {avgMs:F2}ms avg ({iterations} iterations)");

            // Assert
            Assert.True(avgMs < 100, $"Average decryption time {avgMs:F2}ms exceeds target of 100ms");
        }

        /// <summary>
        /// Layer 2: Local Storage Encryption Performance
        /// Target: < 10ms per operation
        /// </summary>
        [Fact]
        public async Task Layer2_Encryption_AverageUnder10ms()
        {
            // Arrange
            byte[] masterKey = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(masterKey);
            string plaintext = "Test data for local storage encryption";
            int iterations = 1000;

            // Act
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                await _layer2Service.EncryptLocalDataAsync(plaintext, masterKey);
            }
            sw.Stop();

            // Calculate
            double avgMs = sw.ElapsedMilliseconds / (double)iterations;
            _output.WriteLine($"Layer 2 Encryption: {avgMs:F2}ms avg ({iterations} iterations)");

            // Assert
            Assert.True(avgMs < 10, $"Average encryption time {avgMs:F2}ms exceeds target of 10ms");
        }

        /// <summary>
        /// Layer 2: Local Storage Decryption Performance
        /// Target: < 10ms per operation
        /// </summary>
        [Fact]
        public async Task Layer2_Decryption_AverageUnder10ms()
        {
            // Arrange
            byte[] masterKey = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(masterKey);
            string plaintext = "Test data for local storage encryption";
            string encrypted = await _layer2Service.EncryptLocalDataAsync(plaintext, masterKey);
            int iterations = 1000;

            // Act
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                await _layer2Service.DecryptLocalDataAsync(encrypted, masterKey);
            }
            sw.Stop();

            // Calculate
            double avgMs = sw.ElapsedMilliseconds / (double)iterations;
            _output.WriteLine($"Layer 2 Decryption: {avgMs:F2}ms avg ({iterations} iterations)");

            // Assert
            Assert.True(avgMs < 10, $"Average decryption time {avgMs:F2}ms exceeds target of 10ms");
        }

        /// <summary>
        /// Master Key Derivation Performance
        /// Target: 100-200ms (intentionally slow - Argon2id)
        /// </summary>
        [Fact]
        public async Task Layer2_MasterKeyDerivation_WithinTarget()
        {
            // Arrange
            string password = "TestPassword123!";
            byte[] salt = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(salt);
            int iterations = 10;

            // Act
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                await _layer2Service.DeriveMasterKeyAsync(password, salt);
            }
            sw.Stop();

            // Calculate
            double avgMs = sw.ElapsedMilliseconds / (double)iterations;
            _output.WriteLine($"Master Key Derivation: {avgMs:F2}ms avg ({iterations} iterations)");

            // Assert (should be slow for security)
            Assert.True(avgMs >= 50, $"Key derivation too fast ({avgMs:F2}ms) - security risk");
            Assert.True(avgMs <= 500, $"Key derivation too slow ({avgMs:F2}ms) - usability issue");
        }

        /// <summary>
        /// Full Encryption Stack Performance (Layer 1 + Layer 2)
        /// Target: < 110ms combined
        /// </summary>
        [Fact]
        public async Task FullStack_EncryptionDecryption_CombinedTarget()
        {
            // Arrange
            var keyPair = await _layer1Service.GenerateKeyPairAsync();
            byte[] masterKey = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(masterKey);
            string plaintext = "Full stack test message";
            int iterations = 100;

            // Act
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                // Layer 2: Encrypt for local storage
                string localEncrypted = await _layer2Service.EncryptLocalDataAsync(plaintext, masterKey);
                
                // Layer 1: Encrypt for transport
                var transportEncrypted = await _layer1Service.EncryptAsync(localEncrypted, keyPair.PublicKey);
                
                // Layer 1: Decrypt transport
                string localDecrypted = await _layer1Service.DecryptAsync(transportEncrypted, keyPair.PrivateKey);
                
                // Layer 2: Decrypt local storage
                string finalPlaintext = await _layer2Service.DecryptLocalDataAsync(localDecrypted, masterKey);
            }
            sw.Stop();

            // Calculate
            double avgMs = sw.ElapsedMilliseconds / (double)iterations;
            _output.WriteLine($"Full Stack (Layer 1+2): {avgMs:F2}ms avg ({iterations} iterations)");

            // Assert
            Assert.True(avgMs < 110, $"Full stack time {avgMs:F2}ms exceeds target of 110ms");
        }

        /// <summary>
        /// Key Generation Performance
        /// Target: < 10ms
        /// </summary>
        [Fact]
        public async Task KeyGeneration_AverageUnder10ms()
        {
            // Arrange
            int iterations = 100;

            // Act
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                await _layer1Service.GenerateKeyPairAsync();
            }
            sw.Stop();

            // Calculate
            double avgMs = sw.ElapsedMilliseconds / (double)iterations;
            _output.WriteLine($"Key Generation: {avgMs:F2}ms avg ({iterations} iterations)");

            // Assert
            Assert.True(avgMs < 10, $"Key generation time {avgMs:F2}ms exceeds target of 10ms");
        }
    }
}

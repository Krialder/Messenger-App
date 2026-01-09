using System.Diagnostics;
using System.Security.Cryptography;
using CryptoService.Layer1;
using FluentAssertions;
using MessengerContracts.DTOs;
using Xunit;

namespace MessengerTests.CryptoTests;

/// <summary>
/// Tests for Layer 1 transport encryption (ChaCha20-Poly1305 + X25519).
/// </summary>
public class TransportEncryptionTests
{
    private readonly TransportEncryptionService _service;

    public TransportEncryptionTests()
    {
        _service = new TransportEncryptionService();
    }

    [Fact]
    public async Task EncryptDecrypt_RoundTrip_Success()
    {
        // Arrange
        string originalMessage = "This is a secret message for testing E2E encryption!";
        var keyPair = await _service.GenerateKeyPairAsync();

        // Act
        var encrypted = await _service.EncryptAsync(originalMessage, keyPair.PublicKey);
        var decrypted = await _service.DecryptAsync(encrypted, keyPair.PrivateKey);

        // Assert
        decrypted.Should().Be(originalMessage);
        encrypted.Ciphertext.Should().NotBeEmpty();
        encrypted.Nonce.Should().HaveCount(24); // SecretBox uses 24 bytes
        encrypted.Tag.Should().HaveCount(16);
        encrypted.EphemeralPublicKey.Should().HaveCount(32);
    }

    [Fact]
    public async Task ForwardSecrecy_DifferentKeys_EachMessage()
    {
        // Arrange
        string message1 = "First message";
        string message2 = "Second message";
        var recipientKeyPair = await _service.GenerateKeyPairAsync();

        // Act
        var encrypted1 = await _service.EncryptAsync(message1, recipientKeyPair.PublicKey);
        var encrypted2 = await _service.EncryptAsync(message2, recipientKeyPair.PublicKey);

        // Assert - Each message should have different ephemeral keys
        encrypted1.EphemeralPublicKey.Should().NotBeEquivalentTo(encrypted2.EphemeralPublicKey);
        encrypted1.Nonce.Should().NotBeEquivalentTo(encrypted2.Nonce);

        // Both should still decrypt correctly
        var decrypted1 = await _service.DecryptAsync(encrypted1, recipientKeyPair.PrivateKey);
        var decrypted2 = await _service.DecryptAsync(encrypted2, recipientKeyPair.PrivateKey);

        decrypted1.Should().Be(message1);
        decrypted2.Should().Be(message2);
    }

    [Fact]
    public async Task Performance_EncryptionUnder10ms()
    {
        // Arrange
        string message = "Performance test message";
        var keyPair = await _service.GenerateKeyPairAsync();
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        await _service.EncryptAsync(message, keyPair.PublicKey);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10, "Encryption should complete in under 10ms");
    }

    [Fact]
    public async Task Performance_DecryptionUnder10ms()
    {
        // Arrange
        string message = "Performance test message";
        var keyPair = await _service.GenerateKeyPairAsync();
        var encrypted = await _service.EncryptAsync(message, keyPair.PublicKey);
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        await _service.DecryptAsync(encrypted, keyPair.PrivateKey);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10, "Decryption should complete in under 10ms");
    }

    [Fact]
    public async Task Encrypt_WithInvalidPublicKey_ThrowsException()
    {
        // Arrange
        string message = "Test message";
        byte[] invalidPublicKey = new byte[16]; // Wrong size (should be 32)

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.EncryptAsync(message, invalidPublicKey));
    }

    [Fact]
    public async Task Decrypt_WithInvalidPrivateKey_ThrowsException()
    {
        // Arrange
        var keyPair = await _service.GenerateKeyPairAsync();
        var encrypted = await _service.EncryptAsync("Test", keyPair.PublicKey);
        byte[] invalidPrivateKey = new byte[16]; // Wrong size

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.DecryptAsync(encrypted, invalidPrivateKey));
    }

    [Fact]
    public async Task Decrypt_WithTamperedCiphertext_ThrowsCryptographicException()
    {
        // Arrange
        string message = "Original message";
        var keyPair = await _service.GenerateKeyPairAsync();
        var encrypted = await _service.EncryptAsync(message, keyPair.PublicKey);

        // Tamper with ciphertext
        encrypted.Ciphertext[0] ^= 0xFF;

        // Act & Assert
        await Assert.ThrowsAsync<CryptographicException>(
            () => _service.DecryptAsync(encrypted, keyPair.PrivateKey));
    }

    [Fact]
    public async Task Decrypt_WithTamperedTag_ThrowsCryptographicException()
    {
        // Arrange
        string message = "Original message";
        var keyPair = await _service.GenerateKeyPairAsync();
        var encrypted = await _service.EncryptAsync(message, keyPair.PublicKey);

        // Tamper with authentication tag
        encrypted.Tag[0] ^= 0xFF;

        // Act & Assert
        await Assert.ThrowsAsync<CryptographicException>(
            () => _service.DecryptAsync(encrypted, keyPair.PrivateKey));
    }

    [Fact]
    public async Task GenerateKeyPair_ReturnsValidKeyPair()
    {
        // Act
        var keyPair = await _service.GenerateKeyPairAsync();

        // Assert
        keyPair.Should().NotBeNull();
        keyPair.PublicKey.Should().HaveCount(32);
        keyPair.PrivateKey.Should().HaveCount(32);
        keyPair.PublicKey.Should().NotBeEquivalentTo(keyPair.PrivateKey);
    }

    [Fact]
    public async Task GenerateKeyPair_MultipleCallsGenerateDifferentKeys()
    {
        // Act
        var keyPair1 = await _service.GenerateKeyPairAsync();
        var keyPair2 = await _service.GenerateKeyPairAsync();

        // Assert
        keyPair1.PublicKey.Should().NotBeEquivalentTo(keyPair2.PublicKey);
        keyPair1.PrivateKey.Should().NotBeEquivalentTo(keyPair2.PrivateKey);
    }

    [Fact]
    public async Task Encrypt_EmptyMessage_ThrowsException()
    {
        // Arrange
        var keyPair = await _service.GenerateKeyPairAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.EncryptAsync(string.Empty, keyPair.PublicKey));
    }

    [Fact]
    public async Task Encrypt_NullMessage_ThrowsException()
    {
        // Arrange
        var keyPair = await _service.GenerateKeyPairAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.EncryptAsync(null!, keyPair.PublicKey));
    }
}

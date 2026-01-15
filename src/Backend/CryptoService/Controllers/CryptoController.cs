using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessengerContracts.DTOs;
using MessengerContracts.Interfaces;
using System.Security.Claims;

namespace CryptoService.Controllers;

/// <summary>
/// API Controller for cryptographic operations (Layer 1 & Layer 2).
/// Provides endpoints for key generation, encryption, and decryption.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CryptoController : ControllerBase
{
    private readonly ITransportEncryptionService _transportEncryption;
    private readonly ILogger<CryptoController> _logger;

    public CryptoController(
        ITransportEncryptionService transportEncryption,
        ILogger<CryptoController> logger)
    {
        _transportEncryption = transportEncryption;
        _logger = logger;
    }

    /// <summary>
    /// Generate a new X25519 key pair for end-to-end encryption.
    /// POST /api/crypto/generate-keypair
    /// </summary>
    /// <returns>Public and private key pair</returns>
    [HttpPost("generate-keypair")]
    [ProducesResponseType(typeof(KeyPair), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateKeyPair()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("Generating key pair for user {UserId}", userId);

            var keyPair = await _transportEncryption.GenerateKeyPairAsync();

            return Ok(keyPair);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating key pair");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Key generation failed",
                Detail = "An error occurred while generating the key pair"
            });
        }
    }

    /// <summary>
    /// Encrypt a message using Layer 1 transport encryption (ChaCha20-Poly1305).
    /// POST /api/crypto/encrypt
    /// </summary>
    /// <param name="request">Plaintext and recipient public key</param>
    /// <returns>Encrypted message with nonce, ciphertext, tag, and ephemeral public key</returns>
    [HttpPost("encrypt")]
    [ProducesResponseType(typeof(EncryptedMessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Encrypt([FromBody] EncryptMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Plaintext))
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid request",
                    Detail = "Plaintext cannot be empty"
                });
            }

            if (request.RecipientPublicKey == null || request.RecipientPublicKey.Length != 32)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid public key",
                    Detail = "Recipient public key must be 32 bytes"
                });
            }

            var userId = GetCurrentUserId();
            
            _logger.LogDebug("Encrypting message for user {UserId}", userId);

            var encrypted = await _transportEncryption.EncryptAsync(
                request.Plaintext, 
                request.RecipientPublicKey);

            return Ok(encrypted);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid encryption request");
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Encryption failed",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during encryption");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Encryption failed",
                Detail = "An error occurred during encryption"
            });
        }
    }

    /// <summary>
    /// Decrypt a message using Layer 1 transport encryption (ChaCha20-Poly1305).
    /// POST /api/crypto/decrypt
    /// </summary>
    /// <param name="request">Encrypted message and private key</param>
    /// <returns>Decrypted plaintext</returns>
    [HttpPost("decrypt")]
    [ProducesResponseType(typeof(DecryptMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Decrypt([FromBody] DecryptMessageRequest request)
    {
        try
        {
            if (request.EncryptedMessage == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid request",
                    Detail = "Encrypted message cannot be null"
                });
            }

            if (request.PrivateKey == null || request.PrivateKey.Length != 32)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid private key",
                    Detail = "Private key must be 32 bytes"
                });
            }

            var userId = GetCurrentUserId();
            
            _logger.LogDebug("Decrypting message for user {UserId}", userId);

            var plaintext = await _transportEncryption.DecryptAsync(
                request.EncryptedMessage, 
                request.PrivateKey);

            return Ok(new DecryptMessageResponse
            {
                Plaintext = plaintext
            });
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            _logger.LogWarning(ex, "Decryption failed - invalid key or tampered ciphertext");
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Decryption failed",
                Detail = "Invalid ciphertext, key, or authentication tag"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid decryption request");
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Decryption failed",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during decryption");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Decryption failed",
                Detail = "An error occurred during decryption"
            });
        }
    }

    /// <summary>
    /// Rotate encryption keys for the current user.
    /// POST /api/crypto/rotate-keys
    /// </summary>
    /// <returns>No content on success</returns>
    [HttpPost("rotate-keys")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RotateKeys()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("Rotating keys for user {UserId}", userId);

            await _transportEncryption.RotateKeyAsync(userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating keys");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Key rotation failed",
                Detail = "An error occurred during key rotation"
            });
        }
    }

    /// <summary>
    /// Helper method to extract authenticated user ID from claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("Invalid user ID in token");
    }
}

/// <summary>
/// Request DTO for message encryption.
/// </summary>
public record EncryptMessageRequest(
    string Plaintext,
    byte[] RecipientPublicKey
);

/// <summary>
/// Request DTO for message decryption.
/// </summary>
public record DecryptMessageRequest(
    EncryptedMessageDto EncryptedMessage,
    byte[] PrivateKey
);

/// <summary>
/// Response DTO for message decryption.
/// </summary>
public record DecryptMessageResponse
{
    public string Plaintext { get; init; } = string.Empty;
}

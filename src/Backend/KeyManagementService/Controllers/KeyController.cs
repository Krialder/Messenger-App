// ========================================
// Sprint 6: Key Management Service
// Status: 🔷 API-Implementierung abgeschlossen
// Dependencies: Sprint 3 (Layer 1 Crypto)
using KeyManagementService.Data;
using KeyManagementService.Data.Entities;
using KeyManagementService.Services;
using MessengerContracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KeyManagementService.Controllers;

/// <summary>
/// Controller for managing user public keys.
/// </summary>
[ApiController]
[Route("api/keys")]
[Authorize]
public class KeyController : ControllerBase
{
    private readonly KeyDbContext _context;
    private readonly IKeyRotationService _keyRotationService;
    private readonly ILogger<KeyController> _logger;

    public KeyController(
        KeyDbContext context,
        IKeyRotationService keyRotationService,
        ILogger<KeyController> logger)
    {
        _context = context;
        _keyRotationService = keyRotationService;
        _logger = logger;
    }

    /// <summary>
    /// Get the active public key for a user.
    /// </summary>
    [HttpGet("public/{userId}")]
    [ProducesResponseType(typeof(PublicKeyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublicKey(Guid userId)
    {
        PublicKey? key = await _context.PublicKeys
            .Where(k => k.UserId == userId && k.IsActive)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync();

        if (key == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Public key not found",
                Detail = $"No active public key found for user {userId}."
            });
        }

        PublicKeyDto dto = new PublicKeyDto
        {
            Id = key.Id,
            UserId = key.UserId,
            PublicKey = Convert.ToBase64String(key.Key),
            CreatedAt = key.CreatedAt,
            ExpiresAt = key.ExpiresAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Rotate the current user's public key.
    /// </summary>
    [HttpPost("rotate")]
    [ProducesResponseType(typeof(PublicKeyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RotateKey([FromBody] RotateKeyRequest request)
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(request.NewPublicKey))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = "NewPublicKey is required."
            });
        }

        byte[] newPublicKeyBytes;

        try
        {
            newPublicKeyBytes = Convert.FromBase64String(request.NewPublicKey);
        }
        catch (FormatException)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid public key format",
                Detail = "NewPublicKey must be a valid Base64 string."
            });
        }

        if (newPublicKeyBytes.Length != 32)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid public key size",
                Detail = "Public key must be 32 bytes (X25519)."
            });
        }

        PublicKey newKey = await _keyRotationService.RotateUserKeyAsync(userId, newPublicKeyBytes);

        _logger.LogInformation("User {UserId} rotated their public key to {KeyId}.", userId, newKey.Id);

        PublicKeyDto dto = new PublicKeyDto
        {
            Id = newKey.Id,
            UserId = newKey.UserId,
            PublicKey = Convert.ToBase64String(newKey.Key),
            CreatedAt = newKey.CreatedAt,
            ExpiresAt = newKey.ExpiresAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Revoke a specific key (emergency).
    /// </summary>
    [HttpDelete("{keyId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeKey(Guid keyId)
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized();
        }

        PublicKey? key = await _context.PublicKeys
            .FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId);

        if (key == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Key not found",
                Detail = $"Key {keyId} not found or does not belong to the current user."
            });
        }

        await _keyRotationService.RevokeKeyAsync(keyId);

        _logger.LogInformation("User {UserId} revoked key {KeyId}.", userId, keyId);

        return NoContent();
    }

    /// <summary>
    /// Get key history for the current user.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<PublicKeyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKeyHistory()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized();
        }

        List<PublicKey> keys = await _context.PublicKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();

        List<PublicKeyDto> dtos = keys.Select(k => new PublicKeyDto
        {
            Id = k.Id,
            UserId = k.UserId,
            PublicKey = Convert.ToBase64String(k.Key),
            CreatedAt = k.CreatedAt,
            ExpiresAt = k.ExpiresAt
        }).ToList();

        return Ok(dtos);
    }
}


// ========================================
// REQUEST/RESPONSE DTOs
// ========================================

public record RotateKeyRequest(
    string NewPublicKey // Base64-encoded X25519 public key (32 bytes)
);

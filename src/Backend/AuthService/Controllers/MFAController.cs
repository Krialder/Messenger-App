using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using MessengerContracts.DTOs;
using MessengerContracts.Interfaces;

namespace AuthService.Controllers;

/// <summary>
/// Multi-Factor Authentication management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MFAController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IMfaService _mfaService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<MFAController> _logger;

    public MFAController(
        AuthDbContext context,
        IMfaService mfaService,
        IPasswordHasher passwordHasher,
        ILogger<MFAController> logger)
    {
        _context = context;
        _mfaService = mfaService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Enable TOTP (Authenticator App) MFA - Step 1: Generate secret and QR code
    /// </summary>
    [HttpPost("enable-totp")]
    [ProducesResponseType(typeof(EnableTotpResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> EnableTotp()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized("User not found");

            // Generate TOTP secret and QR code
            var (secret, qrCodeBase64) = await _mfaService.GenerateTotpSecretAsync(user.Username);
            var qrCodeUrl = $"data:image/png;base64,{qrCodeBase64}";

            // Generate recovery codes
            var recoveryCodes = await _mfaService.GenerateRecoveryCodesAsync(userId);

            _logger.LogInformation("TOTP setup initiated for user {UserId}", userId);

            return Ok(new EnableTotpResponse(
                secret,
                qrCodeUrl,
                recoveryCodes
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling TOTP");
            return StatusCode(500, "An error occurred while enabling TOTP");
        }
    }

    /// <summary>
    /// Enable TOTP (Authenticator App) MFA - Step 2: Verify setup with code
    /// </summary>
    [HttpPost("verify-totp-setup")]
    [ProducesResponseType(typeof(MfaMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyTotpSetup([FromBody] VerifyTotpSetupRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.MfaMethods)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Unauthorized("User not found");

            // Validate TOTP code
            var isValid = _mfaService.ValidateTotpCode(request.Secret, request.Code);
            if (!isValid)
            {
                _logger.LogWarning("Invalid TOTP code during setup for user {UserId}", userId);
                return BadRequest("Invalid TOTP code. Please try again.");
            }

            // Check if user already has a primary TOTP method
            var existingPrimary = user.MfaMethods.FirstOrDefault(m => m.IsPrimary && m.IsActive);
            bool isPrimary = existingPrimary == null;

            // Create MFA method entry
            var mfaMethod = new AuthService.Data.MfaMethod
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MethodType = "totp",
                TotpSecret = request.Secret,
                IsPrimary = isPrimary,
                IsActive = true,
                FriendlyName = request.FriendlyName ?? "Authenticator App",
                CreatedAt = DateTime.UtcNow
            };

            _context.MfaMethods.Add(mfaMethod);

            // Enable MFA for user
            user.MfaEnabled = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("TOTP MFA enabled successfully for user {UserId}", userId);

            return Ok(new MfaMethodDto
            {
                Id = mfaMethod.Id,
                MethodType = mfaMethod.MethodType,
                FriendlyName = mfaMethod.FriendlyName!,
                IsPrimary = mfaMethod.IsPrimary,
                IsEnabled = mfaMethod.IsActive,
                CreatedAt = mfaMethod.CreatedAt,
                LastUsedAt = mfaMethod.LastUsedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP setup");
            return StatusCode(500, "An error occurred while verifying TOTP setup");
        }
    }

    /// <summary>
    /// Get all active MFA methods for current user
    /// </summary>
    [HttpGet("methods")]
    [ProducesResponseType(typeof(List<MfaMethodDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMethods()
    {
        try
        {
            var userId = GetCurrentUserId();

            var methods = await _context.MfaMethods
                .Where(m => m.UserId == userId && m.IsActive)
                .Select(m => new MfaMethodDto
                {
                    Id = m.Id,
                    MethodType = m.MethodType,
                    FriendlyName = m.FriendlyName ?? m.MethodType.ToUpper(),
                    IsPrimary = m.IsPrimary,
                    IsEnabled = m.IsActive,
                    CreatedAt = m.CreatedAt,
                    LastUsedAt = m.LastUsedAt
                })
                .ToListAsync();

            return Ok(methods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching MFA methods");
            return StatusCode(500, "An error occurred while fetching MFA methods");
        }
    }

    /// <summary>
    /// Disable/delete an MFA method
    /// </summary>
    [HttpDelete("methods/{methodId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMethod(Guid methodId, [FromBody] DisableMfaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.MfaMethods.Where(m => m.IsActive))
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Unauthorized("User not found");

            // Verify password
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password during MFA method deletion for user {UserId}", userId);
                return Unauthorized("Invalid password");
            }

            var mfaMethod = user.MfaMethods.FirstOrDefault(m => m.Id == methodId);
            if (mfaMethod == null)
                return NotFound("MFA method not found");

            // Check if this is the last active method
            if (user.MfaMethods.Count == 1)
            {
                return BadRequest("Cannot delete the last MFA method. Disable MFA first if you want to remove all methods.");
            }

            // Disable the method
            mfaMethod.IsActive = false;

            // If this was the primary method, promote another to primary
            if (mfaMethod.IsPrimary)
            {
                var nextMethod = user.MfaMethods.FirstOrDefault(m => m.Id != methodId);
                if (nextMethod != null)
                {
                    nextMethod.IsPrimary = true;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("MFA method {MethodId} disabled for user {UserId}", methodId, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting MFA method");
            return StatusCode(500, "An error occurred while deleting MFA method");
        }
    }

    /// <summary>
    /// Generate new recovery codes (invalidates old ones)
    /// </summary>
    [HttpPost("generate-recovery-codes")]
    [ProducesResponseType(typeof(RecoveryCodesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateRecoveryCodes()
    {
        try
        {
            var userId = GetCurrentUserId();

            // Generate new recovery codes (old ones are automatically deleted by the service)
            var newCodes = await _mfaService.GenerateRecoveryCodesAsync(userId);

            _logger.LogInformation("Recovery codes regenerated for user {UserId}", userId);

            return Ok(new RecoveryCodesResponse(newCodes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recovery codes");
            return StatusCode(500, "An error occurred while generating recovery codes");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return Guid.Parse(userIdClaim!);
    }
}

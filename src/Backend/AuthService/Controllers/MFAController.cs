using AuthService.Data;
using AuthService.Data.Entities;
using MessengerContracts.DTOs;
using MessengerContracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthService.Controllers
{
    /// <summary>
    /// Multi-Factor Authentication controller for TOTP setup and management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MFAController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IMfaService _mfaService;
        private readonly ILogger<MFAController> _logger;

        public MFAController(
            AuthDbContext context,
            IMfaService mfaService,
            ILogger<MFAController> logger)
        {
            _context = context;
            _mfaService = mfaService;
            _logger = logger;
        }

        /// <summary>
        /// Helper method to extract authenticated user ID from claims
        /// </summary>
        /// <returns>User ID if authenticated, null otherwise</returns>
        private Guid? GetAuthenticatedUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out Guid userId) ? userId : null;
        }

        /// <summary>
        /// Enable TOTP-based MFA and get QR code
        /// </summary>
        /// <returns>TOTP secret, QR code, and recovery codes</returns>
        [HttpPost("enable-totp")]
        [ProducesResponseType(typeof(EnableTotpResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EnableTotp()
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                User? user = await _context.Users
                    .Include(u => u.MfaMethods)
                    .FirstOrDefaultAsync(u => u.Id == userId.Value);

                if (user == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "User not found"
                    });
                }

                // Check if TOTP already enabled
                if (user.MfaMethods.Any(m => m.MethodType == "totp" && m.IsActive))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "TOTP already enabled",
                        Detail = "TOTP is already enabled for this account"
                    });
                }

                // Remove any pending (inactive) TOTP methods to prevent race conditions
                var pendingTotpMethods = user.MfaMethods
                    .Where(m => m.MethodType == "totp" && !m.IsActive)
                    .ToList();
                
                if (pendingTotpMethods.Any())
                {
                    _context.MfaMethods.RemoveRange(pendingTotpMethods);
                }

                // Generate TOTP secret and QR code
                (string secret, string qrCodeBase64) = await _mfaService.GenerateTotpSecretAsync(
                    user.Username, 
                    "SecureMessenger");

                // Generate recovery codes
                List<string> recoveryCodes = await _mfaService.GenerateRecoveryCodesAsync(userId.Value);

                // Create MFA method (but don't enable yet - user must verify first)
                MfaMethod totpMethod = new MfaMethod
                {
                    Id = Guid.NewGuid(),
                    UserId = userId.Value,
                    MethodType = "totp",
                    TotpSecret = secret,
                    IsActive = false, // Will be enabled after verification
                    IsPrimary = true,
                    FriendlyName = "Authenticator App",
                    CreatedAt = DateTime.UtcNow
                };

                _context.MfaMethods.Add(totpMethod);
                await _context.SaveChangesAsync();

                _logger.LogInformation("TOTP setup initiated for user {UserId}", userId.Value);

                return Ok(new EnableTotpResponse(
                    Secret: secret,
                    QrCodeUrl: qrCodeBase64,
                    BackupCodes: recoveryCodes
                ));
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrent modification detected during TOTP setup");
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Concurrent modification",
                    Detail = "Another operation is in progress. Please try again."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling TOTP");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "TOTP setup failed",
                    Detail = "An error occurred while setting up TOTP"
                });
            }
        }

        /// <summary>
        /// Verify TOTP setup with first code
        /// </summary>
        /// <param name="request">Verification request with TOTP code</param>
        /// <returns>Success response</returns>
        [HttpPost("verify-totp-setup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyTotpSetup([FromBody] VerifyTotpSetupRequest request)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                User? user = await _context.Users
                    .Include(u => u.MfaMethods)
                    .FirstOrDefaultAsync(u => u.Id == userId.Value);

                if (user == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "User not found"
                    });
                }

                // Find pending TOTP method
                MfaMethod? totpMethod = user.MfaMethods
                    .FirstOrDefault(m => m.MethodType == "totp" && !m.IsActive);

                if (totpMethod == null || string.IsNullOrEmpty(totpMethod.TotpSecret))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "No pending TOTP setup",
                        Detail = "No pending TOTP setup found. Please initiate setup first."
                    });
                }

                // Verify TOTP code
                bool isValid = _mfaService.ValidateTotpCode(totpMethod.TotpSecret, request.Code);

                if (!isValid)
                {
                    _logger.LogWarning("TOTP verification failed for user {UserId}", userId.Value);
                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Invalid TOTP code",
                        Detail = "The provided TOTP code is invalid"
                    });
                }

                // Enable TOTP
                totpMethod.IsActive = true;
                totpMethod.LastUsedAt = DateTime.UtcNow;

                // Enable MFA for user
                user.MfaEnabled = true;

                await _context.SaveChangesAsync();

                _logger.LogInformation("TOTP enabled successfully for user {UserId}", userId.Value);

                return Ok(new { message = "TOTP enabled successfully" });
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrent modification detected during TOTP verification");
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Concurrent modification",
                    Detail = "Another operation is in progress. Please try again."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying TOTP setup");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "TOTP verification failed",
                    Detail = "An error occurred while verifying TOTP setup"
                });
            }
        }

        /// <summary>
        /// Get all MFA methods for current user
        /// </summary>
        /// <returns>List of MFA methods</returns>
        [HttpGet("methods")]
        [ProducesResponseType(typeof(List<MfaMethodDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMfaMethods()
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                List<MfaMethod> methods = await _context.MfaMethods
                    .Where(m => m.UserId == userId.Value)
                    .OrderByDescending(m => m.IsPrimary)
                    .ThenByDescending(m => m.CreatedAt)
                    .ToListAsync();

                List<MfaMethodDto> methodDtos = methods.Select(m => new MfaMethodDto
                {
                    Id = m.Id,
                    MethodType = m.MethodType,
                    IsEnabled = m.IsActive,
                    IsPrimary = m.IsPrimary,
                    FriendlyName = m.FriendlyName ?? string.Empty,
                    CreatedAt = m.CreatedAt,
                    LastUsedAt = m.LastUsedAt
                }).ToList();

                return Ok(methodDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MFA methods");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Failed to get MFA methods"
                });
            }
        }

        /// <summary>
        /// Disable MFA method
        /// </summary>
        /// <param name="methodId">MFA method ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("methods/{methodId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DisableMfaMethod(Guid methodId)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                MfaMethod? method = await _context.MfaMethods
                    .FirstOrDefaultAsync(m => m.Id == methodId && m.UserId == userId.Value);

                if (method == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "MFA method not found"
                    });
                }

                _context.MfaMethods.Remove(method);

                // Check if this was the last ACTIVE MFA method
                bool hasOtherActiveMethods = await _context.MfaMethods
                    .AnyAsync(m => m.UserId == userId.Value && m.Id != methodId && m.IsActive);

                if (!hasOtherActiveMethods)
                {
                    // Disable MFA for user
                    User? user = await _context.Users.FindAsync(userId.Value);
                    if (user != null)
                    {
                        user.MfaEnabled = false;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("MFA method {MethodId} disabled for user {UserId}", methodId, userId.Value);

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrent modification detected while disabling MFA method");
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Concurrent modification",
                    Detail = "Another operation is in progress. Please try again."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling MFA method {MethodId}", methodId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Failed to disable MFA method"
                });
            }
        }

        /// <summary>
        /// Generate new recovery codes (revokes old ones)
        /// </summary>
        /// <returns>New recovery codes</returns>
        [HttpPost("generate-recovery-codes")]
        [ProducesResponseType(typeof(RecoveryCodesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                User? user = await _context.Users.FindAsync(userId.Value);

                if (user == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "User not found"
                    });
                }

                // Generate new recovery codes (MfaService handles removal of old codes)
                List<string> newRecoveryCodes = await _mfaService.GenerateRecoveryCodesAsync(userId.Value);

                _logger.LogInformation("New recovery codes generated for user {UserId}", userId.Value);

                return Ok(new RecoveryCodesResponse(
                    RecoveryCodes: newRecoveryCodes
                ));
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrent modification detected during recovery code generation");
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Concurrent modification",
                    Detail = "Another operation is in progress. Please try again."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recovery codes");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Failed to generate recovery codes"
                });
            }
        }
    }
}

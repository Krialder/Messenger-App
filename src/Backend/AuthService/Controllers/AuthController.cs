using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.Data.Entities;
using MessengerContracts.DTOs;
using MessengerContracts.Interfaces;

namespace AuthService.Controllers;

/// <summary>
/// Authentication controller handling registration, login, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMfaService _mfaService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMfaService mfaService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mfaService = mfaService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // 1. Validate input
            if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3 || request.Username.Length > 50)
                return BadRequest("Username must be between 3 and 50 characters");

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                return BadRequest("Valid email is required");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 12)
                return BadRequest("Password must be at least 12 characters");

            // 2. Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser != null)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "User already exists",
                    Detail = existingUser.Username == request.Username
                        ? "Username is already taken"
                        : "Email is already registered",
                    Status = StatusCodes.Status409Conflict
                });
            }

            // 3. Hash password using Argon2id
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // 4. Generate master key salt (32 bytes for Layer 2 encryption)
            var masterKeySalt = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(masterKeySalt);

            // 5. Create user
            var user = new AuthService.Data.User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                MasterKeySalt = masterKeySalt,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                AccountStatus = "active",
                EmailVerified = false,
                MfaEnabled = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered: {UserId}, Username: {Username}", user.Id, user.Username);

            // 6. Return user ID and master key salt
            return CreatedAtAction(
                nameof(Register),
                new RegisterResponse(
                    user.Id,
                    user.Username,
                    user.Email,
                    Convert.ToBase64String(user.MasterKeySalt),
                    "Registration successful. Please verify your email."
                ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, "An error occurred during registration");
        }
    }

    /// <summary>
    /// Login with username/email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MfaRequiredResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.MfaMethods.Where(m => m.IsActive))
                .FirstOrDefaultAsync(u => u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found - {UsernameOrEmail}", request.UsernameOrEmail);
                return Unauthorized("Invalid credentials");
            }

            if (!user.IsActive || user.AccountStatus != "active")
            {
                return StatusCode(423, "Account is locked or suspended");
            }

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
                return Unauthorized("Invalid credentials");
            }

            if (user.MfaEnabled && user.MfaMethods.Any())
            {
                var sessionToken = _tokenService.GenerateAccessToken(user.Id, user.Username, new List<string> { "mfa_pending" });

                var availableMethods = user.MfaMethods.Select(m => new MfaMethodDto
                {
                    Id = m.Id,
                    MethodType = m.MethodType,
                    FriendlyName = m.FriendlyName ?? m.MethodType.ToUpper(),
                    IsPrimary = m.IsPrimary,
                    IsEnabled = m.IsActive,
                    CreatedAt = m.CreatedAt,
                    LastUsedAt = m.LastUsedAt
                }).ToList();

                return Ok(new MfaRequiredResponse(
                    true,
                    sessionToken,
                    availableMethods
                ));
            }

            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username, new List<string> { "user" });
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new AuthService.Data.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged in: {UserId}", user.Id);

            return Ok(new LoginResponse(
                accessToken,
                refreshToken,
                900,
                "Bearer",
                new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    MfaEnabled = user.MfaEnabled
                }
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "An error occurred during login");
        }
    }

    /// <summary>
    /// Verify MFA code and complete login
    /// </summary>
    [HttpPost("verify-mfa")]
    [Authorize(Roles = "mfa_pending")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid session token");
            }

            var user = await _context.Users
                .Include(u => u.MfaMethods)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Unauthorized("User not found");

            var mfaMethod = user.MfaMethods.FirstOrDefault(m => m.Id == request.MethodId && m.IsActive);
            if (mfaMethod == null)
                return BadRequest("Invalid MFA method");

            bool isValid = false;

            if (mfaMethod.MethodType == "totp" && !string.IsNullOrEmpty(mfaMethod.TotpSecret))
            {
                isValid = _mfaService.ValidateTotpCode(mfaMethod.TotpSecret, request.Code);
            }
            else
            {
                return BadRequest("Unsupported MFA method");
            }

            if (!isValid)
            {
                _logger.LogWarning("MFA verification failed for user {UserId}", userId);
                return Unauthorized("Invalid MFA code");
            }

            mfaMethod.LastUsedAt = DateTime.UtcNow;

            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username, new List<string> { "user" });
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new AuthService.Data.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("MFA verification successful for user {UserId}", userId);

            return Ok(new TokenResponse(
                accessToken,
                refreshToken,
                900,
                "Bearer"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MFA verification");
            return StatusCode(500, "An error occurred during MFA verification");
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var refreshTokenEntity = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked);

            if (refreshTokenEntity == null)
            {
                _logger.LogWarning("Refresh token not found or revoked");
                return Unauthorized("Invalid refresh token");
            }

            if (refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired for user {UserId}", refreshTokenEntity.UserId);
                return Unauthorized("Refresh token expired");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(
                refreshTokenEntity.User.Id,
                refreshTokenEntity.User.Username,
                new List<string> { "user" });

            _logger.LogInformation("Access token refreshed for user {UserId}", refreshTokenEntity.UserId);

            return Ok(new TokenResponse(
                newAccessToken,
                request.RefreshToken,
                900,
                "Bearer"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, "An error occurred during token refresh");
        }
    }

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid token");
            }

            var refreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in refreshTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged out: {UserId}", userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, "An error occurred during logout");
        }
    }
}

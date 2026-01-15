using AuthService.Data;
using AuthService.Data.Entities;
using MessengerContracts.DTOs;
using MessengerContracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AuthService.Controllers;

/// <summary>
/// Authentication controller for user registration, login, and token management
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
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Registration response with user ID and master key salt</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Username already exists",
                    Detail = $"Username '{request.Username}' is already taken"
                });
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Email already exists",
                    Detail = $"Email '{request.Email}' is already registered"
                });
            }

            // Generate master key salt (32 bytes for Argon2id)
            byte[] masterKeySalt = new byte[32];
            RandomNumberGenerator.Fill(masterKeySalt);

            // Hash password
            string passwordHash = _passwordHasher.HashPassword(request.Password);

            // Create new user
            User newUser = new User
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

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} registered successfully with ID {UserId}",
                request.Username, newUser.Id);

            // Return response
            RegisterResponse response = new RegisterResponse(
                UserId: newUser.Id,
                Username: newUser.Username,
                Email: newUser.Email,
                MasterKeySalt: Convert.ToBase64String(masterKeySalt)
            );

            return CreatedAtAction(nameof(Register), new { id = newUser.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Registration failed",
                Detail = "An error occurred during registration"
            });
        }
    }

    /// <summary>
    /// Login with username/email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Login response with JWT token or MFA requirement</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Find user by email
            User? user = await _context.Users
                .Include(u => u.MfaMethods)
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: User not found for email {Email}", request.Email);
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid credentials",
                    Detail = "Email or password is incorrect"
                });
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login attempt failed: Invalid password for user {UserId}", user.Id);
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid credentials",
                    Detail = "Email or password is incorrect"
                });
            }

            // Check if MFA is enabled
            if (user.MfaEnabled && user.MfaMethods.Any(m => m.IsActive))
            {
                _logger.LogInformation("User {UserId} requires MFA verification", user.Id);

                return Ok(new LoginResponse(
                    User: new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        DisplayName = user.Username,
                        MfaEnabled = true,
                        EmailVerified = user.EmailVerified,
                        CreatedAt = user.CreatedAt
                    },
                    AccessToken: string.Empty,
                    RefreshToken: string.Empty,
                    ExpiresIn: 0,
                    MfaRequired: true
                ));
            }

            // Generate JWT tokens
            List<string> roles = new List<string> { "User" };
            string accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username, roles);
            string refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            RefreshToken refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return Ok(new LoginResponse(
                User: new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    DisplayName = user.Username,
                    MfaEnabled = user.MfaEnabled,
                    EmailVerified = user.EmailVerified,
                    CreatedAt = user.CreatedAt
                },
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresIn: 900, // 15 minutes
                MfaRequired: false
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Login failed",
                Detail = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// Verify MFA code and complete login
    /// </summary>
    /// <param name="request">MFA verification request</param>
    /// <returns>Login response with JWT tokens</returns>
    [HttpPost("verify-mfa")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaRequest request)
    {
        try
        {
            // Find user
            User? user = await _context.Users
                .Include(u => u.MfaMethods)
                .Include(u => u.RecoveryCodes)
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null || !user.MfaEnabled)
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "MFA verification failed",
                    Detail = "Invalid user or MFA not enabled"
                });
            }

            bool isValid = false;

            // Try TOTP verification first
            MfaMethod? totpMethod = user.MfaMethods.FirstOrDefault(m => m.MethodType == "totp" && m.IsActive);
            if (totpMethod != null && !string.IsNullOrEmpty(totpMethod.TotpSecret))
            {
                isValid = _mfaService.ValidateTotpCode(totpMethod.TotpSecret, request.MfaCode);

                if (isValid)
                {
                    totpMethod.LastUsedAt = DateTime.UtcNow;
                }
            }

            // If TOTP failed, try recovery code
            if (!isValid)
            {
                RecoveryCode? validCode = user.RecoveryCodes.FirstOrDefault(rc => !rc.Used);
                if (validCode != null)
                {
                    isValid = await _mfaService.ValidateRecoveryCodeAsync(user.Id, request.MfaCode);
                }
            }

            if (!isValid)
            {
                _logger.LogWarning("MFA verification failed for user {UserId}", user.Id);
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "MFA verification failed",
                    Detail = "Invalid MFA code"
                });
            }

            // Generate JWT tokens
            List<string> roles = new List<string> { "User" };
            string accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username, roles);
            string refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            RefreshToken refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} verified MFA successfully", user.Id);

            return Ok(new LoginResponse(
                User: new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    DisplayName = user.Username,
                    MfaEnabled = user.MfaEnabled,
                    EmailVerified = user.EmailVerified,
                    CreatedAt = user.CreatedAt
                },
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresIn: 900,
                MfaRequired: false
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MFA verification for email {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "MFA verification failed",
                Detail = "An error occurred during MFA verification"
            });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access and refresh tokens</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Find refresh token
            RefreshToken? refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked);

            if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token");
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid refresh token",
                    Detail = "Refresh token is invalid or expired"
                });
            }

            // Check if user is still active
            if (!refreshToken.User.IsActive)
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "User account inactive",
                    Detail = "User account is no longer active"
                });
            }

            // Generate new tokens
            List<string> roles = new List<string> { "User" };
            string newAccessToken = _tokenService.GenerateAccessToken(
                refreshToken.User.Id,
                refreshToken.User.Username,
                roles);
            string newRefreshToken = _tokenService.GenerateRefreshToken();

            // Revoke old refresh token
            refreshToken.IsRevoked = true;

            // Create new refresh token
            RefreshToken newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = refreshToken.UserId,
                Token = newRefreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token renewed for user {UserId}", refreshToken.UserId);

            return Ok(new TokenResponse(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken,
                ExpiresIn: 900
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Token refresh failed",
                Detail = "An error occurred during token refresh"
            });
        }
    }

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    /// <param name="request">Logout request with refresh token</param>
    /// <returns>No content on success</returns>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Find and revoke refresh token
            RefreshToken? refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} logged out successfully", refreshToken.UserId);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Logout failed",
                Detail = "An error occurred during logout"
            });
        }
    }
}

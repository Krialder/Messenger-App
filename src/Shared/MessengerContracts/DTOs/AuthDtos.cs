using System;
using System.Collections.Generic;

namespace MessengerContracts.DTOs;

// ========================================
// Authentication DTOs
// ========================================

/// <summary>
/// Request DTO for user registration
/// </summary>
public record RegisterRequest(
    string Username,
    string Email,
    string Password
);

/// <summary>
/// Response DTO for successful registration
/// </summary>
public record RegisterResponse(
    Guid UserId,
    string Username,
    string Email,
    string MasterKeySalt,  // Base64-encoded salt for Layer 2 encryption
    string Message
);

/// <summary>
/// Request DTO for login
/// </summary>
public record LoginRequest(
    string UsernameOrEmail,
    string Password
);

/// <summary>
/// Response DTO for successful login (no MFA)
/// </summary>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,  // Seconds
    string TokenType,
    UserDto User
);

/// <summary>
/// Response DTO when MFA is required
/// </summary>
public record MfaRequiredResponse(
    bool MfaRequired,
    string SessionToken,  // Temporary token for MFA verification
    List<MfaMethodDto> AvailableMethods
);

/// <summary>
/// Request DTO for MFA verification
/// </summary>
public record VerifyMfaRequest(
    Guid MethodId,
    string Code
);

/// <summary>
/// Request DTO for token refresh
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken
);

/// <summary>
/// Response DTO for token operations
/// </summary>
public record TokenResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresIn,
    string TokenType
);

// ========================================
// MFA DTOs (Extended)
// ========================================

/// <summary>
/// Response DTO for TOTP enable step 1
/// </summary>
public record EnableTotpResponse(
    string Secret,  // Base32-encoded TOTP secret
    string QrCodeUrl,  // Data URL for QR code image
    List<string> BackupCodes  // 10 recovery codes (plaintext, show once)
);

/// <summary>
/// Request DTO for TOTP setup verification
/// </summary>
public record VerifyTotpSetupRequest(
    string Secret,
    string Code,
    string? FriendlyName,
    List<string>? RecoveryCodes  // From EnableTotpResponse
);

/// <summary>
/// Request DTO for disabling MFA method
/// </summary>
public record DisableMfaRequest(
    string Password  // Password confirmation required
);

/// <summary>
/// Response DTO for recovery codes generation
/// </summary>
public record RecoveryCodesResponse(
    List<string> Codes  // 10 plaintext codes (display once)
);

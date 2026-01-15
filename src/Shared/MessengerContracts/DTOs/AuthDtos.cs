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
    string MasterKeySalt  // Base64-encoded salt for Layer 2 encryption
);

/// <summary>
/// Request DTO for login
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);

/// <summary>
/// Response DTO for login (with or without MFA)
/// </summary>
public record LoginResponse(
    UserDto User,
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,  // Seconds
    bool MfaRequired = false
);

/// <summary>
/// Request DTO for MFA verification
/// </summary>
public record VerifyMfaRequest(
    string Email,
    string MfaCode
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
    string RefreshToken,
    int ExpiresIn
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
    string Code
);

/// <summary>
/// Response DTO for recovery codes generation
/// </summary>
public record RecoveryCodesResponse(
    List<string> RecoveryCodes  // 10 plaintext codes (display once)
);

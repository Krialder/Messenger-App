namespace MessengerContracts.Interfaces;

/// <summary>
/// Interface for password hashing using Argon2id
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a password using Argon2id
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verify a password against its hash
    /// </summary>
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// Interface for MFA Service
/// </summary>
public interface IMfaService
{
    /// <summary>
    /// Generate TOTP secret and QR code
    /// </summary>
    Task<(string Secret, string QrCodeBase64)> GenerateTotpSecretAsync(string username, string issuer = "SecureMessenger");

    /// <summary>
    /// Validate TOTP code
    /// </summary>
    bool ValidateTotpCode(string secret, string code);

    /// <summary>
    /// Generate recovery codes
    /// </summary>
    Task<List<string>> GenerateRecoveryCodesAsync(Guid userId);

    /// <summary>
    /// Validate recovery code
    /// </summary>
    Task<bool> ValidateRecoveryCodeAsync(Guid userId, string code);
}

/// <summary>
/// Interface for JWT token generation
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate JWT access token
    /// </summary>
    string GenerateAccessToken(Guid userId, string username, List<string> roles);

    /// <summary>
    /// Generate refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate and decode JWT token
    /// </summary>
    (bool IsValid, Guid? UserId) ValidateToken(string token);
}

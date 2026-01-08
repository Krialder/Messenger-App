namespace MessengerContracts.DTOs;

// Authentication DTOs
public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string? PublicKey = null
);

public record LoginRequest(
    string Username,
    string Password
);

public record LoginResponse(
    string? AccessToken,
    string? RefreshToken,
    bool MfaRequired,
    string? SessionToken,
    List<MfaMethodDto>? AvailableMfaMethods
);

public record MfaVerificationRequest(
    string SessionToken,
    string Code,
    string? MethodId = null
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);

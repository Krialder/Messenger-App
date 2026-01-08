using System;

namespace MessengerContracts.DTOs
{
    /// <summary>
    /// DTO for MFA setup
    /// </summary>
    public class MfaSetupDto
    {
        public string QrCodeUri { get; set; } = string.Empty;
        public string ManualEntryKey { get; set; } = string.Empty;
        public string[] RecoveryCodes { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// DTO for MFA verification
    /// </summary>
    public class MfaVerificationDto
    {
        public string Code { get; set; } = string.Empty;
        public MfaMethodType MethodType { get; set; }
    }

    /// <summary>
    /// DTO for MFA method
    /// </summary>
    public class MfaMethodDto
    {
        public Guid Id { get; set; }
        public MfaMethodType MethodType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum MfaMethodType
    {
        TOTP,
        YubiKey,
        FIDO2,
        RecoveryCode
    }
}

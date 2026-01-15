using AuthService.Data;
using AuthService.Data.Entities;
using MessengerContracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using QRCoder;
using System.Security.Cryptography;

namespace AuthService.Services
{
    public class MFAService : IMfaService
    {
        private const int TotpWindow = 1; // Allow ±30 seconds clock drift
        private const int RecoveryCodeCount = 10;
        private const int RecoveryCodeLength = 16;
        
        private readonly AuthDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public MFAService(AuthDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Generate TOTP secret and QR code
        /// </summary>
        public async Task<(string Secret, string QrCodeBase64)> GenerateTotpSecretAsync(
            string username, 
            string issuer = "SecureMessenger")
        {
            // Generate random TOTP secret (Base32 encoded, 160 bits)
            var secretBytes = RandomNumberGenerator.GetBytes(20);
            var secret = Base32Encoding.ToString(secretBytes);

            // Generate OTP auth URI for QR code
            var otpUri = $"otpauth://totp/{issuer}:{username}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";

            // Generate QR code
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(otpUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);
            var qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);

            return (secret, qrCodeBase64);
        }

        /// <summary>
        /// Validate TOTP code (6 digits)
        /// </summary>
        public bool ValidateTotpCode(string secret, string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
                    return false;

                var secretBytes = Base32Encoding.ToBytes(secret);
                var totp = new Totp(secretBytes, step: 30, mode: OtpHashMode.Sha1, totpSize: 6);

                // Verify with time window (allows ±30 seconds clock drift)
                var currentTime = DateTime.UtcNow;
                for (int i = -TotpWindow; i <= TotpWindow; i++)
                {
                    var timeStep = currentTime.AddSeconds(i * 30);
                    var expectedCode = totp.ComputeTotp(timeStep);
                    
                    if (ConstantTimeEquals(code, expectedCode))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generate recovery codes for user
        /// </summary>
        public async Task<List<string>> GenerateRecoveryCodesAsync(Guid userId)
        {
            var codes = new List<string>();

            // Delete existing unused recovery codes
            var existingCodes = await _context.RecoveryCodes
                .Where(r => r.UserId == userId && !r.Used)
                .ToListAsync();
            _context.RecoveryCodes.RemoveRange(existingCodes);

            // Generate new recovery codes
            for (int i = 0; i < RecoveryCodeCount; i++)
            {
                var code = GenerateRecoveryCode();
                codes.Add(code);

                // Hash and store
                var recoveryCode = new RecoveryCode
                {
                    UserId = userId,
                    CodeHash = _passwordHasher.HashPassword(code),
                    Used = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RecoveryCodes.Add(recoveryCode);
            }

            await _context.SaveChangesAsync();
            return codes;
        }

        /// <summary>
        /// Validate recovery code (one-time use)
        /// </summary>
        public async Task<bool> ValidateRecoveryCodeAsync(Guid userId, string code)
        {
            var storedCodes = await _context.RecoveryCodes
                .Where(r => r.UserId == userId && !r.Used)
                .ToListAsync();

            foreach (var storedCode in storedCodes)
            {
                if (_passwordHasher.VerifyPassword(code, storedCode.CodeHash))
                {
                    // Mark as used
                    storedCode.Used = true;
                    storedCode.UsedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Generate a random recovery code (format: XXXX-XXXX-XXXX-XXXX)
        /// </summary>
        private string GenerateRecoveryCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude ambiguous chars
            var random = RandomNumberGenerator.GetBytes(RecoveryCodeLength);
            var code = new char[RecoveryCodeLength];

            for (int i = 0; i < RecoveryCodeLength; i++)
            {
                code[i] = chars[random[i] % chars.Length];
            }

            // Format as XXXX-XXXX-XXXX-XXXX
            return $"{new string(code, 0, 4)}-{new string(code, 4, 4)}-{new string(code, 8, 4)}-{new string(code, 12, 4)}";
        }

        /// <summary>
        /// Constant-time string comparison to prevent timing attacks
        /// </summary>
        private bool ConstantTimeEquals(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}

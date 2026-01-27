using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuthService
{
    public static class EnvironmentValidator
    {
        public static void ValidateProductionEnvironment(IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (!environment.IsProduction())
                return;

            var errors = new List<string>();

            // JWT Secret
            var jwtSecret = configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
            {
                errors.Add("JWT_SECRET must be at least 32 characters in production");
            }

            // TOTP Encryption Key
            var totpKey = Environment.GetEnvironmentVariable("TOTP_ENCRYPTION_KEY") 
                ?? configuration["Security:TotpEncryptionKey"];
            if (string.IsNullOrEmpty(totpKey) || totpKey.Length < 32)
            {
                errors.Add("TOTP_ENCRYPTION_KEY must be at least 32 characters in production");
            }

            // Database Password
            var connectionString = configuration.GetConnectionString("PostgreSQL");
            if (connectionString?.Contains("Password=") == false)
            {
                errors.Add("PostgreSQL connection string must include password in production");
            }

            // Redis Password
            var redisPassword = configuration["Redis:Password"];
            if (string.IsNullOrEmpty(redisPassword))
            {
                errors.Add("Redis password must be configured in production");
            }

            // Rate Limiting
            var rateLimitingEnabled = configuration.GetValue<bool>("IpRateLimiting:EnableEndpointRateLimiting");
            if (!rateLimitingEnabled)
            {
                errors.Add("Rate limiting must be enabled in production");
            }

            if (errors.Any())
            {
                throw new InvalidOperationException(
                    $"Production environment validation failed:\n{string.Join("\n", errors)}");
            }
        }
    }
}

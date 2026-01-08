# AuthService - Authentication & MFA Service

**Status**: ✅ Foundation Phase 1 Complete (Services Layer)  
**Next**: Phase 2 - Controllers Implementation

---

## 📊 Implementation Status

| Component | Status | Type |
|-----------|--------|------|
| **Services** | ✅ **PRODUCTION** | Real Code |
| **Controllers** | ⏳ Pseudo-Code | Pending |
| **EF Core** | ✅ **READY** | Migration created |
| **Configuration** | ✅ **COMPLETE** | appsettings.json |

---

## 🏗️ Architecture

```
AuthService/
├── Controllers/
│   ├── AuthController.cs          # ⏳ Pseudo-Code (Login, Register)
│   └── MFAController.cs           # ⏳ Pseudo-Code (TOTP, Recovery)
├── Services/
│   ├── Argon2PasswordHasher.cs    # ✅ PRODUCTION (Password Hashing)
│   ├── TokenService.cs            # ✅ PRODUCTION (JWT)
│   └── MFAService.cs              # ✅ PRODUCTION (TOTP, QR, Recovery)
├── Data/
│   ├── AuthDbContext.cs           # ✅ PRODUCTION (EF Core)
│   ├── Entities/
│   │   └── User.cs                # ✅ User, MfaMethod, RecoveryCode, RefreshToken
│   └── Migrations/
│       └── InitialCreate.cs       # ✅ CREATED
├── Program.cs                     # ✅ PRODUCTION (DI, JWT, CORS)
├── appsettings.json               # ✅ CONFIGURED
└── appsettings.Development.json   # ✅ CREATED
```

---

## 🔐 Security Features

### **Password Hashing** ✅
- **Algorithm**: Argon2id (OWASP recommended)
- **Parameters**: 3 iterations, 64 MB memory, 1 parallelism
- **Salt**: 16 bytes (128 bits) - random per user
- **Hash**: 32 bytes (256 bits)
- **Format**: `salt:hash` (Base64)
- **Timing Attack Protection**: Constant-time comparison

### **JWT Tokens** ✅
- **Access Token**: 15 minutes (configurable)
- **Refresh Token**: 7 days (cryptographically secure)
- **Algorithm**: HS256 (HMAC-SHA256)
- **Claims**: UserId, Username, Roles, JTI
- **Validation**: ClockSkew = 0 (no expiration tolerance)

### **Multi-Factor Authentication** ✅
- **TOTP**: RFC 6238 compliant
  - SHA1, 6 digits, 30-second window
  - Time drift tolerance: ±30 seconds
- **QR Code**: PNG Base64 (ECCLevel.Q)
- **Recovery Codes**: 10x 16 characters
  - Format: `XXXX-XXXX-XXXX-XXXX`
  - Argon2id hashed
  - One-time use

---

## 🗄️ Database Schema

### **Tables** (PostgreSQL)
- `users` - User accounts with MFA support
- `mfa_methods` - TOTP, YubiKey, FIDO2 methods
- `recovery_codes` - One-time recovery codes
- `refresh_tokens` - JWT refresh tokens

### **Indexes**
- `username` (unique)
- `email` (unique)
- `user_id + is_active`
- `token` (unique, filtered)

### **Constraints**
- Username length ≥ 3 characters
- Account status: `active`, `suspended`, `deleted`
- MFA method type: `totp`, `yubikey`, `fido2`

---

## 🚀 Quick Start

### **Prerequisites**
- .NET 8 SDK
- PostgreSQL 16 (via Docker)
- Visual Studio 2022+ or VS Code

### **Setup**

```bash
# 1. Start PostgreSQL
docker-compose up -d postgres

# 2. Navigate to AuthService
cd src/Backend/AuthService

# 3. Restore packages
dotnet restore

# 4. Run migrations
dotnet ef database update

# 5. Start service
dotnet run

# Service runs on:
# - HTTPS: https://localhost:7001
# - HTTP:  http://localhost:5001
# - Swagger: https://localhost:7001/swagger
```

### **Verify Setup**

```bash
# Health Check
curl http://localhost:5001/health
# Expected: Healthy

# Database Connection
docker exec -it messenger_postgres psql -U messenger_admin -d messenger_auth -c "\dt"
# Expected: users, mfa_methods, recovery_codes, refresh_tokens
```

---

## 📦 NuGet Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Otp.NET" Version="1.4.0" />
<PackageReference Include="QRCoder" Version="1.6.0" />
<PackageReference Include="Konscious.Security.Cryptography.Argon2" Version="1.3.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.2" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

**Total**: 11 packages

---

## 🧪 Testing (Planned)

### **Unit Tests** (Phase 2)
```csharp
// Argon2PasswordHasher
- HashPassword_ValidPassword_ReturnsHash
- VerifyPassword_CorrectPassword_ReturnsTrue
- VerifyPassword_WrongPassword_ReturnsFalse

// TokenService
- GenerateAccessToken_ValidClaims_ReturnsToken
- ValidateToken_ValidToken_ReturnsUserId
- ValidateToken_ExpiredToken_ReturnsFalse

// MFAService
- GenerateTotpSecret_ReturnsSecretAndQR
- ValidateTotpCode_ValidCode_ReturnsTrue
- GenerateRecoveryCodes_Returns10Codes
```

### **Integration Tests** (Phase 2)
```csharp
// AuthController
- Register_ValidUser_ReturnsCreated
- Login_ValidCredentials_ReturnsToken
- Login_WithMFA_RequiresMFACode

// MFAController
- EnableTOTP_ReturnsQRCode
- VerifyTOTP_ValidCode_EnablesMFA
```

---

## 🔧 Configuration

### **appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=messenger_auth;..."
  },
  "JWT": {
    "Secret": "YourSuperSecretJWTKeyHere_MinLength32Characters",
    "Issuer": "MessengerAuthService",
    "Audience": "MessengerClient",
    "AccessTokenExpirationMinutes": "15",
    "RefreshTokenExpirationDays": "7"
  }
}
```

### **Environment Variables** (Production)
```bash
ConnectionStrings__DefaultConnection="..."
JWT__Secret="..."  # 32+ characters, cryptographically random
```

---

## 📝 API Endpoints (Planned)

### **Authentication**
```
POST   /api/auth/register       # Register new user
POST   /api/auth/login          # Login with username/password
POST   /api/auth/verify-mfa     # Verify MFA code
POST   /api/auth/refresh        # Refresh access token
POST   /api/auth/logout         # Revoke refresh token
```

### **Multi-Factor Authentication**
```
POST   /api/mfa/enable-totp     # Enable TOTP (get QR code)
POST   /api/mfa/verify-totp     # Verify TOTP setup
GET    /api/mfa/methods         # List MFA methods
DELETE /api/mfa/methods/{id}    # Remove MFA method
POST   /api/mfa/generate-recovery-codes  # Generate new recovery codes
```

### **Health & Monitoring**
```
GET    /health                  # Health check
GET    /swagger                 # Swagger UI (Development only)
```

---

## 🎯 Next Steps (Phase 2)

### **Priority 1: AuthController**
- [ ] Implement `Register` endpoint
- [ ] Implement `Login` endpoint
- [ ] Implement `VerifyMFA` endpoint
- [ ] Implement `RefreshToken` endpoint

### **Priority 2: MFAController**
- [ ] Implement `EnableTOTP` endpoint
- [ ] Implement `VerifyTOTP` endpoint
- [ ] Implement `GetMFAMethods` endpoint
- [ ] Implement `GenerateRecoveryCodes` endpoint

### **Priority 3: Validation**
- [ ] Add FluentValidation for DTOs
- [ ] Input sanitization
- [ ] Rate limiting (via API Gateway)

### **Priority 4: Testing**
- [ ] Unit tests for all services
- [ ] Integration tests for controllers
- [ ] E2E test: Register → Login → Enable MFA

---

## 📚 Documentation

- **Main Docs**: `../../docs/`
- **Implementation Plan**: `../../docs/07_IMPLEMENTATION_PLAN.md`
- **MFA Documentation**: `../../docs/06_MULTI_FACTOR_AUTHENTICATION.md`
- **Foundation Status**: `../../FOUNDATION_STATUS.md`

---

## 🤝 Dependencies

### **Internal**
- `MessengerContracts` - DTOs, Interfaces
- `MessengerCommon` - Constants, Helpers

### **External**
- PostgreSQL 16
- Redis 7 (for future features)
- RabbitMQ 3 (for events)

---

## 📄 License

[To be determined]

---

**Version**: 1.0 - Foundation Phase 1  
**Last Updated**: 2025-01-06  
**Status**: ✅ Services Layer Complete, Controllers Pending

# Secure Messenger - Complete Workspace Structure

## 📂 **WORKSPACE OVERVIEW**

**Location**: `I:\Just_for_fun\Messenger\`  
**Repository**: https://github.com/Krialder/Messenger-App  
**Branch**: master  
**Framework**: .NET 8.0  
**Architecture**: Microservices

---

## 🏗️ **PROJECT STRUCTURE**

### **Solution File**
```
Messenger.sln                           # Main solution file
```

### **Backend Services** (9 Microservices)
```
src/Backend/
├── AuditLogService/                    # Audit logging service
│   ├── AuditLogService.csproj
│   ├── Controllers/
│   ├── Data/
│   ├── Dockerfile
│   ├── Program.cs
│   └── README.md
│
├── AuthService/                        # ✅ PRODUCTION-READY
│   ├── AuthService.csproj
│   ├── Controllers/
│   │   ├── AuthController.cs           # ⏳ Pseudo-Code
│   │   └── MFAController.cs            # ⏳ Pseudo-Code
│   ├── Services/
│   │   ├── Argon2PasswordHasher.cs     # ✅ PRODUCTION
│   │   ├── TokenService.cs             # ✅ PRODUCTION
│   │   └── MFAService.cs               # ✅ PRODUCTION
│   ├── Data/
│   │   ├── AuthDbContext.cs            # ✅ PRODUCTION
│   │   ├── Entities/
│   │   │   └── User.cs                 # ✅ PRODUCTION
│   │   └── Migrations/
│   │       ├── 20250106200751_InitialCreate.cs
│   │       ├── 20250106200751_InitialCreate.Designer.cs
│   │       └── AuthDbContextModelSnapshot.cs
│   ├── Dockerfile
│   ├── Program.cs                      # ✅ PRODUCTION
│   ├── appsettings.json                # ✅ CONFIGURED
│   ├── appsettings.Development.json    # ✅ CONFIGURED
│   └── README.md
│
├── CryptoService/                      # ⏳ Pseudo-Code
│   ├── CryptoService.csproj
│   ├── Layer1/
│   │   └── TransportEncryptionService.cs
│   ├── Layer2/
│   │   └── LocalStorageEncryptionService.cs
│   ├── Layer3/
│   │   └── MessageEncryptionService.cs
│   ├── Dockerfile
│   ├── Program.cs
│   └── README.md
│
├── FileTransferService/                # ⏳ Pseudo-Code
│   ├── FileTransferService.csproj
│   ├── Controllers/
│   ├── Services/
│   ├── Dockerfile
│   ├── Program.cs
│   └── README.md
│
├── GatewayService/                     # ⏳ Pseudo-Code (Ocelot)
│   ├── GatewayService.csproj
│   ├── Dockerfile
│   ├── ocelot.json
│   ├── Program.cs
│   └── README.md
│
├── KeyManagementService/               # ⏳ Pseudo-Code
│   ├── KeyManagementService.csproj
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   ├── Dockerfile
│   ├── Program.cs
│   └── README.md
│
├── MessageService/                     # ⏳ Pseudo-Code
│   ├── MessageService.csproj
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   ├── Hubs/
│   │   └── MessageHub.cs
│   ├── Dockerfile
│   ├── Program.cs
│   └── README.md
│
├── NotificationService/                # ⏳ Pseudo-Code
│   ├── NotificationService.csproj
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   ├── Dockerfile
│   ├── Program.cs
│   └── README.md
│
└── UserService/                        # ⏳ Pseudo-Code
    ├── UserService.csproj
    ├── Controllers/
    ├── Services/
    ├── Data/
    ├── Dockerfile
    ├── Program.cs
    └── README.md
```

### **Frontend Client**
```
src/Frontend/
└── MessengerClient/                    # Desktop/Mobile client
    ├── MessengerClient.csproj
    ├── App.xaml
    ├── App.xaml.cs
    ├── MainWindow.xaml
    ├── MainWindow.xaml.cs
    ├── Services/
    ├── ViewModels/
    ├── Views/
    └── README.md
```

### **Shared Libraries** (2 Projects)
```
src/Shared/
├── MessengerCommon/                    # ⏳ Pending
│   ├── MessengerCommon.csproj
│   ├── Constants/
│   │   └── Constants.cs
│   ├── Extensions/
│   │   └── Extensions.cs
│   └── Helpers/
│       └── Helpers.cs
│
└── MessengerContracts/                 # ✅ COMPLETE
    ├── MessengerContracts.csproj
    ├── DTOs/
    │   ├── AuthDtos.cs                 # ✅ NEW (LoginRequest, TokenResponse, etc.)
    │   ├── MfaDto.cs                   # ✅ UPDATED
    │   └── UserDto.cs                  # ✅ EXISTING
    └── Interfaces/
        └── IServices.cs                # ✅ NEW (IPasswordHasher, IMfaService, ITokenService)
```

### **Test Projects** (3 Test Suites)
```
tests/
├── MessengerTests/                     # Unit tests
│   ├── MessengerTests.csproj
│   ├── ServiceTests/
│   │   ├── AuthServiceTests.cs
│   │   ├── MessageServiceTests.cs
│   │   └── MFAServiceTests.cs
│   ├── CryptoTests/
│   │   └── EncryptionTests.cs
│   └── IntegrationTests/
│       └── DatabaseTests.cs
│
├── MessengerTests.E2E/                 # End-to-end tests
│   ├── MessengerTests.E2E.csproj
│   ├── LoginFlowTests.cs
│   └── MessageFlowTests.cs
│
└── MessengerTests.Performance/         # Performance tests
    ├── MessengerTests.Performance.csproj
    └── CryptoPerformanceTests.cs
```

### **Infrastructure Files**
```
Root Directory/
├── docker-compose.yml                  # Multi-service Docker compose
├── init-db.sql                         # Database initialization script
├── .gitignore
├── .dockerignore
├── README.md                           # Main project README
└── docs/
    ├── DOCUMENTATION_CHANGELOG.md      # Documentation changes log
    ├── API/
    ├── ARCHITECTURE/
    └── SECURITY/
```

---

## 🏗️ **FOUNDATION PHASE 1 COMPLETED** (2025-01-06)

### ✅ **Phase 1: AuthService Foundation - IMPLEMENTIERT**

**Status**: AuthService hat echte Implementierung (keine Pseudo-Code mehr)

#### **Implementierte Features:**
- ✅ **EF Core DbContext** - Vollständig konfiguriert mit Entities
- ✅ **Argon2id Password Hashing** - Produktionsreif (Konscious.Security.Cryptography.Argon2)
- ✅ **JWT Token Service** - Access + Refresh Tokens
- ✅ **MFA Service** - TOTP mit OTP.NET + QR Code Generation
- ✅ **Recovery Codes** - Argon2id gehashed
- ✅ **Database Migration** - `InitialCreate` erstellt
- ✅ **Project Dependencies** - MessengerContracts + MessengerCommon referenziert

#### **NuGet Packages (AuthService):**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.Npgsql" Version="8.0.2" />
<PackageReference Include="Otp.NET" Version="1.4.0" />
<PackageReference Include="QRCoder" Version="1.6.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="Konscious.Security.Cryptography.Argon2" Version="1.3.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.2" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

---

## 📊 **IMPLEMENTATION STATUS**

| Component | Status | Implementation | Migration | Test Coverage |
|-----------|--------|----------------|-----------|---------------|
| **AuthService** | ✅ **PRODUCTION-READY** | Real Code | ✅ InitialCreate | ⏳ Pending |
| **MessageService** | ⏳ Pseudo-Code | Pending | ❌ | ❌ |
| **NotificationService** | ⏳ Pseudo-Code | Pending | ❌ | ❌ |
| **CryptoService** | ⏳ Pseudo-Code | Pending | N/A | ❌ |
| **KeyManagementService** | ⏳ Pseudo-Code | Pending | ❌ | ❌ |
| **UserService** | ⏳ Pseudo-Code | Pending | ❌ | ❌ |
| **FileTransferService** | ⏳ Pseudo-Code | Pending | ❌ | ❌ |
| **AuditLogService** | ⏳ Pseudo-Code | Pending | ❌ | ❌ |
| **GatewayService** | ⏳ Pseudo-Code | Pending | N/A | ❌ |
| **MessengerContracts** | ✅ **COMPLETE** | Real DTOs | N/A | N/A |
| **MessengerCommon** | ⏳ Pending | Pending | N/A | N/A |
| **MessengerClient** | ⏳ Pending | Pending | N/A | ❌ |

**Overall Progress**: 2/12 Components implementiert (16.7%)

---

## 📁 **AuthService - Detailed Structure**

### **Implementierte Dateien:**

```
src/Backend/AuthService/
├── Controllers/
│   ├── AuthController.cs               # ⏳ Pseudo-Code (Login, Register)
│   └── MFAController.cs                # ⏳ Pseudo-Code (TOTP Enable/Verify)
├── Services/
│   ├── Argon2PasswordHasher.cs         # ✅ PRODUCTION (Argon2id)
│   ├── TokenService.cs                 # ✅ PRODUCTION (JWT)
│   └── MFAService.cs                   # ✅ PRODUCTION (TOTP, Recovery Codes)
├── Data/
│   ├── AuthDbContext.cs                # ✅ PRODUCTION (EF Core)
│   ├── Entities/
│   │   └── User.cs                     # ✅ PRODUCTION (User, MfaMethod, etc.)
│   └── Migrations/
│       ├── 20250106200751_InitialCreate.cs
│       ├── 20250106200751_InitialCreate.Designer.cs
│       └── AuthDbContextModelSnapshot.cs
├── Program.cs                          # ✅ PRODUCTION (DI, JWT, CORS)
├── appsettings.json                    # ✅ CONFIGURED
├── appsettings.Development.json        # ✅ CREATED
├── Dockerfile                          # ✅ PRESENT
├── README.md                           # ✅ DOCUMENTATION
└── AuthService.csproj                  # ✅ ALL PACKAGES
```

### **Services Details:**

#### **1. Argon2PasswordHasher**
```csharp
// Konscious.Security.Cryptography.Argon2id
- Parameters: 3 iterations, 64 MB memory, 1 parallelism
- Salt: 16 bytes (128 bits)
- Hash: 32 bytes (256 bits)
- Format: "salt:hash" (Base64)
- Constant-time comparison (CryptographicOperations.FixedTimeEquals)
```

#### **2. TokenService**
```csharp
// System.IdentityModel.Tokens.Jwt
- Access Token: 15 minutes (configurable)
- Refresh Token: 7 days (configurable)
- Algorithm: HS256
- Claims: UserId, Username, Roles
```

#### **3. MFAService**
```csharp
// OTP.NET + QRCoder
- TOTP: RFC 6238 (SHA1, 6 digits, 30s window)
- QR Code: PNG Base64 (ECCLevel.Q)
- Recovery Codes: 10x 16 chars (format: XXXX-XXXX-XXXX-XXXX)
- Argon2id hashing for recovery codes
```

### **Database Entities:**

```csharp
// ✅ All mapped to PostgreSQL schema
- User (users table)
- MfaMethod (mfa_methods table)
- RecoveryCode (recovery_codes table)
- RefreshToken (refresh_tokens table)

// Navigation Properties: Configured
// Indexes: Username, Email, UserId+IsActive
// Constraints: Username length, Account status enum
```

---

## 📋 **Shared Libraries Status**

### **MessengerContracts** ✅
```
src/Shared/MessengerContracts/
├── DTOs/
│   ├── AuthDtos.cs                     # ✅ NEW (LoginRequest, TokenResponse, etc.)
│   ├── MfaDto.cs                       # ✅ EXISTING (Updated enums)
│   └── UserDto.cs                      # ✅ EXISTING
├── Interfaces/
│   └── IServices.cs                    # ✅ NEW (IPasswordHasher, IMfaService, ITokenService)
└── MessengerContracts.csproj           # ✅ COMPLETE
```

### **MessengerCommon** ⏳
```
src/Shared/MessengerCommon/
├── Constants/
│   └── Constants.cs                    # ⏳ TO-DO
├── Extensions/
│   └── Extensions.cs                   # ⏳ TO-DO
├── Helpers/
│   └── Helpers.cs                      # ⏳ TO-DO
└── MessengerCommon.csproj              # ✅ PRESENT
```

---

## 🧪 **Test Projects Structure**

### **Unit Tests (MessengerTests)**
```
tests/MessengerTests/
├── ServiceTests/
│   ├── AuthServiceTests.cs             # ⏳ TO-DO
│   ├── MessageServiceTests.cs          # ⏳ TO-DO
│   └── MFAServiceTests.cs              # ⏳ TO-DO
├── CryptoTests/
│   └── EncryptionTests.cs              # ⏳ TO-DO
└── IntegrationTests/
    └── DatabaseTests.cs                # ⏳ TO-DO
```

### **End-to-End Tests (MessengerTests.E2E)**
```
tests/MessengerTests.E2E/
├── LoginFlowTests.cs                   # ⏳ TO-DO
└── MessageFlowTests.cs                 # ⏳ TO-DO
```

### **Performance Tests (MessengerTests.Performance)**
```
tests/MessengerTests.Performance/
└── CryptoPerformanceTests.cs           # ⏳ TO-DO
```

---

## 🐳 **Docker & Infrastructure**

### **docker-compose.yml**
```yaml
Services:
- postgres (AuthService DB)
- redis (Caching)
- rabbitmq (Message Queue)
- authservice
- messageservice
- notificationservice
- cryptoservice
- keymanagementservice
- userservice
- filetransferservice
- auditlogservice
- gatewayservice
```

### **Database Initialization**
```
init-db.sql                             # PostgreSQL initialization script
- Creates databases for all services
- Sets up users and permissions
```

---

## 🎯 **Next Steps - Foundation Phase 2**

### **Priorität 1: AuthController vervollständigen**
```csharp
// TO-DO: src/Backend/AuthService/Controllers/AuthController.cs
- [ ] Register-Endpoint (mit Argon2PasswordHasher)
- [ ] Login-Endpoint (mit TokenService)
- [ ] VerifyMFA-Endpoint (mit MFAService)
- [ ] RefreshToken-Endpoint
```

### **Priorität 2: MFAController vervollständigen**
```csharp
// TO-DO: src/Backend/AuthService/Controllers/MFAController.cs
- [ ] EnableTOTP (mit MFAService.GenerateTotpSecretAsync)
- [ ] VerifyTOTP (mit MFAService.ValidateTotpCode)
- [ ] GenerateRecoveryCodes (mit MFAService.GenerateRecoveryCodesAsync)
- [ ] GetMFAMethods (EF Core Query)
```

### **Priorität 3: Datenbank aufsetzen**
```bash
# 1. PostgreSQL starten (Docker)
docker-compose up -d postgres

# 2. Migration ausführen
cd src/Backend/AuthService
dotnet ef database update

# 3. Verifizieren
psql -h localhost -U messenger_admin -d messenger_auth -c "\dt"
```

### **Priorität 4: CryptoService - Layer 2**
```csharp
// TO-DO: src/Backend/CryptoService/Layer2/LocalStorageEncryptionService.cs
- [ ] Master Key Derivation (Argon2id mit User Salt)
- [ ] AES-256-GCM Encryption/Decryption
- [ ] Secure Memory Cleanup
```

### **Priorität 5: Integration Tests**
```csharp
// TO-DO: tests/MessengerTests/ServiceTests/AuthServiceTests.cs
- [ ] Register User Test
- [ ] Login Test (erfolg + fehlgeschlagen)
- [ ] MFA Enable Test
- [ ] Password Hashing Round-trip Test
```

---

## 🚀 **Running AuthService (Standalone)**

### **Lokale Entwicklung:**
```bash
# 1. Starte PostgreSQL
docker-compose up -d postgres

# 2. Führe Migration aus
cd src/Backend/AuthService
dotnet ef database update

# 3. Starte AuthService
dotnet run

# Service läuft auf: https://localhost:7001 (HTTPS) oder http://localhost:5001
# Swagger UI: https://localhost:7001/swagger
```

### **Test-Endpoints:**
```bash
# Health Check
curl http://localhost:5001/health

# Register (wenn implementiert)
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","email":"alice@example.com","password":"SecurePass123!"}'
```

---

## 📦 **All Projects Summary**

### **Total Projects**: 15

| Category | Count | Projects |
|----------|-------|----------|
| Backend Services | 9 | Auth, Message, Notification, Crypto, KeyManagement, User, FileTransfer, AuditLog, Gateway |
| Frontend | 1 | MessengerClient |
| Shared Libraries | 2 | MessengerContracts, MessengerCommon |
| Tests | 3 | MessengerTests, MessengerTests.E2E, MessengerTests.Performance |

### **Technology Stack:**

| Component | Technology |
|-----------|-----------|
| Framework | .NET 8.0 |
| Backend Services | ASP.NET Core Web API |
| Frontend | WPF/MAUI (MessengerClient) |
| Database | PostgreSQL (Npgsql) |
| Caching | Redis |
| Message Queue | RabbitMQ |
| API Gateway | Ocelot |
| Real-time | SignalR |
| ORM | Entity Framework Core 8.0 |
| Authentication | JWT Bearer |
| Password Hashing | Argon2id |
| MFA | TOTP (OTP.NET) |
| Testing | xUnit |
| Containerization | Docker |
| Orchestration | Docker Compose |

---

## 🔄 **Changes from Previous Version**

### ✅ **Foundation Phase 1 (2025-01-06):**
1. **AuthService** - Echte Implementierung statt Pseudo-Code:
   - Argon2PasswordHasher (Konscious.Security.Cryptography.Argon2)
   - TokenService (System.IdentityModel.Tokens.Jwt)
   - MFAService (OTP.NET + QRCoder)
   - EF Core DbContext mit vollständigen Entities
   - Database Migration erstellt

2. **MessengerContracts** - DTOs erweitert:
   - AuthDtos.cs (LoginRequest, TokenResponse, etc.)
   - IServices.cs (IPasswordHasher, IMfaService, ITokenService)

3. **NuGet Packages** - Alle produktionsreife Versionen:
   - Otp.NET 1.4.0 (statt OtpNet)
   - QRCoder 1.6.0
   - System.IdentityModel.Tokens.Jwt 8.0.2 (Security-Update)
   - Swashbuckle.AspNetCore 6.5.0

---

## 🗂️ **Directory Statistics**

```
Total Files: ~150+ (excluding bin/obj)
Total Projects: 15 (.csproj files)
Backend Services: 9
Shared Libraries: 2
Test Projects: 3
Frontend Projects: 1
```

---

## ✅ **Summary**

**Foundation Phase 1**: ✅ **COMPLETE**
- ✅ AuthService Services implementiert (Argon2, JWT, MFA)
- ✅ EF Core DbContext + Entities
- ✅ Database Migration erstellt
- ✅ Shared Contracts erweitert
- ✅ Build erfolgreich
- ⏳ Controller noch Pseudo-Code (Phase 2)

**Next Phase**: Controllers implementieren + Datenbank aufsetzen

**Workspace Structure**: ✅ **FULLY DOCUMENTED**
- All 15 projects cataloged
- File structure mapped
- Dependencies documented
- Technology stack identified

---

**Version**: 4.0 - Complete Workspace Overview  
**Last Updated**: 2025-01-06  
**Status**: 🏗️ **FOUNDATION IN PROGRESS** - AuthService Services produktionsreif

**Progress**: 16.7% (2/12 Components)

**Repository**: https://github.com/Krialder/Messenger-App  
**Location**: I:\Just_for_fun\Messenger\

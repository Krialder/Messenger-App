# Workspace Quick Reference - Secure Messenger

**Version**: 9.0  
**Framework**: .NET 9.0  
**Repository**: https://github.com/Krialder/Messenger-App

---

## 📊 Project Status

| Component | Status | Tests | Coverage |
|-----------|--------|-------|----------|
| **Backend (9 services)** | ✅ 100% | 151/151 | ~97% |
| **Frontend (WPF)** | ✅ 100% | - | - |
| **Integration Tests** | ✅ 100% | 12/12 | 100% |
| **Documentation** | ✅ 100% | - | - |

**Production Ready**: ✅ Yes

---

## 🚀 Quick Start

```bash
# 1. Clone & Configure
git clone https://github.com/Krialder/Messenger-App
cd Messenger-App
cp .env.example .env

# 2. Start Backend (Docker)
docker-compose up -d

# 3. Build Frontend
.\build-client.bat  # Windows
# OR
chmod +x build-client.sh && ./build-client.sh  # Linux/macOS

# 4. Run Application
.\publish\MessengerClient\MessengerClient.exe
```

---

## 📁 Project Structure

```
Messenger/
├── src/
│   ├── Backend/              # 9 microservices
│   │   ├── AuthService       # JWT + MFA
│   │   ├── MessageService    # Messages + SignalR
│   │   ├── CryptoService     # 3-Layer Encryption
│   │   ├── NotificationService
│   │   ├── KeyManagementService
│   │   ├── UserService
│   │   ├── FileTransferService
│   │   ├── AuditLogService
│   │   └── GatewayService    # API Gateway
│   ├── Frontend/             # WPF Client
│   │   └── MessengerClient   # ReactiveUI + MaterialDesign
│   └── Shared/               # DTOs + Common
│       ├── MessengerContracts
│       └── MessengerCommon
├── tests/
│   ├── MessengerTests        # 151 tests (~97% coverage)
│   ├── MessengerTests.E2E    # E2E tests (optional)
│   └── MessengerTests.Performance
├── docs/                     # Documentation
├── docker-compose.yml        # Infrastructure
└── .env.example             # Environment template
```

**Full Structure**: See [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)

---

## 🔧 Common Commands

### Development

```bash
# Run Backend Service
cd src/Backend/AuthService
dotnet run

# Run Frontend
cd src/Frontend/MessengerClient
dotnet run

# Run Tests
cd tests/MessengerTests
dotnet test
```

### Docker

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Restart service
docker-compose restart auth_service
```

### Build

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Build standalone client
.\build-client.bat
```

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [README.md](README.md) | Project overview |
| [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) | Deployment instructions |
| [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) | Complete file structure |
| [SECURITY.md](SECURITY.md) | Security policy |
| [CHANGELOG.md](CHANGELOG.md) | Version history |
| [CODE_AUDIT_REPORT.md](CODE_AUDIT_REPORT.md) | Code audit findings |
| [docs/](docs/) | Technical documentation |

---

## 🎯 Key Technologies

### Backend
- ASP.NET Core 9.0
- PostgreSQL 16
- RabbitMQ 3.12
- Redis 7
- Docker

### Frontend
- WPF (.NET 9.0)
- ReactiveUI (MVVM)
- MaterialDesignThemes
- SQLite (local storage)
- SignalR (real-time)

### Security
- X25519 + ChaCha20-Poly1305 (Layer 1)
- AES-256-GCM (Layer 2)
- Argon2id (password hashing)
- TOTP (MFA)

---

## 🔗 Useful Links

- **Architecture**: [docs/02_ARCHITECTURE.md](docs/02_ARCHITECTURE.md)
- **Cryptography**: [docs/03_CRYPTOGRAPHY.md](docs/03_CRYPTOGRAPHY.md)
- **API Reference**: [docs/09_API_REFERENCE.md](docs/09_API_REFERENCE.md)
- **Testing Strategy**: [docs/08_TESTING.md](docs/08_TESTING.md)

---

**Version**: 9.0  
**Last Updated**: 2025-01-10  
**Status**: ✅ Production Ready

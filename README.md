# Secure Messenger

> End-to-end encrypted messaging platform built with .NET 9.0 and WPF

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/Tests-151%20passing-success)](tests/MessengerTests/)
[![Build](https://img.shields.io/badge/Build-Passing-success)](.github/workflows/)

---

## Overview

Secure Messenger is an open-source encrypted messaging application featuring:

- **3-layer end-to-end encryption** (X25519, ChaCha20-Poly1305, AES-256-GCM)
- **Microservices architecture** (9 independent backend services)
- **Modern WPF desktop client** (MaterialDesign UI)
- **Real-time messaging** via SignalR
- **Multi-factor authentication** (TOTP)

**Status**: Production ready (v9.0)

---

## Tech Stack

### Backend
- ASP.NET Core 9.0 (C#)
- PostgreSQL 16 (Database)
- RabbitMQ 3.12 (Message Broker)
- Redis 7 (Caching)
- Docker (Containerization)

### Frontend
- WPF (.NET 9.0)
- ReactiveUI (MVVM Framework)
- MaterialDesignThemes (UI Library)
- SQLite (Local Storage)
- SignalR (Real-time)

### Security
- X25519 (Key Exchange)
- ChaCha20-Poly1305 (Transport Encryption)
- AES-256-GCM (Storage Encryption)
- Argon2id (Password Hashing)
- TOTP (Multi-Factor Auth)

---

## Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

### Installation

```bash
# 1. Clone repository
git clone https://github.com/Krialder/Messenger-App.git
cd Messenger-App

# 2. Configure environment
cp .env.example .env

# 3. Edit .env and replace placeholder secrets
# IMPORTANT: Change all passwords and JWT_SECRET before running!
# For production, use strong random values (min. 32 characters)

# 4. Start backend services
docker-compose up -d

# 5. Build frontend client
.\build-client.bat    # Windows
# or
chmod +x build-client.sh && ./build-client.sh     # Linux/macOS

# 6. Run application
.\publish\MessengerClient\MessengerClient.exe
```

### First Run

1. Register a new account
2. Enable MFA in Settings (optional but recommended)
3. Add contacts and start messaging

### ⚠️ Security Notice

**Before deploying to production**:

1. Replace all passwords in `.env` with strong, random values
2. Generate secure JWT secret: `openssl rand -base64 64`
3. Never commit `.env` file to version control
4. See [SECURITY.md](SECURITY.md) for best practices

---

## Features

### Messaging
- ✅ One-to-one encrypted chat
- ✅ Group chat with encryption
- ✅ Real-time message delivery
- ✅ Read receipts
- ✅ Typing indicators
- ✅ Message history

### Security
- ✅ End-to-end encryption (3 layers)
- ✅ Perfect forward secrecy (via key rotation)
- ✅ Multi-factor authentication
- ✅ Encrypted file transfer
- ✅ Audit logging
- ✅ Automatic key rotation (every 30 days)

### User Features
- ✅ User profiles
- ✅ Contact management
- ✅ Dark mode (MaterialDesign)
- ✅ Offline message queue
- ✅ Local database sync

---

## Architecture

### Backend Services

| Service | Port | Purpose |
|---------|------|---------|
| **GatewayService** | 5000 | API Gateway (Ocelot) |
| **AuthService** | 5001 | Authentication + JWT + MFA |
| **MessageService** | 5002 | Messages + Conversations |
| **CryptoService** | 5003 | Encryption operations |
| **NotificationService** | 5004 | Real-time notifications |
| **KeyManagementService** | 5005 | Key rotation + storage |
| **UserService** | 5006 | User profiles + contacts |
| **FileTransferService** | 5007 | Encrypted file uploads |
| **AuditLogService** | 5008 | Audit logging |

### Encryption Layers

1. **Layer 1: Transport** - X25519 + ChaCha20-Poly1305 (ECDH + AEAD)
2. **Layer 2: Storage** - AES-256-GCM (local database encryption)
3. **Layer 3: Group** - Signal Protocol (group messaging)

See [docs/03_CRYPTOGRAPHY.md](docs/03_CRYPTOGRAPHY.md) for details.

---

## Development

### Project Structure

```
Messenger/
├── src/
│   ├── Backend/          # 9 microservices
│   ├── Frontend/         # WPF desktop client
│   └── Shared/           # DTOs & common libraries
├── tests/                # 151 tests (~97% coverage)
├── docs/                 # Documentation
└── docker-compose.yml    # Infrastructure
```

See [docs/guides/PROJECT_STRUCTURE.md](docs/guides/PROJECT_STRUCTURE.md) for complete structure.

### Running Tests

```bash
cd tests/MessengerTests
dotnet test
```

**Test Results**:
- 151 tests (100% passing)
- ~97% code coverage
- ~11 second execution time

### Building from Source

```bash
# Backend
cd src/Backend/<ServiceName>
dotnet run

# Frontend
cd src/Frontend/MessengerClient
dotnet run
```

---

## Deployment

### Docker (Recommended)

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Standalone Build

```bash
# Windows
.\build-client.bat

# Linux/macOS
chmod +x build-client.sh
./build-client.sh
```

See [docs/guides/DEPLOYMENT_GUIDE.md](docs/guides/DEPLOYMENT_GUIDE.md) for production deployment.

---

## Documentation

- **[Getting Started](docs/README.md)** - Documentation index
- **[Architecture](docs/02_ARCHITECTURE.md)** - System architecture
- **[Cryptography](docs/03_CRYPTOGRAPHY.md)** - Encryption details
- **[API Reference](docs/09_API_REFERENCE.md)** - API documentation
- **[Deployment Guide](docs/guides/DEPLOYMENT_GUIDE.md)** - Deployment instructions
- **[Project Structure](docs/guides/PROJECT_STRUCTURE.md)** - Complete file structure
- **[Workspace Guide](docs/guides/WORKSPACE_GUIDE.md)** - Quick reference
- **[Testing](docs/08_TESTING.md)** - Testing strategy
- **[Code Audit](docs/reports/CODE_AUDIT_REPORT.md)** - Security audit report
- **[Changelog](CHANGELOG.md)** - Version history

---

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### How to Contribute

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code of Conduct

This project adheres to a [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

---

## Security

Security is a top priority. See [SECURITY.md](SECURITY.md) for:

- Reporting vulnerabilities
- Security best practices
- Supported versions

**⚠️ Do not report security issues via GitHub Issues**  
Email: security@example.com *(update with your contact)*

---

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) for details.

---

## Support

- **Issues**: [GitHub Issues](https://github.com/Krialder/Messenger-App/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Krialder/Messenger-App/discussions)
- **Documentation**: [docs/README.md](docs/README.md)

---

## Acknowledgments

- Built with [ASP.NET Core](https://dotnet.microsoft.com/)
- UI powered by [MaterialDesignThemes](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- Cryptography via [libsodium](https://libsodium.gitbook.io/)

---

**Version**: 9.0.0  
**Status**: Production Ready ✅  
**Last Updated**: 2025-01-10

**Repository**: https://github.com/Krialder/Messenger-App

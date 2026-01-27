# Secure Messenger

**Status**: 🟢 Development (94% Complete)  
**Version**: 9.2.2  
**Last Updated**: 2026-01-27

---

## 🚀 Quick Start

### Prerequisites
- Docker Desktop
- .NET 9.0 SDK
- PowerShell 5.1+ (Windows)

### ⚡ Automatisches Setup (Empfohlen)

#### **Option 1: Quick Start (2-3 Minuten)**
```powershell
# Schneller Start ohne Migrationen
.\quick-start.ps1
```

**Perfekt für:** Entwicklung, Testing, wenn bereits installiert

---

#### **Option 2: Vollständiges Deployment (5-7 Minuten)**
```powershell
# Komplettes Setup mit Datenbank-Migrationen
.\deploy-complete.ps1
```

**Perfekt für:** Erste Installation, Production, nach Reset

**Beinhaltet:**
- ✅ Docker Desktop Check & Auto-Start
- ✅ Environment Validierung
- ✅ Docker Images Build
- ✅ Services Start (PostgreSQL, Redis, RabbitMQ, 9 Backend-Services)
- ✅ Datenbank-Migrationen (6 Services)
- ✅ Health Checks (9 Services)
- ✅ API Test (User-Registrierung)
- ✅ Detaillierte Zusammenfassung

**Weitere Optionen:**
```powershell
# Rebuild aller Images (nach Code-Änderungen)
.\deploy-complete.ps1 -RebuildImages

# Ohne Migrationen (wenn DB bereits existiert)
.\deploy-complete.ps1 -SkipMigrations

# Verbose-Ausgabe für Debugging
.\deploy-complete.ps1 -Verbose
```

---

### 🛑 Services Stoppen & Aufräumen

```powershell
# Stoppe Services (behält Daten)
.\cleanup.ps1

# Kompletter Reset (⚠️ löscht ALLE Daten!)
.\cleanup.ps1 -DeleteData
```

---

### 📖 Detaillierte Anleitungen

- **[SETUP_CHECKLIST.md](SETUP_CHECKLIST.md)** - Komplette Setup-Checkliste
- **[SCRIPTS_GUIDE.md](SCRIPTS_GUIDE.md)** - PowerShell Scripts Dokumentation
- **[docs/DATABASE_MIGRATIONS.md](docs/DATABASE_MIGRATIONS.md)** - Datenbank-Migrationen

---

### 🔧 Manuelles Setup

Falls du jeden Schritt selbst ausführen möchtest:

```bash
# 1. Docker Desktop starten
# Öffne Docker Desktop manuell oder:
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"

# 2. Warte 45 Sekunden auf Docker-Start
timeout /t 45

# 3. Clone Repository (falls noch nicht geschehen)
git clone https://github.com/Krialder/Messenger-App.git
cd Messenger

# 4. Docker Container starten
docker-compose up -d

# 5. Warte 60 Sekunden auf Initialisierung
timeout /t 60

# 6. Health Check
curl http://localhost:5001/health
# Erwartet: Healthy

# 7. Swagger UI öffnen
start http://localhost:5001/swagger
```

**⚠️ Wichtig:** Manuelle Installation benötigt zusätzlich [Datenbank-Migrationen](docs/DATABASE_MIGRATIONS.md)

---

## 📚 Dokumentation

- [Rate Limiting](docs/RATE_LIMITING.md) - Vollständige API-Limits & Swagger-Anleitung
- [Architecture](docs/01_ARCHITECTURE.md) - System-Design & Layer-Architektur
- [API Reference](docs/09_API_REFERENCE.md) - Alle Endpoints dokumentiert
- [Security](docs/03_SECURITY_CONCEPT.md) - Verschlüsselung & MFA
- [Database Migrations](docs/DATABASE_MIGRATIONS.md) - Datenbank-Setup
- [Scripts Guide](SCRIPTS_GUIDE.md) - PowerShell Scripts Übersicht

---

## 🔒 Security Features

- ✅ **Argon2id** Password Hashing (OWASP-konform)
- ✅ **JWT** Authentication (15 min Access Token, 7 Tage Refresh)
- ✅ **TOTP MFA** (RFC 6238, Google Authenticator kompatibel)
- ✅ **Rate Limiting** (Brute-Force Protection via Redis)
- ✅ **AES-256** Encryption (TOTP Secrets)
- ✅ **DSGVO-konform** (Audit Logs, Data Export)

---

## 🧪 Testing

```bash
# Unit Tests
dotnet test --filter "Category=Unit"

# Integration Tests
dotnet test --filter "Category=Integration"

# Alle Tests
dotnet test

# Code Coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📦 Production Deployment

### Schnelles Production-Setup

```powershell
# 1. Secrets generieren
openssl rand -base64 64

# 2. .env bearbeiten
notepad .env
# JWT_SECRET = <generierter-wert>
# TOTP_ENCRYPTION_KEY = <generierter-wert>

# 3. Production Deployment
.\deploy-complete.ps1 -RebuildImages
```

### Vollständiges Production-Setup

```bash
# 1. Generiere sichere Secrets
openssl rand -base64 64

# 2. Bearbeite .env.production
nano .env.production

# 3. Production Images bauen
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build

# 4. Datenbank migrieren
cd src/Backend/AuthService
dotnet ef database update --configuration Release

# 5. Deployment
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## 🎯 Project Status

| Sprint | Feature | Status |
|--------|---------|--------|
| Sprint 1 | System-Architektur | ✅ Complete |
| Sprint 2 | Datenbank-Design | ✅ Complete |
| Sprint 3 | Security-Konzept | ✅ Complete |
| Sprint 4-5 | Authentication Service | ✅ Complete |
| Sprint 6-7 | Message Service | ✅ Complete |
| Sprint 8 | End-to-End Encryption | ✅ Complete |
| Sprint 9-10 | MFA (TOTP) | ✅ Complete |
| Sprint 11 | Audit Log Service | ✅ Complete |
| Sprint 12 | File Transfer Service | ✅ Complete |
| Sprint 13 | WPF Client | ✅ Complete |
| Sprint 14 | API Gateway | ✅ Complete |
| Sprint 15-16 | Testing & Deployment | ✅ Complete |
| Sprint 17 | User Service Controllers | 🟡 In Progress |

**Completion**: 94% (7/9 services production-ready)

---

## 🛠️ Technology Stack

### Backend
- **.NET 9.0** - Modern C# Framework
- **ASP.NET Core** - REST API
- **Entity Framework Core** - ORM
- **PostgreSQL 16** - Primary Database
- **Redis 7** - Caching & Rate Limiting
- **RabbitMQ** - Message Queue

### Security
- **Argon2id** - Password Hashing
- **JWT** - Authentication Tokens
- **OtpNet** - TOTP MFA
- **AES-256** - Symmetric Encryption
- **AspNetCoreRateLimit** - Brute-Force Protection

### Frontend
- **WPF** - Desktop Client (.NET 9.0)
- **MVVM Pattern** - Clean Architecture

### DevOps
- **Docker** - Containerization
- **docker-compose** - Orchestration
- **GitHub Actions** - CI/CD (optional)

---

## 📂 Project Structure

```
Messenger/
├── src/
│   ├── Backend/
│   │   ├── AuthService/           # ✅ Authentication & MFA
│   │   ├── MessageService/        # ✅ Encrypted Messaging
│   │   ├── AuditLogService/       # ✅ DSGVO Audit Logs
│   │   ├── FileTransferService/   # ✅ Secure File Sharing
│   │   └── ApiGateway/            # ✅ Ocelot Gateway
│   ├── Frontend/
│   │   └── MessengerClient/       # ✅ WPF Desktop App
│   └── Shared/
│       ├── MessengerContracts/    # ✅ DTOs & Interfaces
│       └── MessengerCommon/       # ✅ Extensions & Helpers
├── docs/                          # ✅ Vollständige Dokumentation
├── tests/                         # ✅ Unit & Integration Tests
├── docker-compose.yml             # ✅ Development Setup
└── .env                           # ✅ Environment Variables
```

---

## 🐛 Troubleshooting

### Services starten nicht

```bash
# Prüfe Docker
docker ps -a

# Logs anzeigen
docker logs messenger_auth_service --tail 50
docker logs messenger_postgres --tail 50

# Services neustarten
docker-compose restart

# Oder nutze Script
.\cleanup.ps1
.\deploy-complete.ps1
```

### Datenbank-Fehler

```bash
# PostgreSQL Health Check
docker exec messenger_postgres pg_isready -U messenger_admin

# Verbindung testen
docker exec messenger_postgres psql -U messenger_admin -d messenger_auth -c "\dt"

# Oder: Kompletter Reset
.\cleanup.ps1 -DeleteData
.\deploy-complete.ps1
```

### Rate-Limits zurücksetzen

```bash
# Redis leeren
docker exec messenger_redis redis-cli -a "7PjIbsl21UpGkEV06QRH8aY5ytW3wfov" FLUSHDB
```

### Docker Desktop startet nicht

```powershell
# Script nutzt Auto-Start
.\deploy-complete.ps1

# Oder manuell
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
Start-Sleep -Seconds 45
```

### Alle Probleme beheben (Reset)

```powershell
# WARNUNG: Löscht alle Daten!
.\cleanup.ps1 -DeleteData -Force
.\deploy-complete.ps1 -RebuildImages
```

**Weitere Hilfe:** [SETUP_CHECKLIST.md](SETUP_CHECKLIST.md#-troubleshooting)

---

## 📊 Verfügbare Scripts

| Script | Zweck | Dauer |
|--------|-------|-------|
| `quick-start.ps1` | Schneller Start | 2-3 min |
| `deploy-complete.ps1` | Vollständiges Deployment | 5-7 min |
| `cleanup.ps1` | Services stoppen | 1 min |

**Details:** [SCRIPTS_GUIDE.md](SCRIPTS_GUIDE.md)

---

## 📞 Support & Contributing

- **Issues**: [GitHub Issues](https://github.com/Krialder/Messenger-App/issues)
- **Setup-Hilfe**: [SETUP_CHECKLIST.md](SETUP_CHECKLIST.md)
- **Dokumentation**: [docs/](docs/)
- **Wiki**: [GitHub Wiki](https://github.com/Krialder/Messenger-App/wiki)

---

## 📄 License

MIT License - siehe [LICENSE](LICENSE)

---

**Entwickelt mit ❤️ für sichere Kommunikation**

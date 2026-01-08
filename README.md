# Secure Messenger Application - Architektur & Planung

Eine **geplante** Ende-zu-Ende verschlüsselte Messenger-Anwendung für Windows-PC mit erweiterten Sicherheitsfeatures.

> ⚠️ **Projektstatus**: Dieses Repository enthält die **Architektur-Dokumentation und Pseudo-Code-Implementierung**. Die vollständige Implementierung steht noch aus.

## 📋 Was ist das hier?

**Vollständig strukturiertes Planungsprojekt** für einen sicheren Messenger ähnlich Signal mit zusätzlichen Features:

- **Layer 1**: End-to-End Verschlüsselung (wie Signal) - ChaCha20-Poly1305 + X25519
- **Layer 2**: Lokale Datenverschlüsselung - Schutz bei Gerätediebstahl (AES-256-GCM + Argon2id)
- **Layer 3**: Display-Verschlüsselung (optional) - Anti-Shoulder-Surfing mit PIN

## 📚 Was enthält dieses Repository?

### ✅ Vollständig implementiert (Pseudo-Code)

**Backend Services** (9 Microservices):
- ✅ **AuthService** - JWT, MFA (TOTP, YubiKey, FIDO2)
- ✅ **MessageService** - Verschlüsselte Nachrichten, RabbitMQ
- ✅ **NotificationService** - SignalR Real-time, Presence Management
- ✅ **CryptoService** - Layer 1-3 Verschlüsselung
- ✅ **KeyManagementService** - Schlüsselrotation, Lifecycle
- ✅ **UserService** - Profile, Kontakte
- ✅ **AuditLogService** - DSGVO-konformes Logging
- ✅ **FileTransferService** - Verschlüsselter Datei-Upload
- ✅ **GatewayService** - API Gateway (Ocelot), Rate Limiting

**Shared Libraries**:
- ✅ **MessengerContracts** - DTOs, Interfaces
- ✅ **MessengerCommon** - Constants, Extensions, Helpers

**Frontend**:
- ✅ **WPF Client** - MVVM, ReactiveUI, Material Design
- ✅ **Themes** - Dark Mode, Midnight Mode

**Tests**:
- ✅ **MessengerTests** - Unit & Integration Tests
- ✅ **MessengerTests.E2E** - End-to-End Tests
- ✅ **MessengerTests.Performance** - Performance Benchmarks

**Infrastructure**:
- ✅ **Docker Compose** - Alle 9 Services
- ✅ **CI/CD Pipeline** - GitHub Actions
- ✅ **Database Schema** - PostgreSQL init-db.sql

**Dokumentation**:
- ✅ **18 PlantUML-Diagramme**
- ✅ **9 Dokumentations-Dateien**
- ✅ **Vollständige API-Referenz**
- ✅ **Deployment-Guide**

### ❌ Nicht vorhanden

- ⏳ **Produktionscode** - Pseudo-Code muss ersetzt werden
- ⏳ **EF Core Migrations** - Datenbankmigrationen fehlen
- ⏳ **Echte Kryptographie** - Bibliotheken integrieren
- ⏳ **SignalR Implementierung** - Real-time Events vervollständigen

## 🎯 Hauptmerkmale (geplant)

### Sicherheit
- **Dreistufige Verschlüsselung**:
  - **Layer 1**: Server kann Nachrichten nicht lesen (Zero-Knowledge)
  - **Layer 2**: Lokale Daten verschlüsselt (Schutz bei Gerätediebstahl)
  - **Layer 3**: Optional - Display-Schutz vor Mitlesen
- **Multi-Factor Authentication**:
  - TOTP (Google Authenticator, Authy)
  - YubiKey Hardware Token
  - FIDO2/WebAuthn
  - Recovery Codes
- Automatische Schlüsselrotation
- Forward Secrecy

### Compliance
- **DSGVO-konform**:
  - Verschlüsselung at Rest (Layer 2)
  - Recht auf Löschung (30-Tage-Frist)
  - Datenexport
- Audit-Logging (relevante Events werden protokolliert)

### Features
- Real-time Messaging (SignalR)
- Dark Mode, Midnight Mode
- Typing Indicators & Read Receipts
- Contact Management
- Encrypted File Transfer (100 MB max)

### Technologie
- Backend: .NET 8 / ASP.NET Core
- Frontend: WPF (.NET 8) + ReactiveUI + Material Design
- Database: PostgreSQL 16
- Cache: Redis 7
- Message Queue: RabbitMQ 3
- API Gateway: Ocelot

## 🏗️ Architektur

![System Architecture](docs/diagrams/PNG/01_system_architecture.png)
> **Quelle**: [01_system_architecture.puml](docs/diagrams/01_system_architecture.puml)

### Microservices-Übersicht

```
┌─────────────────────────────────────────────────────────────┐
│                     API Gateway (Ocelot)                    │
│        Rate Limiting | Routing | Load Balancing            │
└─────────────────────────────────────────────────────────────┘
                            ▼
    ┌───────────────────────────────────────────────────────┐
    │                                                       │
    ▼                 ▼                 ▼                 ▼
┌─────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│  Auth   │     │ Messages │     │   Keys   │     │  Users   │
│ Service │     │ Service  │     │ Service  │     │ Service  │
└─────────┘     └──────────┘     └──────────┘     └──────────┘
    │                 │                 │                 │
    ▼                 ▼                 ▼                 ▼
┌─────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│   MFA   │     │ Notific. │     │  Files   │     │  Audit   │
│         │     │ (SignalR)│     │ Transfer │     │   Logs   │
└─────────┘     └──────────┘     └──────────┘     └──────────┘
```

### Verschlüsselungs-Layers

```
┌─────────────────────────────────────────────────────────────┐
│ Layer 3 (Optional): Display Encryption                     │
│ - Anti-Shoulder-Surfing                                    │
│ - AES-256-GCM, PIN-basiert                                 │
└─────────────────────────────────────────────────────────────┘
                            ▼
┌─────────────────────────────────────────────────────────────┐
│ Layer 2: Local Storage Encryption                          │
│ - Gerätediebstahl-Schutz                                   │
│ - AES-256-GCM + Argon2id                                   │
│ - Verschlüsselt: Private Keys, Messages, Contacts          │
└─────────────────────────────────────────────────────────────┘
                            ▼
┌─────────────────────────────────────────────────────────────┐
│ Layer 1: E2E Transport Encryption                          │
│ - Zero-Knowledge Server                                    │
│ - ChaCha20-Poly1305 + X25519                               │
│ - Ephemeral Keys (Forward Secrecy)                         │
└─────────────────────────────────────────────────────────────┘
```

## 📚 Dokumentation

📋 **[Dokumentations-Index](docs/00_INDEX.md)** - Zentrale Übersicht  
📋 **[Workspace Guide](WORKSPACE_GUIDE.md)** - Vollständige Struktur-Übersicht

### Hauptdokumente

| Dokument | Beschreibung | Status |
|----------|--------------|--------|
| [Projektantrag](ProjectProposal/01_PROJECT_PROPOSAL.md) | Projektübersicht, Ziele, Budget | ✅ Dokumentiert |
| [System-Architektur](docs/01_SYSTEM_ARCHITECTURE.md) | Microservices, Deployment | ✅ Dokumentiert |
| [Kryptographie](docs/03_CRYPTOGRAPHY.md) | Layer 1-3, Algorithmen | ✅ Dokumentiert |
| [Use Cases](docs/04_USE_CASES.md) | User Stories, Diagramme | ✅ Dokumentiert |
| [Datenmodell](docs/05_DATA_MODEL.md) | PostgreSQL Schema, ERD | ✅ Dokumentiert |
| [Multi-Factor Authentication](docs/06_MULTI_FACTOR_AUTHENTICATION.md) | TOTP, YubiKey, FIDO2 | ✅ Dokumentiert |
| [Implementierungsplan](docs/07_IMPLEMENTATION_PLAN.md) | Sprint-Planung | ✅ Dokumentiert |
| [Testing](docs/08_TESTING.md) | Unit, Integration, E2E Tests | ✅ Dokumentiert |
| [API Reference](docs/09_API_REFERENCE.md) | REST API Endpoints | ✅ Dokumentiert |
| [Deployment](docs/10_DEPLOYMENT.md) | Docker, CI/CD | ✅ Dokumentiert |

### Diagramme

Alle Diagramme: [`docs/diagrams/`](docs/diagrams/)

**Architektur**:
- [System Architecture](docs/diagrams/PNG/01_system_architecture.png)
- [Encryption Layers](docs/diagrams/PNG/02_encryption_layers.png)
- [Docker Deployment](docs/diagrams/PNG/15_docker_deployment.png)

**Sequenzen**:
- [Send Message Flow](docs/diagrams/PNG/03_send_message_sequence.png)
- [Key Rotation](docs/diagrams/PNG/04_key_rotation_sequence.png)
- [MFA Enable](docs/diagrams/PNG/16_mfa_enable_sequence.png)
- [MFA Login](docs/diagrams/PNG/18_mfa_login_sequence.png)

**Datenmodell**:
- [Entity Relationship](docs/diagrams/PNG/07_entity_relationship.png)

📋 **Vollständige Übersicht**: [DIAGRAM_REFERENCE.md](docs/diagrams/DIAGRAM_REFERENCE.md)

## 🛠️ Geplanter Tech-Stack

### Backend
- **Framework**: .NET 8 / ASP.NET Core
- **Datenbank**: PostgreSQL 16
- **Caching**: Redis 7
- **Message Queue**: RabbitMQ 3
- **API Gateway**: Ocelot 22
- **Kryptographie**: libsodium-net, .NET Cryptography

### Frontend
- **Framework**: WPF (.NET 8)
- **MVVM**: ReactiveUI
- **UI**: Material Design in XAML
- **Real-time**: SignalR Client
- **Local Storage**: SQLite (verschlüsselt)

### DevOps
- **Container**: Docker & Docker Compose
- **CI/CD**: GitHub Actions
- **Logging**: Serilog + Seq

## 🔒 Sicherheitskonzept

### Algorithmen

| Layer | Algorithmus | Schlüssellänge | Zweck |
|-------|------------|----------------|-------|
| **Layer 1** | ChaCha20-Poly1305 | 256 Bit | E2E Transport |
| **Layer 1** | X25519 | 256 Bit | Key Exchange |
| **Layer 2** | AES-256-GCM | 256 Bit | Local Storage |
| **Layer 2** | Argon2id | 256 Bit Output | Password Hashing |
| **Layer 3** | AES-256-GCM | 256 Bit | Display Encryption |

### Schutz gegen

| Bedrohung | Layer 1 | Layer 2 | Layer 3 | Resultat |
|-----------|---------|---------|---------|----------|
| **Server-Kompromittierung** | ✅ Zero-Knowledge | ✅ Kein Master Key | - | ✅ Geschützt |
| **Gerätediebstahl** | ❌ | ✅ Verschlüsselt ohne Passwort | - | ✅ Geschützt |
| **Shoulder Surfing** | - | - | ✅ Display Obfuscation | ✅ Geschützt |
| **Malware/Keylogger** | ❌ | ❌ | ❌ | ❌ Nicht geschützt |

**Das System schützt NICHT vor**:
- ❌ Kompromittiertem Client (Malware, Keylogger)
- ❌ Memory-Dumps während aktiver Session
- ❌ Physischem Zugriff auf entsperrten PC

## 📅 Implementierungs-Zeitplan

**Projektdauer**: 6 Monate (12 Sprints à 2 Wochen)  
**Status**: 📋 **Strukturierungsphase abgeschlossen**

| Phase | Sprints | Wochen | Fokus | Deliverable |
|-------|---------|--------|-------|-------------|
| **Phase 1** | 1-3 | 1-6 | Infrastructure, Auth, Crypto | Auth & Basic Crypto |
| **Phase 2** | 4-7 | 7-14 | Services, Backend | Backend vollständig |
| **Phase 3** | 8-10 | 15-20 | UI, Real-time | Funktionsfähiger Messenger |
| **Phase 4** | 11-12 | 21-24 | Security, DSGVO | Production-Ready |

Details: [Implementierungsplan](docs/07_IMPLEMENTATION_PLAN.md)

## 🚀 Nächste Schritte

### Wenn Implementierung startet:

1. **Dokumentation lesen**: [Dokumentations-Index](docs/00_INDEX.md)
2. **Workspace-Struktur verstehen**: [WORKSPACE_GUIDE.md](WORKSPACE_GUIDE.md)
3. **Pseudo-Code ersetzen**: Echte Implementierung in Services
4. **Datenbank-Migrationen**: EF Core Migrations erstellen
5. **Tests erweitern**: > 80% Coverage erreichen

### Setup (Pseudo-Code testen)

```bash
git clone https://github.com/Krialder/Messenger.git
cd Messenger

# Restore NuGet packages
dotnet restore

# Start infrastructure
docker-compose up -d

# Run tests
dotnet test

# Run WPF Client
cd src/Frontend/MessengerClient
dotnet run
```

## 🧪 Testing-Strategie

- **Unit Tests**: > 80% Coverage (Crypto: > 90%)
- **Integration Tests**: API, Database, RabbitMQ
- **E2E Tests**: Critical Workflows (Login, Message Flow)
- **Performance Tests**: Encryption < 10ms
- **Security Tests**: SQL Injection, XSS, Rate Limiting

Details: [Testing-Strategie](docs/08_TESTING.md)

## 📦 Projekt-Struktur

```
Messenger/
├── src/
│   ├── Backend/                     # 9 Microservices
│   │   ├── GatewayService/          # ✅ NEW - API Gateway
│   │   ├── AuthService/
│   │   ├── MessageService/
│   │   ├── NotificationService/     # ✅ NEW - SignalR
│   │   ├── CryptoService/
│   │   ├── KeyManagementService/
│   │   ├── UserService/
│   │   ├── FileTransferService/     # ✅ NEW - Encrypted Files
│   │   └── AuditLogService/
│   ├── Shared/                      # ✅ NEW - Shared Libraries
│   │   ├── MessengerContracts/      # DTOs, Interfaces
│   │   └── MessengerCommon/         # Constants, Extensions
│   └── Frontend/
│       └── MessengerClient/         # WPF Client
├── tests/
│   ├── MessengerTests/              # Unit & Integration
│   ├── MessengerTests.E2E/          # ✅ NEW - End-to-End
│   └── MessengerTests.Performance/  # ✅ NEW - Benchmarks
├── docs/                            # 18 Diagramme + 9 Dokumente
├── docker-compose.yml               # ✅ UPDATED - All 9 services
└── Messenger.sln                    # ✅ UPDATED - 16 projects
```

## 📄 Lizenz

[Noch nicht definiert]

## 🤝 Contributing

Aktuell: Dokumentation verbessern möglich

- 📝 Dokumentation verbessern
- 🖼️ Diagramme erweitern
- 💡 Use Cases hinzufügen
- 🔍 Security Reviews

[CONTRIBUTING.md](CONTRIBUTING.md)

## 📞 Kontakt

- **GitHub**: [Krialder/Messenger](https://github.com/Krialder/Messenger)
- **Issues**: Für Fragen oder Verbesserungsvorschläge

---

**Version**: 4.0  
**Status**: 📋 **Struktur vollständig** - Pseudo-Code vorhanden, Implementierung steht aus  
**Letzte Aktualisierung**: 2025-01-06

## 📝 Changelog

[DOCUMENTATION_CHANGELOG.md](docs/DOCUMENTATION_CHANGELOG.md)

### Neueste Änderungen (v4.0 - Januar 2025)

- ✅ **Vollständige Struktur**: Alle 9 Microservices implementiert (Pseudo-Code)
- ✅ **Shared Libraries**: MessengerContracts + MessengerCommon hinzugefügt
- ✅ **API Gateway**: Ocelot-basiertes Gateway mit Rate Limiting
- ✅ **NotificationService**: Ausgelagert aus MessageService
- ✅ **FileTransferService**: UC-012 verschlüsselter Datei-Upload
- ✅ **Erweiterte Tests**: E2E + Performance Test-Projekte
- ✅ **Docker Compose**: Alle Services containerisiert
- ✅ **Solution-Datei**: 16 Projekte integriert

---

**Dieses Repository ist ein vollständig strukturiertes Planungsprojekt.**  
**Alle Komponenten sind als Pseudo-Code implementiert und bereit für die finale Umsetzung.**

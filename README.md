# Secure Messenger Application - Architektur & Planung

Eine **geplante** Ende-zu-Ende verschlüsselte Messenger-Anwendung für Windows-PC mit erweiterten Sicherheitsfeatures.

> ⚠️ **Projektstatus**: Dieses Repository enthält die **Architektur-Dokumentation und Planung**. Die Implementierung wurde noch nicht gestartet.

## 📋 Was ist das hier?

**Planungsprojekt** für einen sicheren Messenger ähnlich Signal mit zusätzlichen Features:

- **Layer 1**: End-to-End Verschlüsselung (wie Signal) - ChaCha20-Poly1305 + X25519
- **Layer 2**: Lokale Datenverschlüsselung - Schutz bei Gerätediebstahl (AES-256-GCM + Argon2id)
- **Layer 3**: Display-Verschlüsselung (optional) - Anti-Shoulder-Surfing mit PIN

## 📚 Was enthält dieses Repository?

### ✅ Vorhanden

- **18 PlantUML-Diagramme** (System-Architektur, Sequenzdiagramme, ERD)
- **9 Dokumentations-Dateien** mit detaillierter Planung
- **Vollständiges PostgreSQL-Schema**
- **Kryptographie-Konzept** mit Algorithmen-Auswahl
- **Multi-Factor Authentication Design** (TOTP, YubiKey, FIDO2)
- **Implementierungsplan** (12 Sprints, 6 Monate)
- **Testing-Strategie**

### ❌ Nicht vorhanden

- ⏳ Kein Backend-Code
- ⏳ Kein Frontend-Code
- ⏳ Keine Datenbank-Migrationen
- ⏳ Keine Docker-Container
- ⏳ Kein lauffähiger Code

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

### Technologie
- Backend: .NET 8 / ASP.NET Core
- Frontend: WPF (.NET 8) + ReactiveUI + Material Design
- Database: PostgreSQL 16
- Cache: Redis 7
- Message Queue: RabbitMQ 3

## 🏗️ Architektur

![System Architecture](docs/diagrams/PNG/01_system_architecture.png)
> **Quelle**: [01_system_architecture.puml](docs/diagrams/01_system_architecture.puml)

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

### Microservices

**Geplante Services**:
- **Authentication Service** (JWT, MFA)
- **Message Service** (Encrypted Storage)
- **Key Management Service** (Rotation, Lifecycle)
- **User Service** (Profiles, Contacts)
- **Notification Service** (Real-time Push)
- **Audit Log Service** (Logging)

## 📚 Dokumentation

📋 **[Dokumentations-Index](docs/00_INDEX.md)** - Zentrale Übersicht

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
**Status**: 📋 **Planungsphase**

| Phase | Sprints | Wochen | Fokus | Deliverable |
|-------|---------|--------|-------|-------------|
| **Phase 1** | 1-3 | 1-6 | Infrastructure, Auth, Crypto | Auth & Basic Crypto |
| **Phase 2** | 4-7 | 7-14 | Services, Backend | Backend vollständig |
| **Phase 3** | 8-10 | 15-20 | UI, Real-time | Funktionsfähiger Messenger |
| **Phase 4** | 11-12 | 21-24 | Security, DSGVO | Production-Ready |

Details: [Implementierungsplan](docs/07_IMPLEMENTATION_PLAN.md)

### Meilensteine

- ⏳ **Sprint 1-2**: Docker, Auth Service, JWT + MFA
- ⏳ **Sprint 3**: Layer 1 E2E Encryption
- ⏳ **Sprint 4**: Layer 2 Local Storage Encryption
- ⏳ **Sprint 5-6**: Message Service, Key Management
- ⏳ **Sprint 7**: WPF Client Grundlagen
- ⏳ **Sprint 8**: Chat UI
- ⏳ **Sprint 9**: Theme System & MFA (TOTP, Recovery Codes)
- ⏳ **Sprint 10**: Real-time (SignalR) & Enterprise MFA (YubiKey, FIDO2)
- ⏳ **Sprint 11**: DSGVO-Features
- ⏳ **Sprint 12**: Security Hardening

## 🚀 Nächste Schritte (wenn implementiert wird)

1. **Dokumentation lesen**: [Dokumentations-Index](docs/00_INDEX.md)
2. **Architektur verstehen**: [System-Architektur](docs/01_SYSTEM_ARCHITECTURE.md)
3. **Sprint-Plan**: [Implementierungsplan](docs/07_IMPLEMENTATION_PLAN.md)

### Setup (noch nicht verfügbar)

```bash
# Zukünftig geplant:
git clone https://github.com/Krialder/Messenger.git
cd Messenger

dotnet restore
docker-compose up -d
dotnet test

cd src/Client/SecureMessenger.WPF
dotnet run
```

## 🧪 Testing-Strategie

- **Unit Tests**: > 80% Coverage (Crypto: > 90%)
- **Integration Tests**: API, Database, RabbitMQ
- **E2E Tests**: Critical Workflows
- **Security Tests**: SQL Injection, XSS, Rate Limiting
- **Performance Tests**: Encryption < 10ms

Details: [Testing-Strategie](docs/08_TESTING.md)

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

**Version**: 3.4  
**Status**: 📋 **Planungsphase** - Architektur-Dokumentation vorhanden, keine Implementierung  
**Letzte Aktualisierung**: 2025-01-06

## 📝 Changelog

[DOCUMENTATION_CHANGELOG.md](docs/DOCUMENTATION_CHANGELOG.md)

### Neueste Änderungen (v3.4 - Januar 2025)

- ✅ **Projektstatus klargestellt**: Planungsphase, keine Implementierung
- ✅ **Realistische Sprache**: Weniger Marketing, mehr Technik
- ✅ **API-Dokumentation**: Neue Datei 09_API_REFERENCE.md
- ✅ **Deployment-Guide**: Neue Datei 10_DEPLOYMENT.md
- ✅ **Unnötige Aspekte entfernt**: Quantencomputer, zu viele Compliance-Details

---

**Dieses Repository ist ein Planungsprojekt.**  
**Die Dokumentation kann als Grundlage für die Implementierung dienen.**

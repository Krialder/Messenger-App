# Documentation Changelog

## Version 3.4 - Realistische Dokumentation (2025-01-06)

### Hauptänderungen

- ✅ **README.md**: Projektstatus klargestellt
  - Planungsphase explizit erwähnt (keine Implementierung)
  - Realistische Sprache statt Marketing-Sprache
  - Changelog hinzugefügt mit v3.4
  
- ✅ **Neue Datei: docs/09_API_REFERENCE.md**
  - Vollständige REST API Dokumentation
  - Alle Endpoints mit Request/Response-Beispielen
  - Error Responses & Rate Limiting
  - SignalR Events dokumentiert
  
- ✅ **Neue Datei: docs/10_DEPLOYMENT.md**
  - Docker Compose Setup (Dev/Prod)
  - Nginx Reverse Proxy Konfiguration
  - SSL/TLS mit Let's Encrypt
  - Backup & Recovery
  - CI/CD mit GitHub Actions
  - Monitoring & Logging
  
- ✅ **03_CRYPTOGRAPHY.md**: Nüchternere Sprache
  - Quantencomputer-Sektion entfernt (unrealistisch für MVP)
  - Fokus auf praktische Implementierung
  - Layer 3 Session-PIN UX-Verbesserung hinzugefügt
  - Code-Beispiele vereinfacht
  
- ✅ **04_USE_CASES.md**: MFA-Terminologie
  - UC-002: "2FA" → "MFA" im Titel
  - Realistische Formulierungen
  
- ✅ **00_INDEX.md**: Aktualisiert
  - Links zu 09_API_REFERENCE.md
  - Links zu 10_DEPLOYMENT.md
  - Version 3.4 erwähnt

### Entfernte unnötige Aspekte
- ❌ Post-Quantum Cryptography (PQC) Details
- ❌ Zu detaillierte Compliance-Referenzen
- ❌ Marketing-Sprache ("battle-tested", "cutting-edge")
- ❌ Übermäßige theoretische Erklärungen

### Neue Dokumentation
- 📄 **09_API_REFERENCE.md** (komplett neu)
- 📄 **10_DEPLOYMENT.md** (komplett neu)

---

## Version 3.3 - MFA Terminology Update (2025-01-06)

### Diagram Updates
- ✅ **06_use_case_diagram.puml**: MFA Modernisierung
  - UC-002: "Login & **2FA**" → "Login & **MFA**"
  - **NEW**: UC-014 "Manage MFA Methods"
  - **NEW**: UC-019 "Enable Privacy Mode"
  - Complete use case dependencies dokumentiert
  
- ✅ **09_account_status_state.puml**: MFA Terminologie
  - Active State: "**2FA** optional" → "**MFA** optional"
  - Erweiterte MFA-Methoden Notiz
  
- ✅ **16_2fa_enable_sequence.puml**: Renamed zu MFA Enable
  - Titel: "Enable **Two-Factor**..." → "Enable **Multi-Factor**..."
  - API Endpoints: `/api/auth/2fa/...` → `/api/auth/mfa/...`
  - Datenbank: `mfa_methods` table statt `two_factor_enabled`

### Impact
- **Consistency**: 100% MFA-Terminologie
- **Completeness**: Use Case Dependencies vollständig
- **Maintainability**: Neue Use Cases UC-014, UC-019

---

## Version 3.2 - Documentation Index (2025-01-06)

### Documentation Structure
- ✅ **00_INDEX.md**: Zentrale Dokumentations-Übersicht
  - Schnelleinstieg für Entwickler, Security-Architekten, PM
  - Strukturierte Übersicht aller Dokumente
  - Dokumentations-Abhängigkeiten visualisiert
  - Diagramm-Übersicht
  - Versions-Historie

---

## Version 3.1 - Multi-Factor Authentication (2025-01-06)

### Major Changes

#### MFA Architecture
- ✅ **MFA System**: Mehrere Authentifizierungs-Methoden
  - TOTP (Authenticator Apps)
  - YubiKey (Hardware Token)
  - FIDO2/WebAuthn
  - Recovery Codes
  
#### Database Schema
- ✅ **mfa_methods table**: Multi-MFA-Support
- ✅ **recovery_codes table**: Notfall-Zugriffscodes
- ✅ **users table**: REMOVED two_factor_*, ADDED mfa_enabled

#### Documentation
- ✅ **06_MULTI_FACTOR_AUTHENTICATION.md**: Neues Dokument
- ✅ **05_DATA_MODEL.md**: MFA-Tabellen
- ✅ **07_IMPLEMENTATION_PLAN.md**: Sprint 9-10 MFA-Tasks

#### Diagrams
- ✅ **07_entity_relationship.puml**: MFA-Entities
- ✅ **18_mfa_login_sequence.puml**: MFA Login-Flow

---

## Version 3.0 - Layer 2 Refactoring (2024-12-19)

### Major Changes

#### Encryption Architecture
- ✅ **Layer 2 Refactoring**: Password-based Local Storage Encryption
  - Algorithmus: AES-256-GCM (statt RSA-OAEP + AES)
  - Key Derivation: Argon2id(Password, User Salt) → Master Key
  - Performance: 7x schneller (~2-3ms statt ~12-22ms)

#### Database Schema
- ✅ **users table**: `master_key_salt` Spalte (32 Bytes)
- ✅ **crypto_profiles table**: Entfernt

#### Documentation Updates
- ✅ **03_CRYPTOGRAPHY.md**: Layer 2 Details (v4.0)
- ✅ **05_DATA_MODEL.md**: Schema-Änderungen
- ✅ **07_IMPLEMENTATION_PLAN.md**: Sprint 4 aktualisiert

#### Diagrams
- ✅ **02_encryption_layers.puml**: Layer 2 Flow
- ✅ **17_layer3_display_encryption.puml**: Layer 3 Diagramm

---

## Version 2.0 - Initial Architecture (2024-11)

### Initial Documentation
- ✅ Project Proposal
- ✅ System Architecture
- ✅ Cryptography Concept (Layer 1)
- ✅ Use Cases
- ✅ Database Schema
- ✅ Implementation Plan
- ✅ Testing Strategy

### Diagrams
- ✅ 01-16: PlantUML Diagramme
- ✅ PNG Export

---

## Version 1.0 - Project Kickoff (2024-10)

### Initial Setup
- ✅ Repository Setup
- ✅ README.md
- ✅ CONTRIBUTING.md

---

**Maintained by**: Project Team  
**Last Update**: 2025-01-06  
**Next Review**: Sprint 9 (MFA Implementation)


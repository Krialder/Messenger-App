# Implementierungsplan

## 1. Sprint-Übersicht

**Projektdauer**: 6 Monate (24 Wochen)  
**Sprint-Länge**: 2 Wochen  
**Anzahl Sprints**: 12

### Phasen

| Phase | Sprints | Wochen | Fokus | Deliverable |
|-------|---------|--------|-------|-------------|
| **Phase 1: Foundation** | 1-3 | 1-6 | Infrastructure, Auth, Crypto Basics | Funktionierende Auth & Basic Crypto |
| **Phase 2: Core Features** | 4-7 | 7-14 | Services, Layer 2, Backend | Backend vollständig, Client-Grundgerüst |
| **Phase 3: UI & Integration** | 8-10 | 15-20 | Client, Chat UI, Real-time, Layer 3 | Vollständig funktionsfähiger Messenger mit Privacy Mode |
| **Phase 4: Security & Compliance** | 11-12 | 21-24 | DSGVO, Audits, Hardening | Production-Ready Application |

## 2. Sprint-Details (Kompakt)

### Sprint 1-2: Foundation & Auth (Wochen 1-4)

**Ziele**:
- ✅ Docker Compose Environment
- ✅ Solution Structure
- ✅ CI/CD Pipeline
- ✅ Authentication Service (JWT, Basic 2FA)

**Key Tasks**:
1. Repository Setup + Branch Strategy
2. Docker: PostgreSQL, Redis, RabbitMQ
3. Auth Service: Registration, Login
4. Basic 2FA Setup (TOTP Foundation)
5. Unit Tests (> 80% Coverage)

**DoD**: `docker-compose up` funktioniert, Auth-Endpoints working, CI/CD grün

---

### Sprint 3: Kryptographie Layer 1 (Wochen 5-6)

**Ziele**:
- ✅ ChaCha20-Poly1305 Implementation
- ✅ X25519 Key Exchange
- ✅ HKDF Key Derivation
- ✅ Forward Secrecy

**Key Tasks**:
1. Crypto-Library Setup (libsodium-net, BouncyCastle)
2. Interface Design (IEncryptionService, IKeyExchangeService)
3. Encrypt/Decrypt Round-trip Tests
4. Performance Tests (< 100ms)

**DoD**: Layer 1 E2E funktioniert, Performance OK, Tests > 90%

---

### Sprint 4: Layer 2 Local Storage Encryption (Wochen 7-8)

**Ziele**:
- ✅ AES-256-GCM Implementation
- ✅ Argon2id Master Key Derivation
- ✅ Local Storage Service
- ✅ Master Key Salt Management

**Key Tasks**:
1. LocalStorageEncryptionService Implementation
2. MasterKeyDerivationService (Argon2id)
3. User Salt Generation bei Registrierung
4. Private Key Encryption/Decryption mit Master Key
5. Message Cache Encryption (lokal)
6. Performance Tests (< 10ms)

**DoD**: Layer 2 funktioniert, Master Key Derivation, Performance < 10ms

---

### Sprint 5-6: Message & Key Services (Wochen 9-12)

**Ziele**:
- ✅ Message Service (CRUD, RabbitMQ)
- ✅ Key Management Service (Storage, Rotation)

**Key Tasks**:
1. **Message Service**:
   - Database Schema (Partitioning)
   - Repository Pattern
   - RabbitMQ Publisher/Consumer
   - SignalR Events

2. **Key Management**:
   - Public Key Storage
   - Automatic Rotation (Background Service)
   - Key Lifecycle Management
   - Audit Logging

**DoD**: Messages können gesendet/empfangen werden, Keys rotieren automatisch

---

### Sprint 7: WPF Client Grundlagen (Wochen 13-14)

**Ziele**:
- ✅ MVVM Architecture (ReactiveUI)
- ✅ Login/Registration UI
- ✅ HTTP Client für API
- ✅ Crypto-Integration

**Key Tasks**:
1. WPF Project Setup (MaterialDesign)
2. Login/Register Views + ViewModels
3. ApiClient Service (HttpClient)
4. Crypto-Library Reference
5. State Management (Current User, Navigation)

**DoD**: Client kann sich einloggen, JWT Tokens werden gespeichert

---

### Sprint 8: Chat UI (Wochen 15-16)

**Ziele**:
- ✅ Chat-Übersicht
- ✅ Einzelchat-Fenster
- ✅ Message Input & Display
- ✅ Ver-/Entschlüsselung funktioniert

**Key Tasks**:
1. ChatListView (Kontakte, letzte Nachricht)
2. ChatView (Message History, Input)
3. Send Message Flow (Layer 2 + Layer 1)
4. Receive Message Flow (Decrypt)
5. Message Bubbles (Sent vs Received)

**DoD**: Nachrichten können gesendet/empfangen werden, UI benutzerfreundlich

---

### Sprint 9: Theme System & Layer 3 Display Encryption (Wochen 17-18)

**Ziele**:
- ✅ Dark Mode (Default)
- ✅ Midnight Mode
- ✅ Theme Switcher ohne Neustart
- ✅ Layer 3: Privacy Mode (Display Encryption - Optional)
- ✅ Multi-Factor Authentication (MFA) Implementation

**Key Tasks**:
1. Resource Dictionaries (DarkMode.xaml, MidnightMode.xaml)
2. ThemeManager Service
3. Settings UI (Theme Dropdown)
4. Persistence in DB
5. **Layer 3 Implementation (Optional Feature)**:
   - DisplayEncryptionService (AES-256-GCM)
   - Device Key Derivation (DPAPI + PIN)
   - Auto-Obfuscation Service
   - Privacy Mode UI (PIN-Eingabe, Verschleierte Anzeige)
6. **Multi-Factor Authentication**:
   - TOTP Service (RFC 6238)
   - QR Code Generation für Authenticator Apps
   - MFA Methods Management (mfa_methods table)
   - Recovery Codes Generation & Validation
   - YubiKey Challenge-Response (Foundation)
   - UI für MFA-Verwaltung

**DoD**: 3 Themes verfügbar, Wechsel ohne Neustart, **Privacy Mode (Layer 3 - optional) funktioniert**, Nachrichten werden verschleiert angezeigt wenn aktiviert, **TOTP MFA vollständig implementiert**

---

### Sprint 10: Real-time (SignalR) & Enterprise MFA (Wochen 19-20)

**Ziele**:
- ✅ SignalR Hub
- ✅ Push Notifications
- ✅ Online Status
- ✅ Typing Indicators
- ✅ YubiKey & FIDO2 Integration (Enterprise)

**Key Tasks**:
1. NotificationHub (Server-side)
2. SignalRService (Client-side)
3. Push Notifications (OnMessageReceived)
4. Presence Management (Redis)
5. Typing Indicators (Throttled)
6. **YubiKey Integration**:
   - Challenge-Response für Master Key Derivation
   - YubiKey Detection & Communication
   - Backup TOTP Configuration
7. **FIDO2/WebAuthn** (Optional):
   - FIDO2 Credential Registration
   - Assertion Validation
   - Phishing-resistant Login

**DoD**: Real-time Messaging, Online Status, Typing Indicators funktionieren, **YubiKey MFA für Enterprise-Kunden verfügbar**

---

### Sprint 11: DSGVO-Features (Wochen 21-22)

**Ziele**:
- ✅ Datenexport (ZIP)
- ✅ Kontolöschung mit 30-Tage-Frist
- ✅ Audit Logging

**Key Tasks**:
1. Export API (ZIP mit allen Daten)
2. Deletion Logic (30-Tage-Widerruf)
3. Secure Data Overwrite (3x, DoD 5220.22-M)
4. Audit Log Service
5. Privacy Policy UI

**DoD**: Export funktioniert, Löschung DSGVO-konform, Legal Review bestanden

---

### Sprint 12: Security Hardening (Wochen 23-24)

**Ziele**:
- ✅ Penetration Testing
- ✅ Security Code Review
- ✅ Rate Limiting
- ✅ Input Validation

**Key Tasks**:
1. Static Code Analysis (SonarQube)
2. Dependency Scan
3. External Penetration Test
4. Rate Limiting (API Gateway)
5. FluentValidation überall
6. Security Headers (HSTS, CSP)

**DoD**: Penetration Test bestanden, keine kritischen Vulnerabilities, BSI-konform

---

## 3. Team & Velocity

### Team-Struktur

| Rolle | Anzahl | Verantwortung |
|-------|--------|---------------|
| Project Manager | 1 | Koordination, Stakeholder, Risiken |
| Backend Developer | 2 | Microservices, APIs, Datenbank |
| Frontend Developer | 1 | WPF Client, UI/UX |
| Security Engineer | 1 | Kryptographie, Security Audits |
| DevOps Engineer | 1 | Docker, CI/CD, Infrastructure |
| QA Engineer | 1 | Testing, Quality Assurance |

### Velocity

- **Team-Capacity**: 56 Story Points pro Sprint (7 Personen × 8 SP)
- **Gesamt-Story-Points**: ~578 über 12 Sprints

> **📋 Detaillierte Task-Breakdown**: Siehe ursprünglicher [Implementierungsplan - Archivversion](07_IMPLEMENTATION_PLAN_ARCHIVE.md)

---

## 4. Risikomanagement

| Risiko | Wahrscheinlichkeit | Impact | Mitigation |
|--------|-------------------|--------|------------|
| **Kryptographie-Bugs** | Mittel | Kritisch | Code Reviews, externe Audits, bewährte Bibliotheken |
| **Performance-Probleme** | Niedrig | Mittel | Layer 2 sehr performant (~0.5ms), Hardware-Beschleunigung (AES-NI) |
| **DSGVO-Lücken** | Niedrig | Kritisch | Legal Review ab Sprint 1, regelmäßige Compliance-Checks |
| **Scope Creep** | Hoch | Mittel | Striktes Sprint Planning, Change Request Process |
| **Layer 3 UX-Komplexität** | Mittel | Mittel | User Testing in Sprint 9, optionales Feature |

---

## 5. Abhängigkeiten

```
Sprint 1 (Infrastructure) ──┐
                            ├──► Sprint 2 (Auth) ──► Sprint 7 (WPF Client)
Sprint 3 (Crypto L1) ───────┤                            │
                            │                            ▼
Sprint 4 (Crypto L2) ───────┼──────────────────► Sprint 8 (Chat UI)
                            │                            │
Sprint 5-6 (Services) ──────┤                            ▼
                            │                      Sprint 10 (Real-time)
                            │                            │
                            └──────────────────────────► Sprint 11-12 (DSGVO, Security)
```

---

## 6. Sprint Ceremonies

| Ceremony | Dauer | Frequenz |
|----------|-------|----------|
| **Daily Standup** | 15 Min | Täglich |
| **Sprint Planning** | 4 Std | Sprint-Start |
| **Sprint Review** | 2 Std | Sprint-Ende |
| **Sprint Retrospective** | 1.5 Std | Sprint-Ende |

---

## 7. Quality Gates pro Sprint

✅ Alle Tests grün (CI/CD)  
✅ Code Coverage > 80%  
✅ Code Review durchgeführt  
✅ Security Review (für Crypto-Code)  
✅ Performance-Tests bestanden  
✅ Dokumentation aktualisiert

---

**Dokument-Version**: 2.0  
**Letzte Aktualisierung**: 2024  
**Sprint Planning Owner**: Project Manager

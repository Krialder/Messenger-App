# 📚 Secure Messenger - Dokumentations-Index

**Version:** 3.3 | **Letzte Aktualisierung:** 2025-01-06 | **Status:** ✅ Aktuell

---

## 🎯 Schnelleinstieg

### Für neue Entwickler
Empfohlener Lesepfad:

1. **[README.md](../README.md)** - Projekt-Übersicht
2. **[Projektvorschlag](../ProjectProposal/01_PROJECT_PROPOSAL.md)** - Vision, Ziele
3. **[Use Cases](04_USE_CASES.md)** - Anwendungsfälle
4. **[System-Architektur](01_SYSTEM_ARCHITECTURE.md)** - Architektur-Überblick
5. **[Implementierungsplan](07_IMPLEMENTATION_PLAN.md)** - Roadmap
6. **[CONTRIBUTING.md](../CONTRIBUTING.md)** - Entwickler-Guidelines

### Für Security-Architekten
Fokus auf Sicherheit:

1. **[Kryptographie](03_CRYPTOGRAPHY.md)** - E2EE, Layer 1-3
2. **[Multi-Factor Authentication](06_MULTI_FACTOR_AUTHENTICATION.md)** - MFA-System
3. **[Defense in Depth](diagrams/PNG/14_defense_in_depth.png)** - Sicherheits-Layer
4. **[Testing](08_TESTING.md)** - Security Testing

### Für Projektmanager
Planung & Status:

1. **[Projektvorschlag](../ProjectProposal/01_PROJECT_PROPOSAL.md)** - Business Case
2. **[Implementierungsplan](07_IMPLEMENTATION_PLAN.md)** - Timeline
3. **[Use Cases](04_USE_CASES.md)** - Feature-Übersicht
4. **[Changelog](DOCUMENTATION_CHANGELOG.md)** - Änderungshistorie

---

## 📖 Dokumentations-Struktur

### 🏗️ Architektur & Design

| Dokument | Beschreibung | Status | Zielgruppe |
|----------|--------------|--------|------------|
| **[01_SYSTEM_ARCHITECTURE](01_SYSTEM_ARCHITECTURE.md)** | Client/Server Architektur | ✅ | Architekten, Dev |
| **[02_SYSTEM_COMPONENTS](02_SYSTEM_COMPONENTS.md)** | Komponenten-Details | ✅ | Dev, Architekten |
| **[Diagram: 01](diagrams/PNG/01_system_architecture.png)** | System-Architektur | ✅ | Alle |
| **[Diagram: 12](diagrams/PNG/12_client_component_architecture.png)** | Client-Komponenten | ✅ | Frontend Dev |

---

### 🔐 Sicherheit & Kryptographie

| Dokument | Beschreibung | Status | Zielgruppe |
|----------|--------------|--------|------------|
| **[03_CRYPTOGRAPHY](03_CRYPTOGRAPHY.md)** | E2EE, Layer 1-3 | ✅ | Security, Dev |
| **[06_MULTI_FACTOR_AUTHENTICATION](06_MULTI_FACTOR_AUTHENTICATION.md)** | TOTP, YubiKey, FIDO2 | ✅ | Security, Dev |
| **[Diagram: 02](diagrams/PNG/02_encryption_layers.png)** | Verschlüsselungs-Layer | ✅ | Security |
| **[Diagram: 14](diagrams/PNG/14_defense_in_depth.png)** | Defense in Depth | ✅ | Security |
| **[Diagram: 18](diagrams/PNG/18_mfa_login_sequence.png)** | MFA Login-Flow | ✅ | Security, Dev |

**Kernthemen**: 
- **Kryptographie:** ChaCha20-Poly1305, X25519, Argon2id, AES-256-GCM
- **MFA:** TOTP, YubiKey, FIDO2, Recovery Codes
- **Compliance:** DSGVO, BSI TR-02102

---

### 💾 Datenmodell & Backend

| Dokument | Beschreibung | Status | Zielgruppe |
|----------|--------------|--------|------------|
| **[05_DATA_MODEL](05_DATA_MODEL.md)** | PostgreSQL-Schema | ✅ | Backend Dev |
| **[09_API_REFERENCE](09_API_REFERENCE.md)** | REST API Endpoints | ✅ | Backend/Frontend Dev |
| **[Diagram: 07](diagrams/PNG/07_entity_relationship.png)** | ER-Diagramm | ✅ | Backend Dev |

**Kernthemen**: 
- **Tabellen:** users, messages, contacts, mfa_methods, recovery_codes
- **API:** REST Endpoints, SignalR Events
- **Änderungen v3.1:** MFA-Tabellen hinzugefügt

---

### 👤 Use Cases & Features

| Dokument | Beschreibung | Status | Zielgruppe |
|----------|--------------|--------|------------|
| **[04_USE_CASES](04_USE_CASES.md)** | Anwendungsfälle | ✅ | Alle |
| **[Diagram: 03](diagrams/PNG/03_send_message_sequence.png)** | Nachricht senden | ✅ | Dev |
| **[Diagram: 05](diagrams/PNG/05_dsgvo_deletion_sequence.png)** | DSGVO-Löschung | ✅ | Dev, Legal |
| **[Diagram: 06](diagrams/PNG/06_use_case_diagram.png)** | Use Case Übersicht | ✅ | Alle |

**Use Cases**:
- UC-001: Registrierung
- UC-002: Login mit MFA
- UC-003: Nachricht senden
- UC-008: Daten exportieren (DSGVO)
- UC-009: Konto löschen

---

### 🚀 Planung & Entwicklung

| Dokument | Beschreibung | Status | Zielgruppe |
|----------|--------------|--------|------------|
| **[07_IMPLEMENTATION_PLAN](07_IMPLEMENTATION_PLAN.md)** | Sprint-Planung, Roadmap | ✅ | PM, Dev |
| **[08_TESTING](08_TESTING.md)** | Testing-Strategie | ✅ | QA, Dev |
| **[10_DEPLOYMENT](10_DEPLOYMENT.md)** | Docker, CI/CD | ✅ | DevOps |
| **[CONTRIBUTING.md](../CONTRIBUTING.md)** | Entwickler-Guidelines | ✅ | Dev |

**Sprints**:
- **Sprint 1-3:** Infrastructure, Auth, Crypto
- **Sprint 4-7:** Backend Services
- **Sprint 8-10:** UI, Real-time
- **Sprint 11-12:** DSGVO, Security Hardening

---

### 📊 Diagramme

| ID | Diagramm | Typ | Status |
|----|----------|-----|--------|
| 01 | [System Architecture](diagrams/PNG/01_system_architecture.png) | Component | ✅ |
| 02 | [Encryption Layers](diagrams/PNG/02_encryption_layers.png) | Component | ✅ |
| 03 | [Send Message](diagrams/PNG/03_send_message_sequence.png) | Sequence | ✅ |
| 07 | [Entity Relationship](diagrams/PNG/07_entity_relationship.png) | ERD | ✅ |
| 12 | [Client Components](diagrams/PNG/12_client_component_architecture.png) | Component | ✅ |
| 14 | [Defense in Depth](diagrams/PNG/14_defense_in_depth.png) | Mindmap | ✅ |
| 16 | [MFA Enable](diagrams/PNG/16_mfa_enable_sequence.png) | Sequence | ✅ |
| 18 | [MFA Login](diagrams/PNG/18_mfa_login_sequence.png) | Sequence | ✅ |

📋 **Vollständige Referenz:** [DIAGRAM_REFERENCE.md](diagrams/DIAGRAM_REFERENCE.md)

---

### 📝 Meta-Dokumentation

| Dokument | Beschreibung |
|----------|--------------|
| **[DOCUMENTATION_CHANGELOG](DOCUMENTATION_CHANGELOG.md)** | Versions-History |
| **[DIAGRAM_REFERENCE](diagrams/DIAGRAM_REFERENCE.md)** | Diagramm-Übersicht |

---

## 🔗 Dokumentations-Abhängigkeiten

### Kernkonzepte-Graph

```
01_PROJECT_PROPOSAL
    ├── 01_SYSTEM_ARCHITECTURE
    │   ├── 02_SYSTEM_COMPONENTS
    │   ├── 05_DATA_MODEL
    │   └── 09_API_REFERENCE
    ├── 03_CRYPTOGRAPHY
    │   └── 06_MULTI_FACTOR_AUTHENTICATION
    └── 04_USE_CASES
        ├── 07_IMPLEMENTATION_PLAN
        ├── 08_TESTING
        └── 10_DEPLOYMENT
```

---

## 🎨 Diagramm-Typen

### PlantUML Diagramme

Alle Diagramme als `.puml` und `.png` verfügbar:

- **Pfad:** `docs/diagrams/*.puml`
- **PNG:** `docs/diagrams/PNG/*.png`

**Lokales Rendering:**
```bash
plantuml "docs/diagrams/*.puml" -o PNG
```

---

## 📌 Versions-Übersicht

| Version | Datum | Hauptänderungen |
|---------|-------|-----------------|
| **3.4** | 2025-01-06 | Realistische Sprache, API-Doku, Deployment-Guide |
| **3.3** | 2025-01-06 | MFA Terminology (2FA → MFA), UC-014, UC-019 |
| **3.2** | 2025-01-06 | Dokumentations-Index erstellt |
| **3.1** | 2024 | Multi-Factor Authentication System |
| **3.0** | 2024 | Layer 2 Refactoring, Defense in Depth |

📖 **Details:** [DOCUMENTATION_CHANGELOG.md](DOCUMENTATION_CHANGELOG.md)

---

## ✅ Dokumentations-Qualität

### Aktuelle Metriken
- **Vollständigkeit:** 100% ✅
- **Konsistenz:** Excellent ✅
- **Aktualität:** v3.4 ✅
- **Diagramme:** 18/18 ✅
- **Cross-References:** Alle valide ✅

---

## 🤝 Beitragen zur Dokumentation

### Standards

- **Format:** Markdown
- **Diagramme:** PlantUML (`.puml` + `.png`)
- **Versionierung:** Semantic Versioning
- **Changelog:** Jede Änderung dokumentieren

### Workflow

1. Änderungen in `.md` File
2. `DOCUMENTATION_CHANGELOG.md` aktualisieren
3. Diagramme neu rendern (falls geändert)
4. Cross-References prüfen
5. Commit mit `docs:` Prefix

📖 **Details:** [CONTRIBUTING.md](../CONTRIBUTING.md)

---

**Letzte Aktualisierung:** 2025-01-06 | **Version:** 3.4 | **Status:** ✅ Aktuell

# Projektantrag: Secure Messenger Application

## 1. Executive Summary

Entwicklung einer hochsicheren Messenger-Anwendung für Windows-PC mit **dreistufiger Verschlüsselung**, welche Sicherheitsstandards (DSGVO, BSI) erfüllt und gleichzeitig benutzerfreundlich ist. Die Anwendung setzt auf Microservice-Architektur mit Docker-Containerisierung für Skalierbarkeit.

### Projektziele
- Sichere End-to-End verschlüsselte Kommunikation ähnlich Signal
- **Zusätzliche Sicherheitsebenen**: Local Storage Encryption (at Rest) und optionale Display-Verschlüsselung
- Vollständige DSGVO- und BSI-Konformität
- Benutzerfreundliche UI mit Theme-System
- Skalierbare Microservice-Architektur
- Vollständige Docker-Containerisierung

## 2. Projektbegründung

### Marktbedarf
- Wachsende Nachfrage nach sicherer Kommunikation
- Bestehende Lösungen bieten oft keine Encryption at Rest für lokale Daten
- Bedarf an selbst-gehosteten, kontrollierbaren Messenger-Lösungen

### Wettbewerbsvorteile
1. **Dreistufige Verschlüsselung**: 
   - E2E Transport-Verschlüsselung (Layer 1)
   - Password-based Local Storage Encryption at Rest (Layer 2)
   - Optionale Display-Verschlüsselung (Layer 3)
2. **Compliance First**: DSGVO und BSI-Konformität von Anfang an eingebaut
3. **Enterprise-Ready**: Skalierbare Architektur für große Organisationen
4. **Device Theft Protection**: Gerätediebstahl-Schutz durch Layer 2 Encryption at Rest
5. **Self-Hosted Option**: Volle Kontrolle über Daten und Infrastructure

## 3. Funktionale Anforderungen

### 3.1 Sicherheit & Verschlüsselung

#### Layer 1 - Transport-Verschlüsselung (E2E)
- End-to-End Verschlüsselung zwischen Clients
- Algorythmus: ChaCha20-Poly1305 (AEAD)
- Schlüsselaustausch: X25519 (ECDH)
- Automatische Schlüsselrotation nach jeder Nachricht
- Forward Secrecy garantiert
- Zero-Knowledge-Architektur (Server kann nicht entschlüsseln)

**Funktionsweise**:
```
Alice → [Nachricht] → Layer 1 Encrypt (E2E) → Server → Bob
Bob   ← [Nachricht] ← Layer 1 Decrypt (E2E) ← Server ← Alice
Bob   → Layer 2 Encrypt (Local Storage) → SQLite verschlüsselt
```

#### Layer 2 - Local Storage-Verschlüsselung (Encryption at Rest)
- Password-basierte Verschlüsselung lokaler Daten
- Algorithmus: AES-256-GCM (AEAD)
- Schlüssel: Master Key = Argon2id(Benutzerpasswort, Salt)
- Salt: Server-gespeichert (32 Bytes, unique pro User), Master Key **niemals** auf Server
- Verschlüsselt:
  - Private Keys (E2E-Schlüssel für Layer 1)
  - Message Cache (lokal gecachte Nachrichten)
  - Contact-Daten
  - User Settings

**Zweck**: 
- **Gerätediebstahl-Schutz**: Ohne Passwort sind alle lokalen Daten unlesbar
- **Forensik-Resistenz**: Festplatten-Image ohne Master Key nutzlos
- **DSGVO Art. 32**: Erfüllt "Encryption at Rest" Anforderung
- **Malware-Schutz (teilweise)**: Daten nur im RAM während aktiver Session

**Funktionsweise**:
```
┌─────────────────────────────────────────────────────┐
│ Client (AUSGELOGGT)                                 │
│ - Private Keys: verschlüsselt (AES-256-GCM)         │
│ - Message Cache: verschlüsselt (AES-256-GCM)        │
│ ⚠️  Ohne Passwort: Alle Daten unlesbar              │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│ Client (EINGELOGGT)                                 │
│ 1. Password → Argon2id(Password, Salt) → Master Key │
│ 2. Master Key entschlüsselt Private Keys (RAM)      │
│ 3. Master Key entschlüsselt Message Cache (RAM)     │
│ ✅ Bei Logout: Master Key aus RAM gelöscht          │
└─────────────────────────────────────────────────────┘
```

**Master Key Derivation**:
```
User Login → Load Salt from Server → Argon2id(Password, Salt) → Master Key (256 Bit)
                                                                      ↓
                                    Decrypt Private Keys ← AES-256-GCM Decrypt
                                    Decrypt Message Cache ← AES-256-GCM Decrypt
```

**Wichtig**: 
- Master Key existiert **nur im RAM** während der Session
- Salt wird auf Server gespeichert, Master Key **niemals**
- Bei Logout wird Master Key sicher aus RAM gelöscht (`CryptographicOperations.ZeroMemory`)

#### Layer 3 - Display-Verschlüsselung (Optional - Privacy Mode)
- Lokale Verschleierung der Nachrichtenanzeige
- Algorithmus: AES-256-GCM
- Device Key Derivation: Windows DPAPI + User-PIN (Argon2id)
- PIN-basierte Entschlüsselung für temporäre Anzeige
- Auto-Obfuscation nach konfigurierbarem Timeout (Standard: 5 Sekunden)

**Zweck**: Anti-Shoulder-Surfing (Schutz vor physischem Mitlesen in öffentlichen Räumen)

**Funktionsweise**:
```
Bob empfängt → Layer 1 Decrypt → Layer 2 Encrypt (Storage) → Layer 3 Encrypt → Display: 🔒 ████████
Bob klickt "Anzeigen" → PIN-Eingabe → Layer 3 Decrypt → Plaintext (5s) → Auto Re-Encrypt
```

**Anwendungsfälle**:
- Öffentliche Räume (Café, Bibliothek, Büro)
- Geteilte Arbeitsplätze
- Kurz unbeaufsichtigter PC
- Compliance-Anforderungen (zusätzliche Sicherheitsebene)

#### Schlüsselmanagement
- Automatische Schlüsselrotation (Layer 1 - Ephemeral Keys)
- Forward Secrecy
- Secure Key Storage (Layer 2 - verschlüsselt mit Master Key)
- Key Expiration Management
- Emergency Key Revocation

#### Multi-Factor Authentication (MFA)
- **TOTP (Time-based One-Time Password)**: Unterstützung für Authenticator Apps
  - Google Authenticator, Microsoft Authenticator, Authy
  - RFC 6238 konform
  - 30-Sekunden Zeitfenster, 6-stellige Codes
- **YubiKey Hardware Token**: Enterprise-Grade Security
  - Challenge-Response Mode für Master Key Derivation
  - FIDO2/WebAuthn für phishing-resistenten Login
  - Touch-Required für zusätzliche Sicherheit
- **Recovery Codes**: Notfall-Zugriff
  - 10 Codes bei MFA-Aktivierung generiert
  - Einmalige Verwendung
  - Argon2id-gehasht gespeichert
- **Multiple Methods**: Mehrere MFA-Methoden pro User
  - Primary & Backup Methoden konfigurierbar
  - Flexible User Experience

**MFA-Vorteile**:
- ✅ Schutz gegen Passwort-Leaks
- ✅ Phishing-Resistenz (mit FIDO2/YubiKey)
- ✅ Compliance (NIST, BSI, DSGVO)
- ✅ Optionale Hardware-Token-Integration für höchste Sicherheit

### 3.2 Compliance-Anforderungen

#### DSGVO-Konformität
- **Datenschutz by Design**: Verschlüsselung ist Standard
- **Encryption at Rest**: Layer 2 erfüllt Art. 32 DSGVO
- **Multi-Factor Authentication**: Zusätzlicher Schutz personenbezogener Daten
- **Datenminimierung**: Nur notwendige Daten werden gespeichert
- **Recht auf Löschung**: Benutzer können Konto inkl. aller Daten löschen (30-Tage-Widerrufsfrist)
- **Recht auf Datenportabilität**: Export aller persönlichen Daten
- **Transparenz**: Klare Datenschutzerklärung
- **Audit-Logging**: Protokollierung aller Zugriffe auf persönliche Daten (inkl. MFA-Events)
- **Datenschutz-Folgenabschätzung**: Durchgeführt und dokumentiert

#### BSI TR-02102-1 Konformität
- Verwendung zugelassener Algorithmen (ChaCha20-Poly1305, AES-256-GCM)
- Empfohlene Schlüssellängen eingehalten (256 Bit)
- **Mehrere unabhängige Authentifizierungswege** (MFA)
- Sichere Zufallszahlengenerierung (RandomNumberGenerator)
- Argon2id für Password Hashing (OWASP empfohlen)
- Regelmäßige Sicherheitsupdates
- Kryptographische Protokolle nach Stand der Technik

#### NIST SP 800-63B (Digital Identity Guidelines)
- **Authenticator Assurance Level 1 (AAL1)**: Password-basierte Authentifizierung
- **Authenticator Assurance Level 2 (AAL2)**: Multi-Factor Authentication (TOTP, YubiKey)
- **Authenticator Assurance Level 3 (AAL3)**: Hardware Cryptographic Token (FIDO2 YubiKey)
- Phishing-resistente Authenticators empfohlen (FIDO2/WebAuthn)
- Memory-hard Key Derivation Functions (Argon2id)

### 3.3 Benutzerfreundlichkeit

#### User Interface
- Moderne, intuitive Benutzeroberfläche
- Material Design Prinzipien
- MVVM-Architektur für saubere Trennung
- Responsive Design

#### Theme-System
- **Dark Mode**: Standard-Theme
- **Midnight Mode**: Noch dunkleres Theme für OLED-Displays
- **Custom Themes**: Einfache Erweiterbarkeit
- Theme-Wechsel ohne App-Neustart
- Persistierung der Benutzer-Präferenz

**Theme-Struktur**:
```
Themes/
├── DarkMode.xaml
├── MidnightMode.xaml
├── LightMode.xaml (Optional)
└── ThemeManager.cs
```

#### Features
- Real-time Messaging
- Typing Indicators
- Read Receipts
- Online Status
- Contact Management
- Message Search
- File Transfer (verschlüsselt)
- Push Notifications

### 3.4 Skalierbarkeit

#### Architektur-Prinzipien
- **Microservices**: Services sind unabhängig deploybar
- **Horizontal Scaling**: Mehr Instanzen bei Bedarf
- **Load Balancing**: Gleichmäßige Verteilung der Last
- **Stateless Services**: Keine Session-Affinität notwendig
- **Event-Driven**: Asynchrone Kommunikation über Message Queue

#### Performance-Ziele
- Message Encryption (Layer 1): < 100ms
- Local Storage Encryption (Layer 2): < 10ms
- Message Delivery: < 500ms (bei Online-Status)
- API Response Time: < 200ms (p95)
- Concurrent Users: > 10.000 pro Service-Instanz
- Uptime: 99.9%

## 4. Nicht-funktionale Anforderungen

### 4.1 Performance
- Nachrichtenverschlüsselung (Layer 1): < 100ms
- Lokale Verschlüsselung (Layer 2): < 10ms
- Nachrichtenzustellung: < 2 Sekunden (Ende-zu-Ende bei Online)
- UI Response Time: < 100ms
- Database Query Time: < 50ms (p95)

### 4.2 Verfügbarkeit
- Uptime: 99.9% (< 8.76 Stunden Downtime pro Jahr)
- Backup-Strategie: Tägliche automatische Backups
- Disaster Recovery: RTO < 4 Stunden, RPO < 15 Minuten

### 4.3 Wartbarkeit
- Modularer Code mit klaren Schnittstellen
- Dependency Injection für Testbarkeit
- Comprehensive Logging
- Monitoring & Alerting
- Automatisierte Deployment-Pipeline

### 4.4 Testbarkeit
- Unit Test Coverage: > 80%
- Crypto Code Coverage: > 90% (kritisch für Sicherheit)
- Integration Tests für alle APIs
- End-to-End Tests für kritische Workflows
- Performance Tests
- Security Tests

### 4.5 Dokumentation
- Code-Kommentare für komplexe Logik
- API-Dokumentation (OpenAPI/Swagger)
- Architektur-Dokumentation
- Deployment-Guides
- User-Dokumentation

### 4.4 Performance-Charakteristik

| Operation | Layer 1 | Layer 2 | Layer 3* | Gesamt** | Ziel |
|-----------|---------|---------|---------|--------|------|
| **Encryption** | ~1-2 ms | ~0.5 ms | ~0.5 ms | ~2-3 ms | < 10 ms |
| **Decryption** | ~1-2 ms | ~0.5 ms | ~0.5 ms | ~2-3 ms | < 10 ms |
| **Key Generation** | ~0.1 ms | - | ~100 ms (DPAPI) | - | - |
| **Master Key Derivation** | - | ~100-200 ms (Argon2id) | - | - | Einmalig bei Login |
| **PIN Validation** | - | - | ~50-100 ms (Argon2id) | - | < 200 ms |

*Layer 3 nur aktiv wenn Privacy Mode aktiviert ist  
**Gesamt = Layer 1 + Layer 2 (Layer 3 optional)

## 5. Technologische Entscheidungen & Begründungen

### 5.1 Backend-Technologien

#### .NET 8 / ASP.NET Core
**Begründung**:
- Moderne, performante Plattform
- Exzellente Async/Await-Unterstützung
- Cross-Platform (Windows, Linux, macOS)
- Starke Typisierung reduziert Fehler
- Großes Ökosystem
- Kostenlos und Open Source
- Langfristige Microsoft-Unterstützung

#### PostgreSQL
**Begründung**:
- Open Source und kostenlos
- ACID-konform für Datenintegrität
- Exzellente Performance
- Native JSON-Unterstützung
- Encryption at Rest
- Replication & Clustering
- Bewährte Technologie

#### Redis
**Begründung**:
- In-Memory Performance
- Pub/Sub für Real-time Features
- Session Storage
- Caching Layer
- Atomic Operations

#### RabbitMQ
**Begründung**:
- Zuverlässige Message Queue
- Message Persistence
- Clustering & High Availability
- Flexible Routing
- Management UI

### 5.2 Frontend-Technologien

#### WPF (.NET 8)
**Begründung**:
- Native Windows Performance
- Reichhaltige UI-Komponenten
- MVVM-Pattern-Unterstützung
- Data Binding
- Keine Browser-Overhead
- Offline-First
- ❌ Nur Windows (akzeptiert für PC-Fokus)

**Alternative**: Avalonia für Cross-Platform (Linux/macOS)

#### ReactiveUI
**Begründung**:
- Reaktive Programmierung
- Vereinfacht asynchrone UI-Updates
- Testbarkeit
- Observable Collections

### 5.3 Kryptographie-Bibliotheken

#### libsodium-net
**Begründung**:
- NaCl-basiert (Networking and Cryptography library)
- Side-Channel-Attack resistent
- Einfache, sichere API
- Battle-tested
- ChaCha20-Poly1305 Unterstützung

#### .NET Cryptography (System.Security.Cryptography)
**Begründung**:
- Native .NET Integration
- AES-256-GCM für Layer 2 (Local Storage)
- Argon2id für Password Hashing
- Hardware-beschleunigt (AES-NI)
- FIPS-konform

### 5.4 DevOps-Technologien

#### Docker & Docker Compose
**Begründung**:
- Konsistente Umgebungen (Dev/Test/Prod)
- Isolation der Services
- Einfache Skalierung
- Portabilität
- Schnelle Deployments

#### GitHub Actions
**Begründung**:
- Native GitHub-Integration
- Kostenlos für Open Source
- YAML-basierte Konfiguration
- Große Action-Bibliothek
- Matrix Builds

## 6. Projektzeitplan

### Projektdauer
**6 Monate** (24 Wochen / 12 Sprints à 2 Wochen)

### Meilensteine

#### Phase 1: Foundation (Wochen 1-6)
- Sprint 1: Projektsetup & Docker-Infrastructure
- Sprint 2: Authentication Service
- Sprint 3: Kryptographie-Grundlagen (Layer 1 - E2E)

**Deliverable**: Funktionierende Auth & Basic Crypto

#### Phase 2: Core Features (Wochen 7-14)
- Sprint 4: Layer 2 Local Storage Encryption
- Sprint 5: Message Service
- Sprint 6: Key Management Service
- Sprint 7: WPF Client Grundlagen

**Deliverable**: Backend-Services vollständig, Client-Grundgerüst

#### Phase 3: UI & Integration (Wochen 15-20)
- Sprint 8: Chat UI & Crypto-Integration
- Sprint 9: Theme System
- Sprint 10: Real-time Communication (SignalR)

**Deliverable**: Vollständig funktionsfähiger Messenger

#### Phase 4: Security & Compliance (Wochen 21-24)
- Sprint 11: DSGVO-Features (Export, Löschung)
- Sprint 12: Security Hardening & Audits

**Deliverable**: Production-Ready Application

## 7. Ressourcen & Team

### Team-Struktur

| Rolle | Anzahl | Verantwortlichkeiten |
|-------|--------|----------------------|
| Project Manager | 1 | Projektkoordination, Stakeholder-Management, Risikomanagement |
| Backend Developer | 2 | Microservices, APIs, Datenbank-Design |
| Frontend Developer | 1 | WPF Client, UI/UX Design |
| Security Engineer | 1 | Kryptographie-Implementation, Security Audits |
| DevOps Engineer | 1 | Docker, CI/CD, Infrastructure, Monitoring |
| QA Engineer | 1 | Test-Strategie, Automatisierung, Quality Assurance |
| **Total** | **7** | |

### Externe Ressourcen
- **Legal Counsel**: DSGVO-Compliance Review
- **External Security Auditor**: Penetration Testing

## 8. Kosten-Nutzen-Analyse

### Geschätzte Projektkosten (6 Monate)

| Kategorie | Details | Kosten (EUR) |
|-----------|---------|--------------|
| **Personal** | 7 Mitarbeiter × 6 Monate × 8.000€ | 336.000€ |
| **Infrastructure** | Cloud Hosting (Dev/Test/Staging) | 3.000€ |
| **Lizenzen** | IDE, Tools, Cloud Services | 2.000€ |
| **Security Audit** | Externer Penetration Test | 8.000€ |
| **Legal** | DSGVO-Compliance Review | 5.000€ |
| **Contingency** | 10% Risikopuffer | 35.400€ |
| **GESAMT** | | **389.400€** |

### Erwarteter Business Value

#### Direkte Monetarisierung
- **Enterprise Lizenzen**: 500€ - 5.000€ pro Organisation/Jahr
- **On-Premise Deployment**: Einmalig 10.000€ + 2.000€/Jahr Support
- **Premium Features**: 5€ - 20€ pro User/Monat
- **Consulting Services**: 150€/Stunde

#### Indirekte Vorteile
- **Markendifferenzierung**: Höchste Sicherheitsstandards
- **Compliance-Vorteil**: Fertige DSGVO-Lösung (Encryption at Rest)
- **Know-how-Aufbau**: Kryptographie-Expertise
- **Open Source Potential**: Community-Building

### Break-Even-Analyse
- **Investition**: 389.400€
- **Annahme**: 100 Enterprise-Kunden à 2.000€/Jahr = 200.000€/Jahr
- **Break-Even**: ~2 Jahre

## 9. Risikomanagement

### Identifizierte Risiken

| Risiko | Wahrscheinlichkeit | Impact | Mitigation |
|--------|-------------------|--------|------------|
| **Kryptographie-Bugs** | Mittel | Kritisch | - Code Reviews durch Security Engineer<br>- Externe Security Audits<br>- Verwendung bewährter Bibliotheken |
| **Performance-Probleme** | Niedrig | Mittel | - Layer 2 sehr performant (~0.5ms)<br>- Hardware-Beschleunigung (AES-NI) |
| **DSGVO-Compliance-Lücken** | Niedrig | Kritisch | - Legal Review ab Sprint 1<br>- Datenschutz-Folgenabschätzung<br>- Regelmäßige Compliance-Checks |
| **Scope Creep** | Hoch | Mittel | - Striktes Sprint Planning<br>- Change Request Process<br>- Product Owner Entscheidungen |
| **Team-Fluktuation** | Niedrig | Hoch | - Gute Dokumentation<br>- Knowledge Sharing Sessions<br>- Pair Programming |
| **Schlüsselverlust User** | Mittel | Hoch | - User Education: Passwort vergessen = Datenverlust<br>- Klare Warnungen bei Registrierung |
| **Zero-Day in Dependencies** | Niedrig | Hoch | - Dependabot aktivieren<br>- Regelmäßige Updates<br>- Security Monitoring |
| **Layer 3 UX-Komplexität** | Mittel | Mittel | - User Testing in Sprint 9<br>- Optionales Feature<br>- Klare UI-Flows |

### Risiko-Response-Strategien

#### Kryptographie-Bugs
- **Prävention**: Nur bewährte Algorithmen und Bibliotheken
- **Detektion**: Automated Security Testing in CI/CD
- **Response**: Bug Bounty Program nach Release
- **Recovery**: Emergency Patch Process

#### Performance-Probleme
- **Prävention**: Performance-Tests ab Sprint 4
- **Detektion**: Continuous Performance Monitoring
- **Response**: Optimization (bereits sehr schnell: ~2-3ms Layer 2)
- **Escalation**: Hardware-Beschleunigung (z.B. AES-NI)

## 10. Erfolgskriterien

### Technische KPIs
- Layer 1 Verschlüsselung: < 100ms
- Layer 2 Verschlüsselung: < 10ms
- Message Delivery: < 500ms
- Unit Test Coverage: > 80%
- Crypto Test Coverage: > 90%
- API Response Time: < 200ms (p95)
- Uptime: > 99.9%

### Business KPIs
- User Satisfaction Score: > 4.5/5
- Security Audit bestanden ohne kritische Findings
- DSGVO-Compliance bestätigt
- 95% der geplanten Features implementiert

### Qualitätskriterien
- Keine kritischen Bugs bei Release
- Dokumentation vollständig
- Performance-Benchmarks erreicht
- Accessibility-Standards erfüllt

## 11. Nächste Schritte

### Woche 1: Projekt-Kickoff
1. Stakeholder-Approval einholen
2. Team rekrutieren/zuweisen
3. Development Tools aufsetzen
4. Git Repository erstellen
5. Projekt-Board aufsetzen (GitHub Projects)
6. Kickoff-Meeting durchführen

### Woche 2: Sprint 1 Start
1. Docker Compose Environment
2. Solution Structure erstellen
3. CI/CD Pipeline konfigurieren
4. Coding Standards definieren
5. Sprint Planning

## 12. Genehmigung

### Erforderliche Unterschriften

| Rolle | Name | Unterschrift | Datum |
|-------|------|--------------|-------|
| Projektleiter | | | |
| Technischer Leiter | | | |
| Budget-Verantwortlicher | | | |
| Datenschutzbeauftragter | | | |

---

**Dokument-Version**: 2.0  
**Erstellt am**: 2024  
**Letzte Aktualisierung**: 2024-12-19  

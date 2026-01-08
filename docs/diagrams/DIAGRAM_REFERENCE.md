# Diagramm-Referenz-Übersicht

Diese Datei enthält eine Übersicht aller PlantUML-Diagramme und ihrer gerenderten PNG-Dateinamen.

| Nr | PlantUML-Datei | Gerenderte PNG | Verwendung in Dokument | Status |
|----|----------------|----------------|------------------------|--------|
| 01 | [`01_system_architecture.puml`](01_system_architecture.puml) | [`01_system_architecture.png`](PNG/01_system_architecture.png) | [`02_ARCHITECTURE.md`](../02_ARCHITECTURE.md), [`README.md`](../../README.md) | ✅ |
| 02 | [`02_encryption_layers.puml`](02_encryption_layers.puml) | [`02_encryption_layers.png`](PNG/02_encryption_layers.png) | [`02_ARCHITECTURE.md`](../02_ARCHITECTURE.md), [`03_CRYPTOGRAPHY.md`](../03_CRYPTOGRAPHY.md) | ✅ |
| 03 | [`03_send_message_sequence.puml`](03_send_message_sequence.puml) | [`03_send_message_sequence.png`](PNG/03_send_message_sequence.png) | [`02_ARCHITECTURE.md`](../02_ARCHITECTURE.md), [`04_USE_CASES.md`](../04_USE_CASES.md) | ✅ |
| 04 | [`04_key_rotation_sequence.puml`](04_key_rotation_sequence.puml) | [`04_key_rotation_sequence.png`](PNG/04_key_rotation_sequence.png) | [`02_ARCHITECTURE.md`](../02_ARCHITECTURE.md), [`03_CRYPTOGRAPHY.md`](../03_CRYPTOGRAPHY.md) | ✅ |
| 05 | [`05_dsgvo_deletion_sequence.puml`](05_dsgvo_deletion_sequence.puml) | [`05_dsgvo_deletion_sequence.png`](PNG/05_dsgvo_deletion_sequence.png) | [`04_USE_CASES.md`](../04_USE_CASES.md) | ✅ |
| 06 | [`06_use_case_diagram.puml`](06_use_case_diagram.puml) | [`06_use_case_diagram.png`](PNG/06_use_case_diagram.png) | [`04_USE_CASES.md`](../04_USE_CASES.md) | ✅ |
| 07 | [`07_entity_relationship.puml`](07_entity_relationship.puml) | [`07_entity_relationship.png`](PNG/07_entity_relationship.png) | [`05_DATA_MODEL.md`](../05_DATA_MODEL.md) | ✅ |
| 08 | [`08_message_status_state.puml`](08_message_status_state.puml) | [`08_message_status_state.png`](PNG/08_message_status_state.png) | [`04_USE_CASES.md`](../04_USE_CASES.md) | ✅ |
| 09 | [`09_account_status_state.puml`](09_account_status_state.puml) | [`09_account_status_state.png`](PNG/09_account_status_state.png) | [`04_USE_CASES.md`](../04_USE_CASES.md) | ✅ |
| 10 | [`10_key_lifecycle_state.puml`](10_key_lifecycle_state.puml) | [`10_key_lifecycle_state.png`](PNG/10_key_lifecycle_state.png) | [`03_CRYPTOGRAPHY.md`](../03_CRYPTOGRAPHY.md), [`04_USE_CASES.md`](../04_USE_CASES.md) | ✅ |
| 11 | [`11_theme_switch_activity.puml`](11_theme_switch_activity.puml) | [`11_theme_switch_activity.png`](PNG/11_theme_switch_activity.png) | [`04_USE_CASES.md`](../04_USE_CASES.md) | ✅ |
| 12 | [`12_client_component_architecture.puml`](12_client_component_architecture.puml) | [`12_client_component_architecture.png`](PNG/12_client_component_architecture.png) | [`04_USE_CASES.md`](../04_USE_CASES.md) | ✅ |
| 13 | [`13_forward_secrecy_flow.puml`](13_forward_secrecy_flow.puml) | [`13_forward_secrecy_flow.png`](PNG/13_forward_secrecy_flow.png) | [`03_CRYPTOGRAPHY.md`](../03_CRYPTOGRAPHY.md) | ✅ |
| 14 | [`14_defense_in_depth.puml`](14_defense_in_depth.puml) | [`14_defense_in_depth.png`](PNG/14_defense_in_depth.png) | [`02_ARCHITECTURE.md`](../02_ARCHITECTURE.md) | ✅ |
| 15 | [`15_docker_deployment.puml`](15_docker_deployment.puml) | [`15_docker_deployment.png`](PNG/15_docker_deployment.png) | [`02_ARCHITECTURE.md`](../02_ARCHITECTURE.md) | ✅ |
| 16 | [`16_mfa_enable_sequence.puml`](16_2fa_enable_sequence.puml) | [`16_mfa_enable_sequence.png`](PNG/16_mfa_enable_sequence.png) | [`04_USE_CASES.md`](../04_USE_CASES.md), [`06_MULTI_FACTOR_AUTHENTICATION.md`](../06_MULTI_FACTOR_AUTHENTICATION.md) | ✅ |
| 17 | [`17_layer3_display_encryption.puml`](17_layer3_display_encryption.puml) | [`17_layer3_display_encryption.png`](PNG/17_layer3_display_encryption.png) | [`03_CRYPTOGRAPHY.md`](../03_CRYPTOGRAPHY.md), [`04_USE_CASES.md`](../04_USE_CASES.md) | ✅ |
| 18 | [`18_mfa_login_sequence.puml`](18_mfa_login_sequence.puml) | [`18_mfa_login_sequence.png`](PNG/18_mfa_login_sequence.png) | [`06_MULTI_FACTOR_AUTHENTICATION.md`](../06_MULTI_FACTOR_AUTHENTICATION.md), [`README.md`](../../README.md) | ✅ |

## 📊 Status-Übersicht

| Status | Anzahl | Beschreibung |
|--------|--------|--------------|
| ✅ **Vollständig eingefügt** | 13 | Basis-Diagramme erfolgreich eingefügt und verlinkt |
| ✅ **Aktualisiert** | 7 | Diagramme 02, 03, 06, 07, 09, 14, 16 für Layer 3 + MFA erweitert |
| 🆕 **Neu hinzugefügt** | 2 | Diagramm 17 (Layer 3), 18 (MFA Login) |
| ✅ **Gesamt** | 18 | Alle Diagramme aktuell (v3.2) |

## 📁 Alle verfügbaren PNG-Dateien

```
docs/diagrams/PNG/
├── 01_system_architecture.png               ✅ (01 - System Architecture)
├── 02_encryption_layers.png                 ✅ (02 - Encryption Layers) **UPDATED (v3.0)**
├── 03_send_message_sequence.png             ✅ (03 - Send Message Sequence) **UPDATED (v3.0)**
├── 04_key_rotation_sequence.png             ✅ (04 - Key Rotation)
├── 05_dsgvo_deletion_sequence.png           ✅ (05 - DSGVO Deletion)
├── 06_use_case_diagram.png                  ✅ (06 - Use Case Diagram) **UPDATED (v3.2)**
├── 07_entity_relationship.png               ✅ (07 - Entity Relationship) **UPDATED (v3.1)**
├── 08_message_status_state.png              ✅ (08 - Message Status State)
├── 09_account_status_state.png              ✅ (09 - Account Status State) **UPDATED (v3.2)**
├── 10_key_lifecycle_state.png               ✅ (10 - Key Lifecycle State)
├── 11_theme_switch_activity.png             ✅ (11 - Theme Switch Activity)
├── 12_client_component_architecture.png     ✅ (12 - Client Component Architecture)
├── 13_forward_secrecy_flow.png              ✅ (13 - Forward Secrecy Flow)
├── 14_defense_in_depth.png                  ✅ (14 - Defense in Depth) **UPDATED (v3.0)**
├── 15_docker_deployment.png                 ✅ (15 - Docker Deployment)
├── 16_mfa_enable_sequence.png               ✅ (16 - MFA Enable Sequence) **UPDATED (v3.2)**
├── 17_layer3_display_encryption.png         🆕 (17 - Layer 3 Display Encryption) **NEW (v3.0)**
└── 18_mfa_login_sequence.png                🆕 (18 - MFA Login Sequence) **NEW (v3.1)**
```

## ✅ Projekt-Update: MFA Terminology Update (v3.2)

**Multi-Faktor-Authentifizierung Terminologie in Diagrammen vereinheitlicht!**

### Änderungen v3.2 (2025-01-06):

#### Aktualisierte Diagramme:
1. **06_use_case_diagram.puml**: MFA Modernisierung
   - UC-002: "Login & **2FA**" → "Login & **MFA**"
   - **NEU**: UC-014 "Manage MFA Methods" (Primary/Backup, Recovery Codes)
   - **NEU**: UC-019 "Enable Privacy Mode" (Layer 3 Display Encryption)
   - Erweiterte Notizen mit allen MFA-Methoden (TOTP, YubiKey, FIDO2)
   - PNG: `06_use_case_diagram.png` ✅ Aktualisiert

2. **09_account_status_state.puml**: MFA Terminologie
   - Active State: "**2FA** optional" → "**MFA** optional"
   - Erweiterte Notiz mit allen MFA-Methoden
   - Deleted State: "Delete crypto profile" → "Delete MFA methods"
   - PNG: `09_account_status_state.png` ✅ Aktualisiert

3. **16_2fa_enable_sequence.puml**: Umbenannt zu MFA Enable
   - Titel: "Enable **Two-Factor**..." → "Enable **Multi-Factor**..."
   - Dateiname bleibt: `16_2fa_enable_sequence.puml` (Backward Compatibility)
   - PNG: Umbenannt zu `16_mfa_enable_sequence.png` ✅ Aktualisiert
   - API Endpoints: `/api/auth/2fa/...` → `/api/auth/mfa/...`
   - Datenbank: `two_factor_enabled` → `mfa_enabled`, `mfa_methods` table
   - Recovery Codes: Jetzt Argon2id-gehasht (statt plain hashing)
   - Referenz zu Diagramm 18 für andere MFA-Methoden

### Änderungen v3.1 (2025-01-06):

#### Aktualisierte Diagramme:
4. **07_entity_relationship.puml**: MFA Datenmodell
   - **NEU**: `mfa_methods` Entity (method_type, secret_encrypted, is_primary)
   - **NEU**: `recovery_codes` Entity (code_hash, used_at)
   - **REMOVED**: `two_factor_secret`, `two_factor_enabled`, `backup_codes` aus users
   - PNG: `07_entity_relationship.png` ✅ Aktualisiert

#### Neues Diagramm v3.1:
5. **18_mfa_login_sequence.puml**: MFA Login Flow
   - TOTP Authentication Flow
   - YubiKey Challenge-Response Flow
   - Recovery Code Usage
   - Master Key Derivation mit/ohne Hardware Token
   - PNG: `18_mfa_login_sequence.png` 🆕 Neu

### Änderungen v3.0 (2024-12-19):

#### Aktualisierte Diagramme:
1. **02_encryption_layers.puml**: Erweitert um Layer 3 Display Encryption Flow
   - PNG: `02_encryption_layers.png` ✅ Gerendert
2. **03_send_message_sequence.puml**: Zeigt Layer 3 Verschleierung beim Empfänger
   - PNG: `03_send_message_sequence.png` ✅ Gerendert
3. **14_defense_in_depth.puml**: Integriert Layer 3 in Data Security Layer
   - PNG: `14_defense_in_depth.png` ✅ Gerendert

#### Neues Diagramm v3.0:
4. **17_layer3_display_encryption.puml**: Detaillierter Layer 3 Workflow (PIN-Eingabe, Auto-Obfuscation)
   - PNG: `17_layer3_display_encryption.png` 🆕 Gerendert

---

**Maintained by**:  
**Letzte Aktualisierung**: 2025-01-06 (v3.2 - MFA Terminology Update)  
**Nächster Review**: Sprint 10 (YubiKey & FIDO2 Implementation)

### Verwendung in Dokumenten (aktualisiert):

- **02_ARCHITECTURE.md**: 7 Diagramme (01, 02 ⬆️, 03 ⬆️, 04, 14 ⬆️, 15, + Referenzen)
- **03_CRYPTOGRAPHY.md**: 6 Diagramme (02 ⬆️, 04, 10, 13, 17 🆕, + Key Lifecycle)
- **04_USE_CASES.md**: 11 Diagramme (03 ⬆️, 05, 06 ⬆️, 08, 09 ⬆️, 10, 11, 12, 16 ⬆️, 17 🆕, + Referenzen)
- **05_DATA_MODEL.md**: 1 Diagramm (07 ⬆️)
- **06_MULTI_FACTOR_AUTHENTICATION.md**: 2 Diagramme (16 ⬆️, 18 🆕)
- **README.md**: 2 Diagramme (01, 18 🆕)

### Qualitätsmetriken (v3.2):

- ✅ **100% Vollständigkeit**: 18/18 Diagramme
- ✅ **MFA Terminologie**: Vereinheitlicht (2FA → MFA)
- ✅ **Layer 3 Integration**: 3 Diagramme aktualisiert, 1 neu (v3.0)
- ✅ **MFA Integration**: 4 Diagramme aktualisiert, 1 neu (v3.1-3.2)
- ✅ **PNG-Rendering**: Alle 18 PNGs erfolgreich gerendert
- ✅ **Konsistente Benennung**: Alle PNGs folgen dem Schema `{nummer}_{beschreibung}.png`
- ✅ **Korrekte Pfade**: Relative Pfade `diagrams/PNG/` in allen Dokumenten
- ✅ **Quellenangabe**: Jedes Diagramm verweist auf die `.puml` Quelle

### MFA Features (v3.1-3.2):

- 🔐 **Multi-Factor Support**: TOTP, YubiKey, FIDO2, Recovery Codes
- 🔑 **Primary & Backup Methods**: Flexible MFA-Konfiguration
- 🛡️ **Defense in Depth**: Password + MFA Token
- 📱 **Recovery Codes**: 10 Argon2id-gehashte Notfall-Codes
- 🔒 **Hardware Token**: YubiKey Challenge-Response für Master Key

### Layer 3 Features (v3.0):

- 🔒 **Anti-Shoulder-Surfing**: Nachrichten standardmäßig verschleiert (🔒 ████████)
- 🔑 **PIN-basierte Entschlüsselung**: Nur bei expliziter Anfrage sichtbar
- ⏱️ **Auto-Obfuscation**: Automatische Wiederverschleierung nach 5 Sekunden
- 🖥️ **Device-gebundene Keys**: DPAPI + User-PIN für lokale Sicherheit
- ⚙️ **Optional aktivierbar**: "Privacy Mode" in Settings

---

**Maintained by**: Project Team  
**Letzte Aktualisierung**: 2025-01-06 (v3.2 - MFA Terminology Update)  
**Nächster Review**: Sprint 10 (YubiKey & FIDO2 Implementation)


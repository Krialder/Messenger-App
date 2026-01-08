# API Reference

> **Status**: Planungsphase - API-Design, noch nicht implementiert

## 1. Base URL

```
Production:  https://api.secure-messenger.com/v1
Development: http://localhost:5000/api
```

## 2. Authentication

### 2.1 Register

Neuen Benutzer registrieren.

**Endpoint**: `POST /auth/register`

**Request**:
```json
{
  "username": "alice",
  "email": "alice@example.com",
  "password": "StrongP@ss123"
}
```

**Response** (201 Created):
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "username": "alice",
  "email": "alice@example.com",
  "masterKeySalt": "base64-encoded-32-bytes",
  "message": "Bestätigungs-E-Mail wurde gesendet"
}
```

**Validierung**:
- Username: 3-50 Zeichen, alphanumerisch + underscore
- E-Mail: gültiges Format
- Password: min. 12 Zeichen, mind. 1 Großbuchstabe, 1 Kleinbuchstabe, 1 Zahl, 1 Sonderzeichen

**Fehler**:
- `400` - Validierung fehlgeschlagen
- `409` - Username oder E-Mail bereits registriert

---

### 2.2 Login

Einloggen mit Username/E-Mail und Passwort.

**Endpoint**: `POST /auth/login`

**Request**:
```json
{
  "username": "alice",
  "password": "StrongP@ss123"
}
```

**Response ohne MFA** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000",
  "expiresIn": 900,
  "tokenType": "Bearer",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "alice",
    "email": "alice@example.com",
    "mfaEnabled": false
  }
}
```

**Response mit MFA** (200 OK):
```json
{
  "mfaRequired": true,
  "sessionToken": "temp-session-token",
  "availableMethods": [
    {
      "id": "method-uuid-1",
      "type": "totp",
      "friendlyName": "Authenticator App",
      "isPrimary": true
    },
    {
      "id": "method-uuid-2",
      "type": "yubikey",
      "friendlyName": "YubiKey Büro",
      "isPrimary": false
    }
  ]
}
```

**Fehler**:
- `401` - Ungültige Credentials
- `423` - Account gesperrt (zu viele fehlgeschlagene Versuche)

---

### 2.3 MFA Verify

MFA-Code validieren nach Login.

**Endpoint**: `POST /auth/mfa/verify`

**Headers**:
```
Authorization: Bearer {sessionToken}
```

**Request**:
```json
{
  "methodId": "method-uuid-1",
  "code": "123456"
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000",
  "expiresIn": 900,
  "tokenType": "Bearer"
}
```

**Fehler**:
- `401` - Ungültiger MFA-Code
- `429` - Zu viele Versuche (Rate Limiting)

---

### 2.4 Refresh Token

Access Token erneuern.

**Endpoint**: `POST /auth/refresh`

**Request**:
```json
{
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 900
}
```

**Fehler**:
- `401` - Ungültiger oder abgelaufener Refresh Token

---

### 2.5 Logout

Session beenden.

**Endpoint**: `POST /auth/logout`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (204 No Content)

---

## 3. MFA Management

### 3.1 Enable TOTP

TOTP (Authenticator App) aktivieren.

**Endpoint**: `POST /auth/mfa/totp/enable`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "secret": "JBSWY3DPEHPK3PXP",
  "qrCodeUrl": "data:image/png;base64,iVBORw0KGgo...",
  "backupCodes": [
    "ABCD-1234-EFGH-5678",
    "IJKL-9012-MNOP-3456",
    "..."
  ]
}
```

**Next Step**: Client zeigt QR-Code, User scannt mit App, sendet ersten Code zur Bestätigung.

---

### 3.2 Confirm TOTP

TOTP-Aktivierung bestätigen.

**Endpoint**: `POST /auth/mfa/totp/confirm`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "code": "123456"
}
```

**Response** (200 OK):
```json
{
  "message": "TOTP erfolgreich aktiviert",
  "methodId": "method-uuid"
}
```

**Fehler**:
- `400` - Ungültiger Code

---

### 3.3 Disable MFA

MFA-Methode deaktivieren.

**Endpoint**: `DELETE /auth/mfa/methods/{methodId}`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request Body**:
```json
{
  "password": "StrongP@ss123"
}
```

**Response** (204 No Content)

**Fehler**:
- `401` - Falsches Passwort
- `400` - Kann letzte MFA-Methode nicht löschen (zuerst MFA global deaktivieren)

---

## 4. Messages

### 4.1 Send Message

Nachricht senden (E2E-verschlüsselt).

**Endpoint**: `POST /messages`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "recipientId": "recipient-user-uuid",
  "encryptedContent": "base64-encoded-ciphertext",
  "nonce": "base64-encoded-nonce",
  "ephemeralPublicKey": "base64-encoded-key"
}
```

**Response** (201 Created):
```json
{
  "messageId": "message-uuid",
  "timestamp": "2025-01-06T12:34:56Z",
  "status": "sent"
}
```

**Fehler**:
- `404` - Empfänger nicht gefunden
- `403` - Empfänger hat Sender blockiert

---

### 4.2 Get Messages

Nachrichten für Konversation abrufen.

**Endpoint**: `GET /messages/{contactId}`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Query Parameters**:
- `limit` (optional): max. Anzahl (default: 50)
- `before` (optional): Nachrichten vor diesem Timestamp
- `after` (optional): Nachrichten nach diesem Timestamp

**Response** (200 OK):
```json
{
  "messages": [
    {
      "id": "message-uuid",
      "senderId": "sender-uuid",
      "recipientId": "recipient-uuid",
      "encryptedContent": "base64-encoded-ciphertext",
      "nonce": "base64-encoded-nonce",
      "ephemeralPublicKey": "base64-encoded-key",
      "timestamp": "2025-01-06T12:34:56Z",
      "status": "read"
    }
  ],
  "hasMore": true
}
```

---

### 4.3 Mark as Read

Nachricht als gelesen markieren.

**Endpoint**: `PATCH /messages/{messageId}/read`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (204 No Content)

---

### 4.4 Delete Message

Nachricht löschen (beide Seiten).

**Endpoint**: `DELETE /messages/{messageId}`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (204 No Content)

**Hinweis**: Nachricht wird für Sender und Empfänger gelöscht.

---

## 5. Contacts

### 5.1 Get Contacts

Kontaktliste abrufen.

**Endpoint**: `GET /contacts`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "contacts": [
    {
      "userId": "contact-uuid",
      "username": "bob",
      "publicKey": "base64-encoded-x25519-public-key",
      "isOnline": true,
      "lastSeen": "2025-01-06T12:30:00Z"
    }
  ]
}
```

---

### 5.2 Add Contact

Kontakt hinzufügen.

**Endpoint**: `POST /contacts`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "username": "bob"
}
```

**Response** (201 Created):
```json
{
  "contactId": "contact-uuid",
  "username": "bob",
  "publicKey": "base64-encoded-x25519-public-key"
}
```

**Fehler**:
- `404` - User nicht gefunden
- `409` - Kontakt bereits hinzugefügt

---

### 5.3 Remove Contact

Kontakt entfernen.

**Endpoint**: `DELETE /contacts/{userId}`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (204 No Content)

---

## 6. Keys

### 6.1 Get Public Key

Public Key eines Users abrufen.

**Endpoint**: `GET /keys/public/{userId}`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "userId": "user-uuid",
  "publicKey": "base64-encoded-x25519-public-key",
  "createdAt": "2025-01-01T00:00:00Z",
  "expiresAt": "2026-01-01T00:00:00Z"
}
```

---

### 6.2 Rotate Key

Eigenen Public Key rotieren (Client generiert neues Keypair).

**Endpoint**: `POST /keys/rotate`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "newPublicKey": "base64-encoded-x25519-public-key"
}
```

**Response** (200 OK):
```json
{
  "message": "Key rotiert",
  "keyId": "new-key-uuid"
}
```

---

## 7. User Profile

### 7.1 Get Profile

Eigenes Profil abrufen.

**Endpoint**: `GET /users/me`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "id": "user-uuid",
  "username": "alice",
  "email": "alice@example.com",
  "createdAt": "2025-01-01T00:00:00Z",
  "mfaEnabled": true,
  "emailVerified": true
}
```

---

### 7.2 Update Profile

Profil aktualisieren.

**Endpoint**: `PATCH /users/me`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "email": "newemail@example.com"
}
```

**Response** (200 OK):
```json
{
  "message": "Profil aktualisiert",
  "emailVerificationRequired": true
}
```

---

### 7.3 Delete Account

Account löschen (30-Tage-Frist).

**Endpoint**: `DELETE /users/me`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "password": "StrongP@ss123",
  "confirmDelete": true
}
```

**Response** (200 OK):
```json
{
  "message": "Account zur Löschung vorgemerkt",
  "deletionDate": "2025-02-05T00:00:00Z",
  "cancellationDeadline": "2025-02-05T00:00:00Z"
}
```

---

### 7.4 Export Data (DSGVO)

Alle persönlichen Daten exportieren.

**Endpoint**: `POST /users/me/export`

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (202 Accepted):
```json
{
  "message": "Export wird vorbereitet",
  "taskId": "export-task-uuid"
}
```

**Status abrufen**: `GET /users/me/export/{taskId}`

**Response wenn fertig** (200 OK):
```json
{
  "status": "completed",
  "downloadUrl": "https://cdn.../export-alice-2025-01-06.zip",
  "expiresAt": "2025-01-13T00:00:00Z"
}
```

---

## 8. Error Responses

### Standard Error Format

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "email",
        "message": "Invalid email format"
      }
    ]
  }
}
```

### HTTP Status Codes

| Code | Bedeutung |
|------|-----------|
| `200` | OK |
| `201` | Created |
| `204` | No Content |
| `400` | Bad Request (Validierung fehlgeschlagen) |
| `401` | Unauthorized (Auth fehlgeschlagen) |
| `403` | Forbidden (Nicht berechtigt) |
| `404` | Not Found |
| `409` | Conflict (Ressource existiert bereits) |
| `423` | Locked (Account gesperrt) |
| `429` | Too Many Requests (Rate Limiting) |
| `500` | Internal Server Error |

---

## 9. Rate Limiting

**Limits pro IP**:
- Login: 5 Versuche / 15 Minuten
- MFA Verify: 5 Versuche / 15 Minuten
- Message Send: 100 / Minute
- API Calls (gesamt): 1000 / Stunde

**Header**:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1609459200
```

**Fehler bei Limit**:
```json
{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests",
    "retryAfter": 300
  }
}
```

---

## 10. Real-time Events (SignalR)

### Connection

**Hub URL**: `wss://api.secure-messenger.com/hubs/notifications`

**Auth**: Query-String-Token
```
wss://.../hubs/notifications?access_token={accessToken}
```

### Events

**MessageReceived**:
```json
{
  "messageId": "message-uuid",
  "senderId": "sender-uuid",
  "timestamp": "2025-01-06T12:34:56Z"
}
```

**MessageRead**:
```json
{
  "messageId": "message-uuid",
  "readBy": "reader-uuid",
  "readAt": "2025-01-06T12:35:00Z"
}
```

**UserOnline**:
```json
{
  "userId": "user-uuid",
  "status": "online"
}
```

**UserTyping**:
```json
{
  "userId": "user-uuid",
  "conversationId": "conversation-uuid"
}
```

---

**Dokument-Version**: 1.0  
**Letzte Aktualisierung**: 2025-01-06  
**Status**: Planungsphase - API-Design

# Rate Limiting Configuration

**Updated**: 2026-01-20  
**Version**: 12.0

---

## 🚀 Swagger UI Quick Start

### ✅ Schritt 1: Registrierung

**Endpoint**: `POST /api/auth/register`

```json
{
  "username": "alice2026",
  "email": "alice2026@example.com",
  "password": "SecurePass123!"
}
```

**Passwort-Anforderungen**:
- Mindestens 8 Zeichen
- 1 Großbuchstabe, 1 Kleinbuchstabe, 1 Ziffer, 1 Sonderzeichen

**Gültige Passwörter**: `SecurePass123!`, `MyP@ssw0rd`, `Test1234!@#`

---

### ✅ Schritt 2: Login

**Endpoint**: `POST /api/auth/login`

```json
{
  "email": "alice2026@example.com",
  "password": "SecurePass123!"
}
```

**Response**: Kopiere `accessToken` aus Response

---

### ✅ Schritt 3: Authentifizierung

1. Klicke **"Authorize"** 🔒
2. Eingabe: `Bearer <dein-token>`
3. Klicke **"Authorize"** → **"Close"**

---

### ✅ Schritt 4: MFA aktivieren (Optional)

**TOTP aktivieren**: `POST /api/mfa/enable-totp`  
**TOTP bestätigen**: `POST /api/mfa/verify-totp-setup` mit 6-stelligem Code aus Authenticator-App

---

## 📊 Rate Limits

### Development (appsettings.json)

| Endpoint | Limit | Period |
|----------|-------|--------|
| POST /api/auth/login | 100 | 15m |
| POST /api/auth/register | 100 | 1h |
| POST /api/auth/verify-mfa | 50 | 15m |
| POST /api/mfa/enable-totp | 50 | 1h |
| POST /api/mfa/generate-recovery-codes | 20 | 1h |

### Production (appsettings.Production.json)

| Endpoint | Limit | Period | Security |
|----------|-------|--------|----------|
| POST /api/auth/login | 20 | 15m | 🟢 High |
| POST /api/auth/register | 50 | 1h | 🟡 Medium |
| POST /api/auth/verify-mfa | 20 | 15m | 🟢 High |
| POST /api/mfa/enable-totp | 10 | 1h | 🟢 High |
| POST /api/mfa/generate-recovery-codes | 5 | 1h | 🔴 Critical |

---

## 🐛 Troubleshooting

### HTTP 400 - Validierungsfehler
**Ursache**: Platzhalter `"string"` nicht ersetzt  
**Lösung**: Echte Werte verwenden (siehe Beispiele oben)

### HTTP 401 - Unauthorized
**Ursache**: Kein Token  
**Lösung**: Login → Token kopieren → "Authorize"

### HTTP 500 - Internal Server Error
**Ursache**: Benutzer existiert nicht  
**Lösung**: Zuerst registrieren, dann einloggen

### HTTP 429 - Too Many Requests
**Ursache**: Rate-Limit erreicht  
**Lösung**: Warten (siehe `Retry-After` Header) oder Redis leeren:
```powershell
docker exec messenger_redis redis-cli -a "7PjIbsl21UpGkEV06QRH8aY5ytW3wfov" FLUSHDB
```

---

## 🔧 Konfiguration

### Rate Limiting deaktivieren (nur Development!)

**appsettings.json**:
```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": false
  }
}
```

⚠️ **Nach Tests wieder aktivieren!**

---

### Limits anpassen

**appsettings.Production.json**:
```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "POST:/api/auth/login",
        "Period": "15m",
        "Limit": 20
      }
    ]
  }
}
```

---

## 📈 Monitoring

### Rate-Limit-Headers prüfen

```powershell
$response = Invoke-WebRequest -Uri "http://localhost:5001/api/auth/login" `
    -Method POST -ContentType "application/json" `
    -Body '{"email":"test@example.com","password":"Test123!"}'

Write-Host "Limit: $($response.Headers['X-Rate-Limit-Limit'][0])"
Write-Host "Remaining: $($response.Headers['X-Rate-Limit-Remaining'][0])"
```

### Redis überwachen

```powershell
# Alle Rate-Limit-Keys anzeigen
docker exec messenger_redis redis-cli -a "7PjIbsl21UpGkEV06QRH8aY5ytW3wfov" KEYS "*rate_limit*"

# Spezifische IP prüfen
docker exec messenger_redis redis-cli -a "7PjIbsl21UpGkEV06QRH8aY5ytW3wfov" GET "rate_limit:POST:/api/auth/login:192.168.1.100"
```

---

## 🧪 Testing

### Quick Test - PowerShell

```powershell
for ($i = 1; $i -le 5; $i++) {
    $body = @{
        username = "testuser$i"
        email = "testuser$i@example.com"
        password = "SecurePass123!"
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod `
            -Uri "http://localhost:5001/api/auth/register" `
            -Method POST -ContentType "application/json" -Body $body
        
        Write-Host "[✓] User $i registered" -ForegroundColor Green
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 429) {
            Write-Host "[✗] User $i - Rate Limited" -ForegroundColor Red
        } else {
            Write-Host "[✗] User $i - Error: $statusCode" -ForegroundColor Yellow
        }
    }
}
```

### Health Check testen

```powershell
# Option 1 (Empfohlen)
Invoke-RestMethod -Uri http://localhost:5001/health

# Option 2
curl -UseBasicParsing http://localhost:5001/health

# Erwartet: Healthy
```

---

## 📚 Resources

- **AspNetCoreRateLimit**: https://github.com/stefanprodan/AspNetCoreRateLimit
- **OWASP Rate Limiting**: https://cheatsheetseries.owasp.org/cheatsheets/Denial_of_Service_Cheat_Sheet.html
- **JWT Debugging**: https://jwt.io

---

**Version**: 12.0  
**Last Updated**: 2026-01-20  
**Status**: ✅ Production Ready

# Rate Limiting Configuration

**Updated**: 2025-01-16  
**Version**: 10.2

---

## 📊 Current Limits

### **Development Environment**

| Endpoint | Limit | Period | Purpose |
|----------|-------|--------|---------|
| **POST /api/auth/login** | 100 | 15 minutes | Allow extensive testing |
| **POST /api/auth/register** | 100 | 1 hour | Enable integration tests |
| **POST /api/auth/verify-mfa** | 50 | 15 minutes | MFA testing |
| **POST /api/mfa/verify-totp-setup** | 20 | 15 minutes | TOTP setup testing |

**Configuration**: `src/Backend/AuthService/appsettings.json`

---

### **Production Environment**

| Endpoint | Limit | Period | Security Level |
|----------|-------|--------|----------------|
| **POST /api/auth/login** | 20 | 15 minutes | 🟢 High (prevents brute-force) |
| **POST /api/auth/register** | 50 | 1 hour | 🟡 Medium (allows new users) |
| **POST /api/auth/verify-mfa** | 20 | 15 minutes | 🟢 High (prevents MFA bypass) |
| **POST /api/mfa/verify-totp-setup** | 10 | 15 minutes | 🟢 High (prevents TOTP abuse) |

**Configuration**: `src/Backend/AuthService/appsettings.Production.json`

---

## 🔒 Security Rationale

### **Why these limits?**

#### **Development (100/hour)**
- ✅ Allows running full test suites
- ✅ Enables rapid prototyping
- ✅ No interference with local development
- ⚠️ **DO NOT use in production!**

#### **Production (50/hour registration, 20/15min login)**
- ✅ Prevents brute-force attacks (20 login attempts / 15 min)
- ✅ Allows legitimate user signups (50 / hour)
- ✅ Stops automated bot registrations
- ✅ Maintains good UX (users rarely hit limits)

---

## 🧪 Testing Rate Limits

### **Test Script (PowerShell)**

```powershell
# Test registration limit
for ($i = 1; $i -le 10; $i++) {
    $body = @{
        username = "testuser$i"
        email = "test$i@example.com"
        password = "Test123!"
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5001/api/auth/register" `
            -Method POST `
            -ContentType "application/json" `
            -Body $body
        
        Write-Host "[OK] Registration $i successful" -ForegroundColor Green
    } catch {
        Write-Host "[BLOCKED] Registration $i blocked - Rate limit reached" -ForegroundColor Red
    }
}
```

**Expected**: First 100 succeed (dev), then HTTP 429 Too Many Requests

---

## 🔧 Customizing Limits

### **Change Limits**

Edit `appsettings.Production.json`:

```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "POST:/api/auth/register",
        "Period": "1h",
        "Limit": 100    // Adjust this value
      }
    ]
  }
}
```

### **Bypass Rate Limiting (Testing Only)**

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": false  // Disable all limits
  }
}
```

**⚠️ WARNING**: Only for local testing! Never disable in production!

---

## 📈 Monitoring Rate Limits

### **Check Redis for Rate Limit Data**

```powershell
# Connect to Redis
docker exec -it messenger_redis redis-cli -a "YOUR_REDIS_PASSWORD"

# View all rate limit keys
KEYS *rate_limit*

# Check specific IP
GET rate_limit:POST:/api/auth/register:192.168.1.100

# Clear rate limits (for testing)
FLUSHALL
```

---

## 🚨 Rate Limit Responses

### **HTTP 429 Too Many Requests**

**Response Body**:
```json
{
  "statusCode": 429,
  "message": "Rate limit exceeded. Try again in 15 minutes."
}
```

**Headers**:
```
X-Rate-Limit-Limit: 20
X-Rate-Limit-Remaining: 0
X-Rate-Limit-Reset: 1642345678
Retry-After: 900
```

---

## 🔄 Rate Limit Reset

### **Automatic Reset**
- Limits reset automatically after the specified period
- Example: Login limit (20/15min) resets every 15 minutes

### **Manual Reset (Admin)**

```powershell
# Clear rate limits for specific IP
docker exec -it messenger_redis redis-cli -a "PASSWORD" DEL "rate_limit:POST:/api/auth/login:192.168.1.100"

# Clear all rate limits
docker exec -it messenger_redis redis-cli -a "PASSWORD" FLUSHDB
```

---

## 📚 Additional Resources

- **AspNetCoreRateLimit Docs**: https://github.com/stefanprodan/AspNetCoreRateLimit
- **OWASP Rate Limiting**: https://cheatsheetseries.owasp.org/cheatsheets/Denial_of_Service_Cheat_Sheet.html
- **Redis Persistence**: [docs/REDIS_CONFIGURATION.md](docs/REDIS_CONFIGURATION.md)

---

**Version**: 10.2  
**Last Updated**: 2025-01-16  
**Status**: ✅ Optimized for Development & Production

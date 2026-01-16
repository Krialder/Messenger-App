# 🚀 FINALE DEPLOYMENT-ANLEITUNG
# Windows 11 Production Deployment - Schritt-für-Schritt

**Datum**: 2025-01-16  
**Status**: READY TO DEPLOY ✅

---

## ✅ PRE-DEPLOYMENT CHECKLIST

Führe diese Schritte **VOR** dem Deployment aus:

### **1. Validierung ausführen**

```cmd
validate-deployment.bat
```

**Erwartete Ausgabe**:
```
================================================
  ALL FILES PRESENT - READY TO DEPLOY!
================================================
```

---

### **2. Secrets generieren**

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows\generate-secrets.ps1
```

**Was passiert**:
- ✅ Erstellt `.env.production` aus Template
- ✅ Generiert alle Secrets automatisch
- ✅ Zeigt deine LAN-IP an
- ✅ Validiert Konfiguration

**Erwartete Ausgabe**:
```
╔══════════════════════════════════════════╗
║   Secure Messenger - Secret Generator   ║
╚══════════════════════════════════════════╝

✅ .env.production erstellt
✅ JWT_SECRET gesetzt
✅ TOTP_ENCRYPTION_KEY gesetzt
✅ POSTGRES_PASSWORD gesetzt
✅ REDIS_PASSWORD gesetzt
✅ RABBITMQ_PASSWORD gesetzt

ℹ️  LAN-IP gefunden: 192.168.1.100
```

**Notiere dir die angezeigte LAN-IP!**

---

### **3. Nginx LAN-IP konfigurieren**

```cmd
notepad nginx\nginx-lan.conf
```

**Ändere Zeile 41**:
```nginx
# VORHER:
server_name 192.168.1.100 localhost;

# NACHHER (mit DEINER IP):
server_name 192.168.1.XXX localhost;
```

**Speichern & Schließen**

---

### **4. Windows Firewall konfigurieren** (PowerShell als Admin)

```powershell
# PowerShell als Administrator starten

# Port 80 für LAN öffnen
New-NetFirewallRule -DisplayName "Messenger HTTP (LAN)" `
    -Direction Inbound `
    -LocalPort 80 `
    -Protocol TCP `
    -Action Allow `
    -Profile Private

# Prüfen
Get-NetFirewallRule -DisplayName "Messenger*"
```

---

## 🚀 DEPLOYMENT DURCHFÜHREN

### **Option A: Einfacher Start (Empfohlen)** ⭐

```cmd
deploy.bat -SkipSSL
```

**Das startet automatisch**:
1. ✅ Prerequisites Check (Docker, Compose)
2. ✅ Secrets Validation
3. ✅ SSL Check (wird übersprungen mit -SkipSSL)
4. ✅ Docker Images Build (10-20 Minuten)
5. ✅ Services Start
6. ✅ Health Checks (120 Sekunden Timeout)
7. ✅ Summary anzeigen

---

### **Option B: Manuell mit PowerShell**

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows\Deploy-Production.ps1 -SkipSSL
```

**Parameter-Optionen**:
- `-SkipSSL` - Überspringt SSL-Check (für LAN)
- `-SkipHealthCheck` - Überspringt Health-Check (schneller)
- `-Force` - Überspringt Admin-Check

---

## 📊 ERWARTETE AUSGABE

### **Während des Deployments**:

```
╔══════════════════════════════════════════╗
║   Secure Messenger - Production Deploy  ║
╚══════════════════════════════════════════╝

[1/7] Checking prerequisites...
✅ Docker installed: Docker version 24.0.7
✅ Docker Compose installed: Docker Compose version v2.23.3
✅ .env.production found

[2/7] Validating secrets...
✅ All secrets configured

[3/7] Checking SSL certificates...
ℹ️  Skipping SSL check (-SkipSSL flag)
ℹ️  Deployment Type: LAN (HTTP)

[4/7] Building Docker images...
[+] Building 456.2s (89/89) FINISHED
✅ Images built successfully

[5/7] Stopping old containers...
✅ Old containers stopped

[6/7] Starting production services...
[+] Running 12/12
 ✔ Network messenger_network        Created
 ✔ Container messenger_postgres     Started
 ✔ Container messenger_redis        Started
 ✔ Container messenger_rabbitmq     Started
 ✔ Container messenger_auth_service Started
 ✔ Container messenger_gateway      Started
 ✔ Container messenger_nginx_lan    Started
 ...
✅ Services started

ℹ️  Waiting for services to be healthy (max 120 seconds)...
✅ All services are healthy!

[7/7] Running health checks...
ℹ️  Testing health endpoint: http://192.168.1.100/health
✅ Health check passed (HTTP 200)

╔══════════════════════════════════════════╗
║         Deployment Complete!             ║
╚══════════════════════════════════════════╝

✅ Deployment successful! 🚀
```

---

## ✅ POST-DEPLOYMENT VALIDATION

### **1. Container Status prüfen**

```cmd
docker compose ps
```

**Erwartete Ausgabe**: Alle Container "healthy" oder "running"

```
NAME                          STATUS
messenger_postgres            Up (healthy)
messenger_redis               Up (healthy)
messenger_rabbitmq            Up (healthy)
messenger_auth_service        Up (healthy)
messenger_message_service     Up (healthy)
messenger_user_service        Up (healthy)
messenger_crypto_service      Up (healthy)
messenger_key_service         Up (healthy)
messenger_notification_service Up (healthy)
messenger_file_service        Up (healthy)
messenger_audit_service       Up (healthy)
messenger_nginx_lan           Up (healthy)
messenger_gateway             Up (healthy)
```

---

### **2. Health Endpoint testen**

```powershell
# Ersetze 192.168.1.100 mit DEINER LAN-IP
$lanIP = "192.168.1.100"
Invoke-RestMethod -Uri "http://$lanIP/health"
```

**Erwartete Antwort**:
```
status
------
Healthy
```

---

### **3. User Registration testen**

```powershell
$lanIP = "192.168.1.100"

$registerBody = @{
    username = "testuser"
    email = "test@firma.local"
    password = "SecurePass123!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://$lanIP/api/auth/register" `
    -Method POST `
    -ContentType "application/json" `
    -Body $registerBody
```

**Erwartete Antwort**: HTTP 201 Created mit User-ID

---

### **4. Logs prüfen**

```cmd
docker compose logs -f auth-service
```

**STRG+C zum Beenden**

---

## 🔧 TROUBLESHOOTING

### **Problem: Docker Desktop nicht gestartet**

```cmd
# Docker Desktop manuell starten
start "" "C:\Program Files\Docker\Docker\Docker Desktop.exe"

# 30 Sekunden warten
timeout /t 30

# Nochmal versuchen
deploy.bat -SkipSSL
```

---

### **Problem: Port 80 blockiert**

```powershell
# Prüfen was Port 80 verwendet
Get-Process -Id (Get-NetTCPConnection -LocalPort 80).OwningProcess

# Falls IIS: Stoppen
Stop-Service -Name W3SVC
```

---

### **Problem: Services nicht healthy**

```cmd
# Logs aller Services anzeigen
docker compose logs

# Einzelner Service
docker compose logs auth-service

# Service neu starten
docker compose restart auth-service
```

---

## 📱 CLIENT DEPLOYMENT

### **1. Client bauen**

```cmd
.\build-client.bat
```

**Output**: `publish\MessengerClient\MessengerClient.exe`

---

### **2. Client-Konfiguration**

Bearbeite `src/Frontend/MessengerClient/App.xaml.cs`:

```csharp
// Zeile ~40-60 (ConfigureServices)
services.AddRefitClient<IAuthApiService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://192.168.1.100"));
    //                                                  ^^^^^^^^^^^^^^^^^^^^
    //                                                  DEINE LAN-IP

// Wiederhole für alle 5 API Services
```

**Neu bauen**:
```cmd
.\build-client.bat
```

---

### **3. Client verteilen (Netzwerk-Share)**

```powershell
# Als Administrator
New-SmbShare -Name "MessengerClient" `
    -Path "C:\Server\Messenger\publish\MessengerClient" `
    -FullAccess "DOMAIN\IT-Admins" `
    -ReadAccess "DOMAIN\Users"
```

**Clients zugreifen via**: `\\SERVER-NAME\MessengerClient\MessengerClient.exe`

---

## 🔄 BACKUP EINRICHTEN

### **Manuelles Backup testen**

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows\backup-database.ps1
```

---

### **Task Scheduler (täglich 02:00 Uhr)**

```powershell
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-ExecutionPolicy Bypass -File C:\Server\Messenger\scripts\windows\backup-database.ps1"

$trigger = New-ScheduledTaskTrigger -Daily -At 2:00AM

$principal = New-ScheduledTaskPrincipal -UserID "NT AUTHORITY\SYSTEM" `
    -LogonType ServiceAccount -RunLevel Highest

Register-ScheduledTask -TaskName "Messenger_Daily_Backup" `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal
```

**Prüfen**:
```powershell
Get-ScheduledTask -TaskName "Messenger_Daily_Backup"
```

---

## ✅ FINALE CHECKLISTE

- [ ] **Validierung** erfolgreich (`validate-deployment.bat`)
- [ ] **Secrets** generiert (`.env.production` vorhanden)
- [ ] **nginx-lan.conf** LAN-IP aktualisiert
- [ ] **Firewall** Port 80 geöffnet
- [ ] **Deployment** gestartet (`deploy.bat -SkipSSL`)
- [ ] **Container** alle healthy (`docker compose ps`)
- [ ] **Health Check** erfolgreich (HTTP 200)
- [ ] **User Registration** getestet
- [ ] **Client** gebaut und konfiguriert
- [ ] **Backup** eingerichtet (Task Scheduler)

---

## 📞 SUPPORT

Bei Problemen:

1. **Logs prüfen**: `docker compose logs -f`
2. **Dokumentation**: [docs/WINDOWS_DEPLOYMENT.md](docs/WINDOWS_DEPLOYMENT.md)
3. **Script Guide**: [scripts/windows/README.md](scripts/windows/README.md)
4. **GitHub Issues**: https://github.com/Krialder/Messenger-App/issues

---

**Status**: ✅ **BEREIT ZUM DEPLOYMENT**

**Version**: 1.0 Windows Production  
**Deployment-Zeit**: ~2-3 Stunden  
**Target**: 60-1000 Benutzer

---

## 🎯 QUICK START (Zusammenfassung)

```cmd
REM 1. Validierung
validate-deployment.bat

REM 2. Secrets generieren
powershell -ExecutionPolicy Bypass -File scripts\windows\generate-secrets.ps1

REM 3. Nginx konfigurieren (LAN-IP eintragen)
notepad nginx\nginx-lan.conf

REM 4. Deployment starten
deploy.bat -SkipSSL

REM 5. Testen
docker compose ps
```

**Das war's!** 🚀

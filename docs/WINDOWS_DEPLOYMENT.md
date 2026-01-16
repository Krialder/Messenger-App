# 🚀 Windows 11 Server Deployment Guide - Secure Messenger

**Version**: 1.0 Windows Edition  
**Status**: Production Ready (LAN) + Skalierbar (WAN)  
**Zielgruppe**: Windows 11 Pro Server - Firmeninternes Netzwerk  
**Benutzer**: 60 Benutzer (aktuell) → 1000 Benutzer (skaliert)  
**Last Updated**: 2025-01-16

---

## 📋 **Voraussetzungen**

### **Server Hardware**

| Ressource | **Aktuell (60 User)** | **Skaliert (1000 User)** |
|-----------|----------------------|--------------------------|
| **CPU** | 4 Cores | 8+ Cores |
| **RAM** | 16 GB | 32+ GB |
| **Storage** | 100 GB SSD | 500+ GB SSD (NVMe) |
| **OS** | Windows 11 Pro ✅ | Windows Server 2022 |
| **Network** | 1 Gbit/s LAN | 1 Gbit/s LAN + WAN |

### **Software Requirements**

- ✅ **Windows 11 Pro** (Home nicht unterstützt!)
- ✅ **Docker Desktop** (Version 24.x oder neuer)
- ✅ **WSL2** (Windows Subsystem for Linux 2)
- ✅ **PowerShell 7.x** oder höher
- ✅ **Git for Windows**

### **Ports (Windows Firewall)**

| Port | Protokoll | Zweck | **Phase 1 (LAN)** | **Phase 2 (WAN)** |
|------|-----------|-------|-------------------|-------------------|
| **80** | TCP | HTTP | ✅ Intern | ✅ Extern |
| **443** | TCP | HTTPS | ❌ | ✅ Extern |
| **3389** | TCP | RDP (Management) | ✅ Intern | ❌ Blockiert |

---

## 🎯 **Deployment-Phasen**

### **Phase 1: Internes LAN** (JETZT)
- 🏢 Nur firmenintern erreichbar
- 🔐 HTTP (kein SSL)
- 📱 60 Benutzer
- 💰 Kosten: $0

### **Phase 2: Öffentliches WAN** (SPÄTER)
- 🌐 Extern via Dynamic DNS
- 🔐 HTTPS mit Firmenzertifikat
- 📱 1000 Benutzer
- 💰 Firmenzertifikat (~300-500€/Jahr)

---

## 🔧 **PHASE 1: LAN DEPLOYMENT**

---

### **Schritt 1: WSL2 + Docker Desktop Installation**

#### **1.1 WSL2 aktivieren**

```powershell
# PowerShell als Administrator ausführen

# WSL2 installieren
wsl --install

# Neustart erforderlich!
Restart-Computer
```

**Nach Neustart**:

```powershell
# WSL2 Version prüfen
wsl --status

# Erwartete Ausgabe:
# Standardversion: 2
```

#### **1.2 Docker Desktop installieren**

**Option A: Winget**

```powershell
# PowerShell als Administrator
winget install Docker.DockerDesktop
```

**Option B: Manueller Download**

1. Download: https://www.docker.com/products/docker-desktop/
2. Installer ausführen
3. ✅ "Use WSL 2 instead of Hyper-V" ankreuzen
4. Installation abschließen
5. **Neustart**

#### **1.3 Docker Desktop konfigurieren**

```powershell
# Docker Desktop starten (GUI)

# Settings → Resources → WSL Integration
# ✅ "Enable integration with my default WSL distro"

# Docker testen
docker --version
# Erwartete Ausgabe: Docker version 24.x.x

docker compose version
# Erwartete Ausgabe: Docker Compose version v2.x.x
```

---

### **Schritt 2: Projekt Setup**

#### **2.1 Repository klonen**

```powershell
# Projekt-Verzeichnis erstellen
New-Item -ItemType Directory -Path "C:\Server\Messenger" -Force
cd C:\Server\Messenger

# Repository klonen
git clone https://github.com/Krialder/Messenger-App.git .

# Branch prüfen
git branch
# Sollte "master" zeigen
```

#### **2.2 LAN-IP ermitteln**

```powershell
# Server-IP im LAN ermitteln
Get-NetIPAddress -AddressFamily IPv4 | 
    Where-Object {$_.IPAddress -like "192.168.*" -or $_.IPAddress -like "10.*"}

# Beispiel-Ausgabe:
# IPAddress: 192.168.1.100
```

**⚠️ Notiere dir die IP!** Beispiel: `192.168.1.100`

---

### **Schritt 3: Secrets Generieren**

#### **3.1 Automatisches PowerShell Script** ⭐

```powershell
cd C:\Server\Messenger

# Secret-Generator ausführen
powershell -ExecutionPolicy Bypass -File scripts\windows\generate-secrets.ps1

# Das Script:
# ✅ Erstellt .env.production
# ✅ Generiert alle Secrets automatisch
# ✅ Validiert Konfiguration
# ✅ Zeigt LAN-IP an
```

**Ausgabe**:

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

✅ Keine Platzhalter mehr vorhanden ✓

ℹ️  LAN-IP gefunden: 192.168.1.100
⚠️  WICHTIG: Trage diese IP in nginx/nginx-lan.conf ein!

╔══════════════════════════════════════════╗
║        Secrets erfolgreich erstellt!     ║
╚══════════════════════════════════════════╝

Backup erstellen? (J/n):
```

#### **3.2 Secrets validieren**

```powershell
# Prüfen ob noch Platzhalter vorhanden
Select-String -Path .env.production -Pattern "CHANGE_THIS"

# Erwartete Ausgabe: NICHTS!
```

---

### **Schritt 4: Nginx LAN-Konfiguration anpassen**

```powershell
# nginx-lan.conf bearbeiten
notepad nginx\nginx-lan.conf

# Zeile 41 anpassen:
# server_name 192.168.1.100 localhost;
#             ^^^^^^^^^^^^^^^
#             DEINE LAN-IP HIER
```

**Wichtig**: Ersetze `192.168.1.100` mit deiner **tatsächlichen LAN-IP**!

---

### **Schritt 5: Windows Firewall Konfiguration**

```powershell
# PowerShell als Administrator

# Port 80 (HTTP) für LAN öffnen
New-NetFirewallRule -DisplayName "Messenger HTTP (LAN)" `
    -Direction Inbound `
    -LocalPort 80 `
    -Protocol TCP `
    -Action Allow `
    -Profile Private

# Firewall Status prüfen
Get-NetFirewallRule -DisplayName "Messenger*" | Format-Table

# Erwartete Ausgabe:
# DisplayName              Enabled Direction
# -----------              ------- ---------
# Messenger HTTP (LAN)     True    Inbound
```

---

### **Schritt 6: Docker Deployment**

#### **6.1 Docker Images bauen**

```powershell
cd C:\Server\Messenger

# Production Build mit LAN-Override
docker compose -f docker-compose.yml -f docker-compose.lan.yml build --parallel

# Dauer: ~10-20 Minuten (erstmaliger Build)
```

**Erwartete Ausgabe**:

```
[+] Building 15.2s (89/89) FINISHED
 => [auth-service] ...
 => [message-service] ...
 => [gateway-service] ...
 => [nginx] ...
Successfully built 9 images
```

#### **6.2 Services starten**

```powershell
# Detached Mode (Hintergrund)
docker compose -f docker-compose.yml -f docker-compose.lan.yml up -d

# Logs live verfolgen (optional)
docker compose logs -f
```

**Erwartete Ausgabe**:

```
[+] Running 12/12
 ✔ Network messenger_network        Created
 ✔ Container messenger_postgres     Started
 ✔ Container messenger_redis        Started
 ✔ Container messenger_rabbitmq     Started
 ✔ Container messenger_auth_service Started
 ✔ Container messenger_gateway      Started
 ✔ Container messenger_nginx_lan    Started
 ...
```

#### **6.3 Health Checks**

```powershell
# Container Status prüfen
docker compose ps

# Erwartete Ausgabe: Alle "healthy" oder "running"

# API Health Check (LAN-IP verwenden)
$lanIP = "192.168.1.100"  # DEINE IP
Invoke-RestMethod -Uri "http://$lanIP/health" -Method Get

# Erwartete Antwort:
# status
# ------
# Healthy
```

---

### **Schritt 7: Client-Konfiguration**

#### **7.1 Frontend Build**

```powershell
cd C:\Server\Messenger

# Client bauen
.\build-client.bat

# Output: publish\MessengerClient\MessengerClient.exe
```

#### **7.2 Client-Konfiguration**

**Option A: Code anpassen** (empfohlen)

Bearbeite `src/Frontend/MessengerClient/App.xaml.cs`:

```csharp
// Zeile ~40-60 (im ConfigureServices)
services.AddRefitClient<IAuthApiService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://192.168.1.100"));
    //                                                  ^^^^^^^^^^^^^^^^^^^^
    //                                                  DEINE LAN-IP

services.AddRefitClient<IMessageApiService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://192.168.1.100"));

// Wiederhole für alle 5 API Services
```

**Option B: Konfigurationsdatei** (für einfache Updates)

Erstelle `src/Frontend/MessengerClient/appsettings.json`:

```json
{
  "ApiBaseUrl": "http://192.168.1.100"
}
```

Dann in `App.xaml.cs` auslesen:

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var apiUrl = config["ApiBaseUrl"];

services.AddRefitClient<IAuthApiService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiUrl));
```

#### **7.3 Client verteilen**

**Via Netzwerk-Share**:

```powershell
# Netzwerk-Share erstellen
New-SmbShare -Name "MessengerClient" `
    -Path "C:\Server\Messenger\publish\MessengerClient" `
    -FullAccess "DOMAIN\IT-Admins" `
    -ReadAccess "DOMAIN\Users"

# Clients können nun zugreifen via:
# \\SERVER-NAME\MessengerClient\MessengerClient.exe
```

**Via Group Policy (empfohlen für große Firmen)**:

1. Exe nach `\\domain\NETLOGON\Apps\Messenger\` kopieren
2. GPO erstellen: "Software Installation"
3. Package: `MessengerClient.exe`
4. Deployment Type: "Assigned"

---

### **Schritt 8: Verifikation & Testing**

#### **8.1 Server-Tests**

```powershell
$lanIP = "192.168.1.100"  # DEINE IP

# Health Check
$health = Invoke-RestMethod -Uri "http://$lanIP/health"
Write-Host "Health: $($health.status)" -ForegroundColor Green

# User Registration Test
$registerBody = @{
    username = "testuser"
    email = "test@firma.local"
    password = "SecurePass123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://$lanIP/api/auth/register" `
    -Method POST `
    -ContentType "application/json" `
    -Body $registerBody

Write-Host "User created: $($response.userId)" -ForegroundColor Green

# Login Test
$loginBody = @{
    email = "test@firma.local"
    password = "SecurePass123!"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "http://$lanIP/api/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $loginBody

Write-Host "Access Token: $($loginResponse.accessToken.Substring(0,20))..." -ForegroundColor Green
```

#### **8.2 Client-Test**

1. Starte: `\\SERVER-NAME\MessengerClient\MessengerClient.exe`
2. **Registrieren**: Neuer Benutzer anlegen
3. **Login**: Mit neuem Benutzer anmelden
4. **Nachricht senden**: An anderen Benutzer testen

---

### **Schritt 9: Monitoring & Logs**

#### **9.1 Docker Logs**

```powershell
# Alle Logs
docker compose logs -f

# Spezifischer Service
docker compose logs -f auth-service

# Logs exportieren
docker compose logs > C:\Server\Messenger\logs\all-services.log
```

#### **9.2 Resource Monitoring**

```powershell
# Container Stats (Live)
docker stats

# Disk Usage
docker system df

# Windows Task Manager: Docker Desktop Ressourcen
```

---

### **Schritt 10: Backup & Recovery**

#### **10.1 Datenbank Backup**

Verwende `scripts/windows/backup-database.ps1`:

```powershell
# Backup-Script ausführen
powershell -ExecutionPolicy Bypass -File scripts\windows\backup-database.ps1

# Output: C:\Server\Messenger\backups\postgres_20250116_020000.zip
```

#### **10.2 Automatisches Backup (Task Scheduler)**

```powershell
# Task Scheduler Job erstellen
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-ExecutionPolicy Bypass -File C:\Server\Messenger\scripts\windows\backup-database.ps1"

$trigger = New-ScheduledTaskTrigger -Daily -At 2:00AM

$principal = New-ScheduledTaskPrincipal -UserID "NT AUTHORITY\SYSTEM" `
    -LogonType ServiceAccount -RunLevel Highest

Register-ScheduledTask -TaskName "Messenger_Daily_Backup" `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Description "Tägliches Backup der Messenger-Datenbank"

Write-Host "✅ Backup-Task erstellt (täglich 02:00 Uhr)" -ForegroundColor Green
```

#### **10.3 Restore**

```powershell
# Backup wiederherstellen
$BACKUP_FILE = "C:\Server\Messenger\backups\postgres_20250116_020000.zip"

# Entpacken
Expand-Archive -Path $BACKUP_FILE -DestinationPath "C:\Temp\restore"

# In PostgreSQL importieren
Get-Content "C:\Temp\restore\postgres_20250116_020000.sql" | 
    docker exec -i messenger_postgres psql -U messenger_prod
```

---

## 🛠️ **Wartung & Updates**

### **Updates durchführen**

```powershell
cd C:\Server\Messenger

# 1. Code aktualisieren
git pull origin master

# 2. Rebuild (falls nötig)
docker compose -f docker-compose.yml -f docker-compose.lan.yml build

# 3. Rolling Update (kein Downtime)
docker compose -f docker-compose.yml -f docker-compose.lan.yml up -d

# 4. Alte Images aufräumen
docker image prune -f
```

### **Service Restart**

```powershell
# Einzelner Service
docker compose restart auth-service

# Alle Services
docker compose -f docker-compose.yml -f docker-compose.lan.yml restart
```

---

## 🆘 **Troubleshooting**

### **Problem: Docker Desktop startet nicht**

```powershell
# Docker Desktop Status
Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue

# Manuell starten
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"

# Logs prüfen
Get-Content "$env:LOCALAPPDATA\Docker\log.txt" -Tail 50
```

### **Problem: Services nicht erreichbar (LAN)**

```powershell
# Port 80 prüfen
Test-NetConnection -ComputerName 192.168.1.100 -Port 80

# Firewall-Regel prüfen
Get-NetFirewallRule -DisplayName "Messenger HTTP (LAN)"

# Container Status
docker compose ps
```

### **Problem: Client kann sich nicht verbinden**

**Checkliste**:

1. ❌ Falsche BaseAddress im Client (muss LAN-IP sein)
2. ❌ Firewall blockiert Port 80
3. ❌ Docker Container nicht gestartet

**Lösung**:

```powershell
# 1. Client-Konfiguration prüfen
Select-String -Path "src\Frontend\MessengerClient\App.xaml.cs" -Pattern "BaseAddress"

# 2. Firewall-Regel prüfen
Get-NetFirewallRule -DisplayName "Messenger HTTP (LAN)"

# 3. Container Status
docker compose ps
```

---

## ✅ **Deployment Checkliste**

### **Pre-Deployment**
- [x] Windows 11 Pro installiert
- [x] Docker Desktop installiert
- [x] WSL2 aktiviert
- [x] `.env.production` erstellt
- [x] Secrets generiert
- [x] LAN-IP ermittelt
- [x] `nginx-lan.conf` angepasst

### **Deployment**
- [ ] Docker Images gebaut
- [ ] Services gestartet
- [ ] Health Checks bestanden
- [ ] Firewall-Regeln aktiv
- [ ] Client-Build erfolgreich

### **Post-Deployment**
- [ ] Backup-Script getestet
- [ ] Task Scheduler aktiviert
- [ ] 5 Testbenutzer registriert
- [ ] Nachrichtenaustausch getestet
- [ ] Dokumentation aktualisiert

---

## 🎯 **Quick Start Summary**

```powershell
# 1. WSL2 + Docker Desktop
wsl --install
winget install Docker.DockerDesktop
Restart-Computer

# 2. Projekt klonen
git clone https://github.com/Krialder/Messenger-App.git C:\Server\Messenger
cd C:\Server\Messenger

# 3. Secrets generieren
powershell -ExecutionPolicy Bypass -File scripts\windows\generate-secrets.ps1

# 4. LAN-IP in nginx-lan.conf eintragen
notepad nginx\nginx-lan.conf

# 5. Firewall öffnen
New-NetFirewallRule -DisplayName "Messenger HTTP (LAN)" -Direction Inbound -LocalPort 80 -Protocol TCP -Action Allow

# 6. Docker Services starten
docker compose -f docker-compose.yml -f docker-compose.lan.yml up -d

# 7. Health Check
Invoke-RestMethod -Uri "http://DEINE-LAN-IP/health"

# 8. Client bauen
.\build-client.bat
```

**Deployment-Zeit**: ~2-3 Stunden

---

## 📞 **Support**

**Interner Support**:
- IT-Abteilung: (Kontakt eintragen)

**Externer Support**:
- GitHub Issues: https://github.com/Krialder/Messenger-App/issues
- Documentation: [README.md](../README.md)

---

**Status**: ✅ **Windows 11 LAN Deployment Ready**

**Version**: 1.0 Windows Edition  
**Created**: 2025-01-16  
**Target**: 60 Benutzer (intern) → 1000 Benutzer (öffentlich)

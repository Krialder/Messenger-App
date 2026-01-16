# Windows Deployment Scripts

Automatisierte PowerShell-Scripts für Windows 11 Server Deployment.

---

## 📁 Verfügbare Scripts

### **1. `generate-secrets.ps1`** - Secret Generator ⭐

Generiert automatisch alle Production Secrets.

**Verwendung**:
```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows\generate-secrets.ps1
```

**Was es macht**:
- ✅ Erstellt `.env.production` aus Template
- ✅ Generiert sichere Secrets (JWT, Passwörter)
- ✅ Validiert Konfiguration
- ✅ Zeigt LAN-IP an
- ✅ Erstellt Backup

**Parameter**:
- `-Force`: Überschreibt existierende `.env.production` ohne Nachfrage

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

ℹ️  LAN-IP gefunden: 192.168.1.100
```

---

### **2. `Deploy-Production.ps1`** - Production Deployment ⭐

Vollständiges Production Deployment (analog zu Linux `deploy-production.sh`).

**Verwendung**:
```powershell
# Standard (HTTPS mit SSL)
powershell -ExecutionPolicy Bypass -File scripts\windows\Deploy-Production.ps1

# LAN ohne SSL (HTTP)
powershell -ExecutionPolicy Bypass -File scripts\windows\Deploy-Production.ps1 -SkipSSL

# Ohne Health Check (schneller)
powershell -ExecutionPolicy Bypass -File scripts\windows\Deploy-Production.ps1 -SkipHealthCheck

# Alle Optionen
powershell -ExecutionPolicy Bypass -File scripts\windows\Deploy-Production.ps1 -SkipSSL -SkipHealthCheck -Force
```

**Was es macht**:
1. ✅ Prüft Prerequisites (Docker, Docker Compose)
2. ✅ Validiert Secrets (`.env.production`)
3. ✅ Prüft SSL-Zertifikate (optional)
4. ✅ Baut Docker Images
5. ✅ Stoppt alte Container
6. ✅ Startet Services
7. ✅ Führt Health Checks aus

**Parameter**:
- `-SkipSSL`: Überspringt SSL-Check (für LAN-Deployment)
- `-SkipHealthCheck`: Überspringt Health-Check
- `-Force`: Überspringt Admin-Check

**Ausgabe**:
```
╔══════════════════════════════════════════╗
║   Secure Messenger - Production Deploy  ║
╚══════════════════════════════════════════╝

[1/7] Checking prerequisites...
✅ Docker installed
✅ Docker Compose installed
✅ .env.production found

[2/7] Validating secrets...
✅ All secrets configured

[3/7] Checking SSL certificates...
✅ SSL certificates found

[4/7] Building Docker images...
✅ Images built successfully

[5/7] Stopping old containers...
✅ Old containers stopped

[6/7] Starting production services...
✅ Services started
✅ All services are healthy!

[7/7] Running health checks...
✅ Health check passed (HTTP 200)

╔══════════════════════════════════════════╗
║         Deployment Complete!             ║
╚══════════════════════════════════════════╝

✅ Deployment successful! 🚀
```

---

### **3. `backup-database.ps1`** - Datenbank Backup

Erstellt automatisches PostgreSQL Backup.

**Verwendung**:
```powershell
# Standard Backup
powershell -ExecutionPolicy Bypass -File scripts\windows\backup-database.ps1

# Eigenes Backup-Verzeichnis
powershell -ExecutionPolicy Bypass -File scripts\windows\backup-database.ps1 -BackupDir "D:\Backups"

# Andere Retention (Standardwert: 30 Tage)
powershell -ExecutionPolicy Bypass -File scripts\windows\backup-database.ps1 -RetentionDays 60
```

**Was es macht**:
- ✅ Erstellt PostgreSQL Dump via Docker
- ✅ Komprimiert zu ZIP (automatisch)
- ✅ Löscht alte Backups (> 30 Tage)
- ✅ Zeigt Backup-Statistiken

**Parameter**:
- `-BackupDir`: Backup-Verzeichnis (Standard: `C:\Server\Messenger\backups`)
- `-RetentionDays`: Aufbewahrungsdauer in Tagen (Standard: 30)

**Ausgabe**:
```
╔══════════════════════════════════════════╗
║   Secure Messenger - Database Backup    ║
╚══════════════════════════════════════════╝

ℹ️  Erstelle Datenbank-Backup...
✅ SQL Dump erstellt: 45.2 MB
✅ Backup komprimiert: 8.3 MB (Kompression: 81.6%)

ℹ️  Bereinige alte Backups (älter als 30 Tage)...
✅ 2 alte Backup(s) gelöscht

╔══════════════════════════════════════════╗
║          Backup erfolgreich!             ║
╚══════════════════════════════════════════╝

ℹ️  Gesamt: 15 Backup(s), 0.12 GB
```

**Task Scheduler einrichten** (täglich um 02:00 Uhr):
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

---

## 🚀 Quick Start

### **Erste Deployment (komplett neu)**:

```powershell
# 1. Secrets generieren
powershell -ExecutionPolicy Bypass -File scripts\windows\generate-secrets.ps1

# 2. LAN-IP in nginx-lan.conf eintragen
notepad nginx\nginx-lan.conf
# Zeile 41: server_name DEINE-IP localhost;

# 3. Deployment starten (LAN ohne SSL)
powershell -ExecutionPolicy Bypass -File scripts\windows\Deploy-Production.ps1 -SkipSSL

# 4. Backup einrichten (Task Scheduler)
# Siehe oben
```

### **Spätere Deployments (Updates)**:

```powershell
# Code aktualisieren
git pull origin master

# Deployment mit Rebuild
powershell -ExecutionPolicy Bypass -File scripts\windows\Deploy-Production.ps1 -SkipSSL
```

---

## 📋 Batch-Launcher (Root-Verzeichnis)

Für einfachere Nutzung gibt es Batch-Dateien im Root:

```cmd
deploy.bat              # Production Deployment
deploy.bat -SkipSSL     # LAN Deployment (HTTP)
```

---

## 🔧 Troubleshooting

### **Problem: "Execution Policy"**

```powershell
# Temporär für aktuelle Session
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

# Permanent (als Administrator)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine
```

### **Problem: "Docker nicht gefunden"**

```powershell
# Docker Desktop Status prüfen
Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue

# Manuell starten
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
```

### **Problem: ".env.production nicht gefunden"**

```powershell
# Template vorhanden?
Test-Path .env.production.example

# Secret-Generator ausführen
powershell -ExecutionPolicy Bypass -File scripts\windows\generate-secrets.ps1
```

---

## 📊 Vergleich: Linux vs. Windows

| Feature | Linux Script | Windows Script |
|---------|-------------|----------------|
| **Datei** | `scripts/deploy-production.sh` | `scripts/windows/Deploy-Production.ps1` |
| **Launcher** | - | `deploy.bat` |
| **Secret Generator** | Manuell (bash) | `generate-secrets.ps1` |
| **Backup** | `backup-db.sh` | `backup-database.ps1` |
| **SSL Check** | ✅ | ✅ |
| **Health Check** | ✅ | ✅ |
| **Docker Compose** | ✅ | ✅ |
| **Auto-Backup** | Cron | Task Scheduler |

---

## 🎯 Best Practices

1. **Secrets**:
   - ✅ Immer `generate-secrets.ps1` verwenden
   - ✅ Backup von `.env.production` erstellen
   - ❌ Niemals in Git committen

2. **Deployment**:
   - ✅ Erste Deployment: `-SkipSSL` für LAN
   - ✅ Updates: Rebuild mit `-Force`
   - ✅ Health Checks nicht überspringen

3. **Backup**:
   - ✅ Task Scheduler einrichten
   - ✅ Backups regelmäßig testen (Restore)
   - ✅ Retention anpassen (Standard: 30 Tage)

---

## 📞 Support

**Issues**: https://github.com/Krialder/Messenger-App/issues  
**Documentation**: [docs/WINDOWS_DEPLOYMENT.md](../../docs/WINDOWS_DEPLOYMENT.md)

---

**Version**: 1.0  
**Created**: 2025-01-16  
**Last Updated**: 2025-01-16

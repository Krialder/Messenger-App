# Scripts Quick Reference Card

> Schnellübersicht für Secure Messenger Automation Scripts

## 🚀 Hauptbefehle (Root-Verzeichnis)

```bash
setup.bat       # Komplettes Docker Setup (empfohlen)
test.bat        # Alle Tests ausführen
status.bat      # Docker Status anzeigen
cleanup.bat     # Services stoppen & bereinigen
```

## 📁 Verzeichnisstruktur

```
scripts/
├── powershell/       # Module (Kernlogik)
├── batch/            # Launcher (Einstiegspunkte)
├── archive/          # Alte Scripts (Legacy)
└── README.md         # Vollständige Dokumentation
```

## 🔧 PowerShell Module

### Common.psm1 - Utilities

```powershell
Import-Module ".\scripts\powershell\Common.psm1"

Write-Header "Mein Titel"
Write-Success "Erfolgreich!"
Write-Error "Fehler!"
Write-Warning "Warnung!"
Write-Info "Info"

if (Confirm-Action "Fortfahren?") {
    # User confirmed
}
```

### DockerSetup.psm1 - Docker Management

```powershell
Import-Module ".\scripts\powershell\DockerSetup.psm1"

# Docker prüfen
$ok = Test-DockerInstallation
if (-not $ok) { Start-DockerDesktop }

# Umgebung initialisieren
Initialize-DockerEnvironment

# Services starten
Start-DockerServices -Rebuild -WaitSeconds 90

# Health Check
$health = Test-ServiceHealth
Write-Host "Health: $($health.Rate)%"

# Status anzeigen
Show-DockerStatus

# Services stoppen
Stop-DockerServices

# Cleanup
Remove-DockerResources -IncludeVolumes
```

### TestRunner.psm1 - Tests

```powershell
Import-Module ".\scripts\powershell\TestRunner.psm1"

# Docker Tests
$results = Invoke-DockerTests
# Returns: @{ ContainerCheck, PortCheck, HealthCheck, DatabaseCheck, OverallSuccess }

# Unit Tests
Invoke-UnitTests -Filter "Category=Unit"
```

## 📋 Workflows

### Tägliche Entwicklung

```bash
# Morning
setup.bat

# Work...

# Before commit
test.bat

# End of day
cleanup.bat
```

### Neues Feature entwickeln

```bash
# 1. Setup
setup.bat

# 2. Code ändern
# ...

# 3. Tests
test.bat

# 4. Services neu bauen
setup.bat    # Wähle "j" bei "Images neu bauen?"
```

### Debugging

```bash
# 1. Status prüfen
status.bat

# 2. Logs anzeigen
docker-compose logs -f [service-name]

# 3. Container neustarten
docker-compose restart [service-name]

# 4. Kompletter Neustart
cleanup.bat
setup.bat
```

## 🐳 Docker Befehle

```bash
# Alle Services starten
docker-compose up -d

# Logs verfolgen
docker-compose logs -f

# Status
docker-compose ps

# Service neu starten
docker-compose restart auth-service

# Service stoppen
docker-compose stop auth-service

# Alle stoppen
docker-compose down

# Mit Volumes
docker-compose down -v
```

## 🔌 Service Ports

| Service | Port | URL |
|---------|------|-----|
| Gateway | 5000 | http://localhost:5000 |
| AuthService | 5001 | http://localhost:5001 |
| MessageService | 5002 | http://localhost:5002 |
| CryptoService | 5003 | http://localhost:5003 |
| NotificationService | 5004 | http://localhost:5004 |
| KeyManagementService | 5005 | http://localhost:5005 |
| UserService | 5006 | http://localhost:5006 |
| FileTransferService | 5007 | http://localhost:5007 |
| AuditLogService | 5008 | http://localhost:5008 |
| PostgreSQL | 5432 | localhost:5432 |
| Redis | 6379 | localhost:6379 |
| RabbitMQ | 5672 | localhost:5672 |
| RabbitMQ UI | 15672 | http://localhost:15672 |

## 🆘 Troubleshooting

### "Docker Desktop läuft nicht"
```bash
# Script startet automatisch, warte 45 Sekunden
# Oder manuell: Start → "Docker Desktop"
```

### "Port already in use"
```bash
# Prozess finden
netstat -ano | findstr :5000

# Prozess beenden
taskkill /PID [PID] /F

# Oder cleanup
cleanup.bat
setup.bat
```

### "Services nicht gesund"
```bash
status.bat                      # Status prüfen
docker-compose logs [service]   # Logs
cleanup.bat                     # Neustart
setup.bat
```

### ".env nicht gefunden"
```bash
# Wird automatisch erstellt
# Aber: PASSWÖRTER ANPASSEN!
notepad .env
```

## 📚 Weitere Hilfe

- **Dokumentation**: `scripts/README.md`
- **Changelog**: `CHANGELOG_v10.2.md`
- **GitHub**: https://github.com/Krialder/Messenger-App

## 🔐 Sicherheit

**Vor Produktiveinsatz .env bearbeiten!**

```bash
notepad .env

# Ändere:
POSTGRES_PASSWORD=ÄNDERE_MICH
RABBITMQ_PASSWORD=ÄNDERE_MICH
JWT_SECRET=GENERIERE_EIN_SICHERES_SECRET
```

Generiere Secrets:
```powershell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

---

**Version**: 10.2.0  
**Erstellt**: 2025-01-16

# Secure Messenger - Scripts Dokumentation

Dieses Verzeichnis enthält alle Automatisierungs-Scripts für das Secure Messenger Projekt.

## 📁 Struktur

```
scripts/
├── powershell/           # PowerShell Module mit Kernfunktionalität
│   ├── Common.psm1       # Gemeinsame Utilities (Logging, UI)
│   ├── DockerSetup.psm1  # Docker Setup & Management
│   └── TestRunner.psm1   # Test-Funktionen
├── batch/                # Batch-Launcher (Einstiegspunkte)
│   ├── setup.bat         # Docker Setup starten
│   ├── test.bat          # Tests ausführen
│   ├── cleanup.bat       # Docker Cleanup
│   └── status.bat        # Status anzeigen
└── archive/              # Alte/Legacy Scripts
```

## 🚀 Verwendung

### Aus dem Root-Verzeichnis (empfohlen)

```cmd
setup.bat       # Komplettes Docker Setup
test.bat        # Alle Tests ausführen
status.bat      # Docker Status anzeigen
cleanup.bat     # Docker bereinigen
```

### Direkt aus scripts/batch/

```cmd
cd scripts\batch
setup.bat
test.bat
# ... etc
```

## 📦 PowerShell Module

### Common.psm1 - Gemeinsame Utilities

**Funktionen:**
- `Write-Header` - Formatierte Überschriften
- `Write-Success` / `Write-Error` / `Write-Warning` / `Write-Info` - Farbige Ausgaben
- `Confirm-Action` - Benutzer-Bestätigung
- `Get-ScriptRoot` - Script-Verzeichnis ermitteln

**Verwendung:**
```powershell
Import-Module ".\scripts\powershell\Common.psm1"
Write-Header "Mein Setup"
Write-Success "Erfolgreich!"
```

### DockerSetup.psm1 - Docker Management

**Funktionen:**

#### `Test-DockerInstallation`
Prüft ob Docker Desktop installiert und verfügbar ist.

```powershell
$isOk = Test-DockerInstallation
if (-not $isOk) { 
    Start-DockerDesktop 
}
```

#### `Start-DockerDesktop`
Startet Docker Desktop und wartet 45 Sekunden.

#### `Initialize-DockerEnvironment`
Prüft/erstellt `.env` Datei und validiert `docker-compose.yml`.

```powershell
$envReady = Initialize-DockerEnvironment -ProjectRoot "C:\MyProject"
```

#### `Start-DockerServices`
Startet alle Docker Services via `docker-compose`.

```powershell
# Ohne Rebuild
Start-DockerServices -WaitSeconds 60

# Mit Rebuild
Start-DockerServices -Rebuild -WaitSeconds 90
```

#### `Test-ServiceHealth`
Testet Health Endpoints aller 9 Microservices.

```powershell
$health = Test-ServiceHealth
# Rückgabe: @{ Healthy = 9, Total = 9, Rate = 100.0, Success = $true }
```

#### `Stop-DockerServices`
Stoppt alle Container via `docker-compose down`.

#### `Remove-DockerResources`
Bereinigt Docker (Container, Images, Networks, Volumes).

```powershell
# Mit Bestätigung
Remove-DockerResources

# Mit Volumes
Remove-DockerResources -IncludeVolumes

# Ohne Bestätigung
Remove-DockerResources -Force
```

#### `Show-DockerStatus`
Zeigt detaillierten Status (Container, Ports, Resources).

### TestRunner.psm1 - Test-Funktionen

**Funktionen:**

#### `Invoke-DockerTests`
Führt umfassende Docker/Service Tests aus:
- Container Status
- Port Connectivity (5000-5008, 5432, 6379, 5672)
- Health Checks aller Services
- Database Connectivity (PostgreSQL, Redis)

```powershell
$results = Invoke-DockerTests
# Rückgabe: @{ ContainerCheck, PortCheck, HealthCheck, DatabaseCheck, OverallSuccess }
```

#### `Invoke-UnitTests`
Führt .NET Unit Tests aus (findet automatisch `*.Tests.csproj`).

```powershell
# Alle Unit Tests
Invoke-UnitTests

# Mit Filter
Invoke-UnitTests -Filter "Category=Integration"
```

## 🔧 Batch-Launcher

### setup.bat
**Ablauf:**
1. Docker Installation prüfen (startet Docker Desktop falls nötig)
2. Umgebung initialisieren (.env, docker-compose.yml)
3. Services starten (optional mit Rebuild)
4. Health Checks durchführen

**Ausgabe:**
```
✅ Alle Services laufen (100% gesund)
ℹ️  Gateway: http://localhost:5000
ℹ️  RabbitMQ UI: http://localhost:15672
```

### test.bat
**Ablauf:**
1. Docker Services Tests (Container, Ports, Health, DB)
2. .NET Unit Tests (alle `*.Tests.csproj`)

**Exit Codes:**
- `0` = Alle Tests bestanden
- `1` = Tests fehlgeschlagen

### cleanup.bat
**Ablauf:**
1. Bestätigung abfragen
2. Services stoppen
3. Docker Ressourcen bereinigen (Container, Images, Networks)

**Warnung:** Löscht ungenutzte Docker Objekte!

### status.bat
**Ausgabe:**
- 🐳 Container Status (`docker-compose ps`)
- 🔌 Port Status (alle 12 Ports)
- 💾 Resource Usage (`docker system df`)

## 🎯 Workflow-Beispiele

### Erstes Setup
```cmd
setup.bat          # Komplettes Setup
status.bat         # Status prüfen
test.bat           # Tests ausführen
```

### Nach Code-Änderungen
```cmd
setup.bat          # Wähle "j" bei "Images neu bauen?"
test.bat           # Validiere Änderungen
```

### Neustart nach Problemen
```cmd
cleanup.bat        # Alles aufräumen
setup.bat          # Neu starten
```

### Debugging
```cmd
status.bat                     # Status prüfen
docker-compose logs -f         # Live Logs
docker-compose logs authservice # Service-spezifisch
```

## 🔄 Integration in andere Tools

### PowerShell
```powershell
# Module direkt importieren
Import-Module ".\scripts\powershell\DockerSetup.psm1"
$health = Test-ServiceHealth
Write-Host "Status: $($health.Rate)%"
```

### CI/CD (GitHub Actions)
```yaml
- name: Setup Docker Environment
  run: |
    .\setup.bat
  shell: cmd

- name: Run Tests
  run: |
    .\test.bat
  shell: cmd
```

### Custom Scripts
```cmd
@echo off
:: Mein Custom Setup
call scripts\batch\setup.bat
if %ERRORLEVEL% equ 0 (
    echo Starte meine App...
    .\publish\MessengerClient\MessengerClient.exe
)
```

## 📋 Troubleshooting

### Problem: "Docker Desktop läuft nicht"
**Lösung:** Script startet Docker automatisch. Warte 45 Sekunden.

### Problem: "Services nicht gesund"
```cmd
status.bat                        # Status prüfen
docker-compose logs [service]     # Logs anzeigen
cleanup.bat                       # Neustart
setup.bat
```

### Problem: "Port already in use"
```cmd
# Finde Prozess auf Port 5000
netstat -ano | findstr :5000

# Beende Prozess
taskkill /PID [PID] /F

# Oder cleanup und neu starten
cleanup.bat
setup.bat
```

### Problem: ".env nicht gefunden"
Script erstellt `.env` automatisch von `.env.example`.  
Danach **manuell** Passwörter anpassen!

## 🔐 Sicherheit

⚠️ **WICHTIG:** Nach erstem Setup `.env` bearbeiten:
```env
POSTGRES_PASSWORD=ÄNDERE_MICH
RABBITMQ_PASSWORD=ÄNDERE_MICH
JWT_SECRET=GENERIERE_EIN_SICHERES_SECRET
```

Generiere sichere Secrets:
```powershell
# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

## 📚 Weitere Dokumentation

- **Setup Guide**: `docs/SETUP.md`
- **Docker Guide**: `docs/DOCKER.md`
- **Testing Guide**: `docs/TESTING.md`
- **Architecture**: `docs/ARCHITECTURE.md`

## 🆘 Support

Bei Problemen:
1. `status.bat` ausführen
2. Logs prüfen: `docker-compose logs`
3. GitHub Issues: https://github.com/Krialder/Messenger-App/issues

---

**Erstellt:** 2024  
**Version:** 1.0  
**Projekt:** Secure Messenger

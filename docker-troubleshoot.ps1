# Docker Desktop Troubleshooting für Secure Messenger
# Häufige Probleme und Lösungen

Write-Host "🔧 Docker Desktop Troubleshooting" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host "Pfad: C:\Program Files\Docker\Docker" -ForegroundColor Gray
Write-Host ""

function Show-DockerTrouble {
    param([string]$Problem, [string[]]$Solutions)
    
    Write-Host "❌ Problem: $Problem" -ForegroundColor Red
    Write-Host "🔧 Lösungen:" -ForegroundColor Yellow
    for ($i = 0; $i -lt $Solutions.Length; $i++) {
        Write-Host "   $($i + 1). $($Solutions[$i])" -ForegroundColor White
    }
    Write-Host ""
}

# 1. Docker Desktop startet nicht
Show-DockerTrouble -Problem "Docker Desktop startet nicht" -Solutions @(
    "Als Administrator ausführen",
    "Windows neustarten", 
    "Docker Desktop deinstallieren und neu installieren",
    "WSL2 prüfen: wsl --status",
    "Hyper-V aktivieren (Windows Features)"
)

# 2. Docker CLI nicht verfügbar
Show-DockerTrouble -Problem "Docker CLI nicht verfügbar" -Solutions @(
    "PowerShell als Administrator starten",
    "PATH Umgebungsvariable prüfen",
    "Docker Desktop komplett neustarten",
    "Windows Benutzer zu 'docker-users' Gruppe hinzufügen"
)

# 3. WSL2 Probleme
Show-DockerTrouble -Problem "WSL2 Probleme" -Solutions @(
    "WSL2 Backend in Docker Desktop aktivieren",
    "wsl --update ausführen",
    "wsl --set-default-version 2",
    "Ubuntu Distribution installieren: wsl --install -d Ubuntu"
)

# 4. Hyper-V Probleme  
Show-DockerTrouble -Problem "Hyper-V Probleme" -Solutions @(
    "Windows Features → Hyper-V aktivieren",
    "BIOS: Virtualisierung aktivieren", 
    "Windows Pro/Enterprise erforderlich",
    "Alternative: Docker mit WSL2 Backend"
)

# 5. Speicherplatz Probleme
Show-DockerTrouble -Problem "Nicht genug Speicherplatz" -Solutions @(
    "docker system prune -a --volumes",
    "Alte Images löschen: docker image prune -a",
    "Docker Data Root verschieben",
    "Festplatte bereinigen"
)

# 6. Netzwerk Probleme
Show-DockerTrouble -Problem "Container können nicht kommunizieren" -Solutions @(
    "Windows Firewall prüfen",
    "Docker Netzwerke zurücksetzen",
    "Proxy-Einstellungen in Docker Desktop",
    "Antivirus Software ausschließen"
)

# Jetzt automatische Diagnose starten
Write-Host "🔍 Automatische Diagnose" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan

# Test 1: Docker Desktop Prozess
$dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
if ($dockerProcess) {
    Write-Host "✅ Docker Desktop Prozess läuft" -ForegroundColor Green
} else {
    Write-Host "❌ Docker Desktop Prozess läuft nicht" -ForegroundColor Red
    Write-Host "🔧 Starte Docker Desktop..." -ForegroundColor Yellow
    
    $dockerExe = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    if (Test-Path $dockerExe) {
        Start-Process $dockerExe
        Write-Host "✅ Docker Desktop gestartet" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker Desktop nicht gefunden: $dockerExe" -ForegroundColor Red
    }
}

# Test 2: Docker CLI
try {
    $dockerVersion = docker --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker CLI funktioniert: $dockerVersion" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker CLI Fehler: $dockerVersion" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Docker CLI nicht gefunden" -ForegroundColor Red
    Write-Host "🔧 Füge Docker zum PATH hinzu oder starte als Admin" -ForegroundColor Yellow
}

# Test 3: Docker Engine
try {
    $dockerInfo = docker info --format "{{.ServerVersion}}" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker Engine läuft (Version: $dockerInfo)" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker Engine nicht erreichbar" -ForegroundColor Red
        Write-Host "🔧 Docker Desktop vollständig neustarten" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Docker Engine Test fehlgeschlagen" -ForegroundColor Red
}

# Test 4: WSL2 Status (wenn Windows)
if ($env:OS -eq "Windows_NT") {
    try {
        $wslStatus = wsl --status 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ WSL2 verfügbar" -ForegroundColor Green
            Write-Host "   $wslStatus" -ForegroundColor Gray
        } else {
            Write-Host "❌ WSL2 Problem: $wslStatus" -ForegroundColor Red
        }
    } catch {
        Write-Host "⚠️  WSL2 Status konnte nicht geprüft werden" -ForegroundColor Yellow
    }
}

# Test 5: Docker Compose
try {
    $composeVersion = docker-compose --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker Compose verfügbar: $composeVersion" -ForegroundColor Green
    } else {
        $composeV2 = docker compose version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker Compose v2 verfügbar: $composeV2" -ForegroundColor Green
        } else {
            Write-Host "❌ Docker Compose nicht verfügbar" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "❌ Docker Compose Test fehlgeschlagen" -ForegroundColor Red
}

# Test 6: Benutzergruppe
try {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $isDockerUser = $currentUser.Groups | Where-Object { $_.Value -like "*docker-users*" }
    if ($isDockerUser) {
        Write-Host "✅ Benutzer ist in 'docker-users' Gruppe" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Benutzer ist nicht in 'docker-users' Gruppe" -ForegroundColor Yellow
        Write-Host "🔧 Als Administrator ausführen: net localgroup docker-users $($currentUser.Name) /add" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Gruppenprüfung übersprungen" -ForegroundColor Yellow
}

# Test 7: Docker System Resources
Write-Host "`n📊 System Resources:" -ForegroundColor Cyan
try {
    docker system df --format "table {{.Type}}\t{{.Total}}\t{{.Active}}\t{{.Size}}" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker System Resources abgerufen" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️  Konnte System Resources nicht abrufen" -ForegroundColor Yellow
}

# Schnelle Reparatur-Optionen
Write-Host "`n🔧 Schnelle Reparatur-Optionen:" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "1. Docker Desktop neustarten:     Rechtsklick auf Docker Icon → Restart" -ForegroundColor White
Write-Host "2. Docker Engine Reset:           docker system prune -a --volumes" -ForegroundColor White
Write-Host "3. WSL2 zurücksetzen:              wsl --shutdown && wsl" -ForegroundColor White
Write-Host "4. Windows neustarten:             shutdown /r /t 0" -ForegroundColor White
Write-Host "5. Docker Desktop neu installieren: winget install Docker.DockerDesktop" -ForegroundColor White

Write-Host "`n🆘 Support Links:" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan
Write-Host "Docker Desktop Docs: https://docs.docker.com/desktop/windows/" -ForegroundColor Blue
Write-Host "WSL2 Installation:   https://docs.microsoft.com/en-us/windows/wsl/install" -ForegroundColor Blue
Write-Host "Docker Troubleshoot: https://docs.docker.com/desktop/troubleshoot/overview/" -ForegroundColor Blue

Write-Host "`n✅ Diagnose abgeschlossen!" -ForegroundColor Green

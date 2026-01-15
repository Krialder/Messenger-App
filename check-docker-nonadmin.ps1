# Docker Check - Non-Admin Version für Secure Messenger
# Funktioniert ohne Administrator-Rechte

Write-Host "🐳 Docker Desktop Check (Non-Admin)" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Hinweis: Läuft ohne Administrator-Rechte" -ForegroundColor Gray
Write-Host ""

# Prüfe aktuelle Benutzerrechte
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if ($isAdmin) {
    Write-Host "✅ Läuft als Administrator" -ForegroundColor Green
} else {
    Write-Host "⚠️  Läuft NICHT als Administrator" -ForegroundColor Yellow
    Write-Host "💡 Einige Docker-Funktionen könnten eingeschränkt sein" -ForegroundColor Cyan
}

# 1. Docker Desktop Prozess prüfen (funktioniert ohne Admin)
Write-Host "`n1️⃣ Docker Desktop Status..." -ForegroundColor Yellow

try {
    $dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
    if ($dockerProcess) {
        Write-Host "✅ Docker Desktop Prozess gefunden (PID: $($dockerProcess.Id))" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker Desktop Prozess nicht gefunden" -ForegroundColor Red
        Write-Host "🔧 Lösungen:" -ForegroundColor Yellow
        Write-Host "   1. Docker Desktop manuell starten" -ForegroundColor White
        Write-Host "   2. Windows-Taste → 'Docker Desktop' suchen" -ForegroundColor White
        Write-Host "   3. Aus Taskleiste starten (falls verfügbar)" -ForegroundColor White
        
        # Versuche Docker Desktop zu finden und zu starten
        $dockerPaths = @(
            "$env:ProgramFiles\Docker\Docker\Docker Desktop.exe",
            "${env:ProgramFiles(x86)}\Docker\Docker\Docker Desktop.exe",
            "$env:LOCALAPPDATA\Docker\Docker Desktop.exe"
        )
        
        foreach ($path in $dockerPaths) {
            if (Test-Path $path) {
                Write-Host "📍 Docker gefunden: $path" -ForegroundColor Cyan
                Write-Host "🚀 Versuche zu starten..." -ForegroundColor Yellow
                try {
                    Start-Process $path -ErrorAction Stop
                    Write-Host "✅ Docker Desktop gestartet" -ForegroundColor Green
                    Write-Host "⏳ Warte 45 Sekunden auf Start..." -ForegroundColor Yellow
                    Start-Sleep -Seconds 45
                    break
                } catch {
                    Write-Host "⚠️  Konnte Docker nicht starten: $($_.Exception.Message)" -ForegroundColor Yellow
                }
            }
        }
    }
} catch {
    Write-Host "⚠️  Fehler bei Prozess-Prüfung: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 2. Docker CLI Test (funktioniert meist auch ohne Admin)
Write-Host "`n2️⃣ Docker CLI Test..." -ForegroundColor Yellow

try {
    # Teste ob Docker CLI im PATH ist
    $dockerCmd = Get-Command docker -ErrorAction SilentlyContinue
    if ($dockerCmd) {
        Write-Host "✅ Docker CLI gefunden: $($dockerCmd.Source)" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker CLI nicht im PATH gefunden" -ForegroundColor Red
        
        # Suche Docker CLI in Standard-Pfaden
        $dockerCliPaths = @(
            "$env:ProgramFiles\Docker\Docker\resources\bin\docker.exe",
            "${env:ProgramFiles(x86)}\Docker\Docker\resources\bin\docker.exe",
            "$env:LOCALAPPDATA\Docker\Docker\resources\bin\docker.exe"
        )
        
        foreach ($path in $dockerCliPaths) {
            if (Test-Path $path) {
                Write-Host "📍 Docker CLI gefunden: $path" -ForegroundColor Cyan
                # Temporär zum PATH hinzufügen
                $env:PATH = "$env:PATH;$(Split-Path $path)"
                break
            }
        }
    }
    
    # Teste Docker Version
    $dockerVersion = docker --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker CLI funktioniert: $dockerVersion" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker CLI Fehler: $dockerVersion" -ForegroundColor Red
        Write-Host "💡 Mögliche Gründe:" -ForegroundColor Cyan
        Write-Host "   1. Docker Desktop startet noch" -ForegroundColor White
        Write-Host "   2. Docker Engine nicht bereit" -ForegroundColor White
        Write-Host "   3. Berechtigung fehlt (User nicht in docker-users)" -ForegroundColor White
    }
    
} catch {
    Write-Host "❌ Docker CLI Test fehlgeschlagen: $($_.Exception.Message)" -ForegroundColor Red
}

# 3. Docker Engine Test (kann ohne Admin fehlschlagen)
Write-Host "`n3️⃣ Docker Engine Test..." -ForegroundColor Yellow

try {
    $dockerInfo = docker info --format "{{.ServerVersion}}" 2>&1
    if ($LASTEXITCODE -eq 0 -and $dockerInfo -notlike "*error*") {
        Write-Host "✅ Docker Engine läuft (Version: $dockerInfo)" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker Engine nicht erreichbar" -ForegroundColor Red
        Write-Host "📝 Fehlermeldung: $dockerInfo" -ForegroundColor Gray
        
        # Detailliertere Diagnose
        Write-Host "`n🔍 Diagnose:" -ForegroundColor Cyan
        
        # Prüfe Docker Daemon
        try {
            $daemonInfo = docker version --format "{{.Server.Version}}" 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Docker Daemon erreichbar" -ForegroundColor Green
            } else {
                Write-Host "❌ Docker Daemon nicht erreichbar: $daemonInfo" -ForegroundColor Red
            }
        } catch {
            Write-Host "❌ Docker Daemon Test fehlgeschlagen" -ForegroundColor Red
        }
        
        # Prüfe Benutzergruppe
        try {
            $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
            $isInDockerGroup = $currentUser.Groups | Where-Object { $_.Translate([Security.Principal.SecurityIdentifier]).IsWellKnown([Security.Principal.WellKnownSidType]::BuiltinUsersSid) -or $_.Value -like "*docker*" }
            
            if ($isInDockerGroup) {
                Write-Host "✅ Benutzer hat Docker-Berechtigung" -ForegroundColor Green
            } else {
                Write-Host "⚠️  Benutzer möglicherweise nicht in 'docker-users' Gruppe" -ForegroundColor Yellow
                Write-Host "🔧 Lösung: Administrator muss User zu docker-users hinzufügen" -ForegroundColor Cyan
            }
        } catch {
            Write-Host "⚠️  Gruppenprüfung übersprungen" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "❌ Docker Engine Test Fehler: $($_.Exception.Message)" -ForegroundColor Red
}

# 4. Docker Compose Test
Write-Host "`n4️⃣ Docker Compose Test..." -ForegroundColor Yellow

try {
    # Teste docker-compose (alte Version)
    $composeVersion = docker-compose --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker Compose (v1) verfügbar: $composeVersion" -ForegroundColor Green
    } else {
        # Teste docker compose (neue Version)
        $composeV2Version = docker compose version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker Compose (v2) verfügbar: $composeV2Version" -ForegroundColor Green
        } else {
            Write-Host "❌ Docker Compose nicht verfügbar" -ForegroundColor Red
            Write-Host "Fehler v1: $composeVersion" -ForegroundColor Gray
            Write-Host "Fehler v2: $composeV2Version" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "❌ Docker Compose Test fehlgeschlagen: $($_.Exception.Message)" -ForegroundColor Red
}

# 5. Zusammenfassung und nächste Schritte
Write-Host "`n📊 Zusammenfassung:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

if (-not $isAdmin) {
    Write-Host "⚠️  WICHTIG: Läuft ohne Administrator-Rechte" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "🔧 Lösungsoptionen:" -ForegroundColor Cyan
    Write-Host "===================" -ForegroundColor Cyan
    Write-Host "1. 🔐 Als Administrator starten:" -ForegroundColor Yellow
    Write-Host "   • Windows+R → powershell → Strg+Shift+Enter" -ForegroundColor White
    Write-Host "   • Dann: cd 'I:\Just_for_fun\Messenger' && .\check-docker.ps1" -ForegroundColor White
    Write-Host ""
    Write-Host "2. 👥 User zu docker-users Gruppe hinzufügen (Admin erforderlich):" -ForegroundColor Yellow
    Write-Host "   • net localgroup docker-users $env:USERNAME /add" -ForegroundColor White
    Write-Host "   • Windows neustarten" -ForegroundColor White
    Write-Host ""
    Write-Host "3. 🐳 Docker Desktop manuell starten:" -ForegroundColor Yellow
    Write-Host "   • Windows-Taste → 'Docker Desktop' suchen" -ForegroundColor White
    Write-Host "   • Warten bis vollständig geladen" -ForegroundColor White
    Write-Host ""
    Write-Host "4. 🔄 Alternative: Entwicklung ohne Docker:" -ForegroundColor Yellow
    Write-Host "   • Services einzeln mit 'dotnet run' starten" -ForegroundColor White
    Write-Host "   • PostgreSQL/RabbitMQ separat installieren" -ForegroundColor White
}

Write-Host "`n🎯 Empfohlene nächste Schritte:" -ForegroundColor Cyan
if ($isAdmin) {
    Write-Host "✅ Admin-Rechte verfügbar - starte normales Setup:" -ForegroundColor Green
    Write-Host ".\setup-docker.ps1" -ForegroundColor White
} else {
    Write-Host "1. PowerShell als Administrator starten" -ForegroundColor White
    Write-Host "2. Dieses Skript erneut ausführen" -ForegroundColor White
    Write-Host "3. Dann: .\setup-docker.ps1" -ForegroundColor White
}

Write-Host "`n🆘 Bei Problemen:" -ForegroundColor Cyan
Write-Host ".\docker-troubleshoot.ps1" -ForegroundColor White

Write-Host "`n📋 Status Check abgeschlossen!" -ForegroundColor Green

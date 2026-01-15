# Docker Desktop Prüfung und Setup für Secure Messenger
# Pfad: C:\Program Files\Docker\Docker

Write-Host "🐳 Docker Desktop Setup - Secure Messenger" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Docker Pfad: C:\Program Files\Docker\Docker" -ForegroundColor Gray
Write-Host ""

# 1. Docker Desktop Status prüfen
Write-Host "1️⃣ Docker Desktop Status prüfen..." -ForegroundColor Yellow

# Prüfe ob Docker Desktop Prozess läuft
$dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
if ($dockerProcess) {
    Write-Host "✅ Docker Desktop Prozess läuft" -ForegroundColor Green
} else {
    Write-Host "⚠️  Docker Desktop Prozess läuft nicht" -ForegroundColor Yellow
    Write-Host "🚀 Starte Docker Desktop..." -ForegroundColor Cyan
    
    # Versuche Docker Desktop zu starten
    $dockerPath = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    if (Test-Path $dockerPath) {
        Start-Process $dockerPath
        Write-Host "⏳ Warte 30 Sekunden auf Docker Desktop Start..." -ForegroundColor Yellow
        Start-Sleep -Seconds 30
    } else {
        Write-Host "❌ Docker Desktop nicht gefunden unter: $dockerPath" -ForegroundColor Red
        Write-Host "📥 Bitte installiere Docker Desktop von: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
        exit 1
    }
}

# 2. Docker CLI Verfügbarkeit prüfen
Write-Host "`n2️⃣ Docker CLI prüfen..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker CLI verfügbar: $dockerVersion" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker CLI nicht verfügbar" -ForegroundColor Red
        Write-Host "⏳ Warte weitere 30 Sekunden..." -ForegroundColor Yellow
        Start-Sleep -Seconds 30
        
        # Nochmal versuchen
        $dockerVersion = docker --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker CLI jetzt verfügbar: $dockerVersion" -ForegroundColor Green
        } else {
            Write-Host "❌ Docker CLI immer noch nicht verfügbar" -ForegroundColor Red
            Write-Host "🔧 Lösungen:" -ForegroundColor Yellow
            Write-Host "   1. Docker Desktop neustarten" -ForegroundColor White
            Write-Host "   2. PowerShell als Administrator ausführen" -ForegroundColor White
            Write-Host "   3. Windows neustarten" -ForegroundColor White
            exit 1
        }
    }
}
catch {
    Write-Host "❌ Fehler beim Docker CLI Test: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 3. Docker Compose prüfen
Write-Host "`n3️⃣ Docker Compose prüfen..." -ForegroundColor Yellow
try {
    $dockerComposeVersion = docker-compose --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker Compose verfügbar: $dockerComposeVersion" -ForegroundColor Green
    } else {
        # Versuche mit 'docker compose' (neuere Syntax)
        $dockerComposeVersion = docker compose version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker Compose (v2) verfügbar: $dockerComposeVersion" -ForegroundColor Green
        } else {
            Write-Host "❌ Docker Compose nicht verfügbar" -ForegroundColor Red
            exit 1
        }
    }
}
catch {
    Write-Host "❌ Fehler beim Docker Compose Test: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 4. Docker Engine Status prüfen
Write-Host "`n4️⃣ Docker Engine Status prüfen..." -ForegroundColor Yellow
try {
    $dockerInfo = docker info --format "{{.ServerVersion}}" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker Engine läuft (Version: $dockerInfo)" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker Engine läuft nicht oder ist nicht erreichbar" -ForegroundColor Red
        Write-Host "⏳ Warte weitere 20 Sekunden und versuche erneut..." -ForegroundColor Yellow
        Start-Sleep -Seconds 20
        
        $dockerInfo = docker info --format "{{.ServerVersion}}" 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker Engine jetzt verfügbar (Version: $dockerInfo)" -ForegroundColor Green
        } else {
            Write-Host "❌ Docker Engine immer noch nicht verfügbar" -ForegroundColor Red
            Write-Host "🔧 Mögliche Ursachen:" -ForegroundColor Yellow
            Write-Host "   1. Docker Desktop startet noch" -ForegroundColor White
            Write-Host "   2. WSL2 Problem (bei Windows)" -ForegroundColor White
            Write-Host "   3. Hyper-V Problem" -ForegroundColor White
            Write-Host "   4. Berechtigungen fehlen" -ForegroundColor White
            exit 1
        }
    }
}
catch {
    Write-Host "❌ Fehler beim Docker Engine Test: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 Docker Desktop Setup erfolgreich!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

# 5. Zeige Docker System Info
Write-Host "`n📊 System Information:" -ForegroundColor Cyan
try {
    Write-Host "Docker Version:" -ForegroundColor White
    docker --version
    
    Write-Host "`nDocker Compose Version:" -ForegroundColor White
    docker-compose --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        docker compose version
    }
    
    Write-Host "`nDocker System Info:" -ForegroundColor White
    docker system df --format "table {{.Type}}\t{{.Total}}\t{{.Active}}\t{{.Size}}"
}
catch {
    Write-Host "⚠️  Konnte System-Info nicht abrufen" -ForegroundColor Yellow
}

Write-Host "`n🚀 Nächste Schritte:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan
Write-Host "1. Messenger Backend starten:  .\setup-docker.ps1" -ForegroundColor White
Write-Host "2. Setup testen:               .\test-docker.ps1" -ForegroundColor White
Write-Host "3. Frontend bauen:             .\build-client.bat" -ForegroundColor White
Write-Host "4. Status prüfen:              docker-compose ps" -ForegroundColor White

Write-Host "`n✅ Docker Desktop ist bereit für Secure Messenger!" -ForegroundColor Green

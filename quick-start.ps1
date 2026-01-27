# =====================================================
# Secure Messenger - Quick Start (Einfache Version)
# =====================================================

Write-Host ""
Write-Host "🚀 Secure Messenger - Quick Start" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# 1. Docker prüfen
Write-Host "[1/5] Prüfe Docker..." -ForegroundColor Yellow
$dockerRunning = Get-Process "Docker Desktop" -ErrorAction SilentlyContinue

if (-not $dockerRunning) {
    Write-Host "⚠️  Docker Desktop startet..." -ForegroundColor Yellow
    Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    Write-Host "⏳ Warte 45 Sekunden..." -ForegroundColor Yellow
    Start-Sleep -Seconds 45
}

docker --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker nicht verfügbar!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Docker OK" -ForegroundColor Green

# 2. Ins Projekt-Verzeichnis
Write-Host "`n[2/5] Wechsle ins Projekt..." -ForegroundColor Yellow
cd "I:\Just_for_fun\Messenger"
Write-Host "✅ Verzeichnis OK" -ForegroundColor Green

# 3. Docker Services starten
Write-Host "`n[3/5] Starte Services..." -ForegroundColor Yellow
docker-compose down 2>$null
docker-compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Services konnten nicht gestartet werden!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Services gestartet" -ForegroundColor Green
Write-Host "⏳ Warte 60 Sekunden..." -ForegroundColor Yellow
Start-Sleep -Seconds 60

# 4. Health Check
Write-Host "`n[4/5] Prüfe Services..." -ForegroundColor Yellow

$services = @(
    @{Name="AuthService"; Port=5001},
    @{Name="Gateway"; Port=7001}
)

foreach ($service in $services) {
    try {
        Invoke-RestMethod -Uri "http://localhost:$($service.Port)/health" -TimeoutSec 5 | Out-Null
        Write-Host "  ✅ $($service.Name)" -ForegroundColor Green
    } catch {
        Write-Host "  ❌ $($service.Name)" -ForegroundColor Red
    }
}

# 5. Zusammenfassung
Write-Host "`n[5/5] Fertig!" -ForegroundColor Yellow
Write-Host ""
Write-Host "🌐 URLs:" -ForegroundColor Cyan
Write-Host "  Swagger:  http://localhost:5001/swagger" -ForegroundColor White
Write-Host "  Gateway:  http://localhost:7001" -ForegroundColor White
Write-Host ""
Write-Host "📋 Befehle:" -ForegroundColor Cyan
Write-Host "  Status:   docker-compose ps" -ForegroundColor White
Write-Host "  Logs:     docker-compose logs -f" -ForegroundColor White
Write-Host "  Stoppen:  docker-compose down" -ForegroundColor White
Write-Host ""
Write-Host "✅ Ready to use! 🚀" -ForegroundColor Green

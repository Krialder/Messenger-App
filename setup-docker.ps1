# Docker Setup für Secure Messenger - Erweiterte Version
# Docker Desktop Pfad: C:\Program Files\Docker\Docker

Write-Host "🐳 Secure Messenger - Docker Setup" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

# Schritt 1: Docker Status prüfen
Write-Host "`n🔍 Schritt 1: Docker Status prüfen" -ForegroundColor Yellow
Write-Host "====================================" -ForegroundColor Yellow

# Docker Desktop Check
& ".\check-docker.ps1"
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker Setup fehlgeschlagen. Behebe die oben genannten Probleme." -ForegroundColor Red
    exit 1
}

# Schritt 2: Projekt-spezifische Prüfungen
Write-Host "`n🔍 Schritt 2: Projekt Setup prüfen" -ForegroundColor Yellow
Write-Host "====================================" -ForegroundColor Yellow

# .env Datei prüfen
if (!(Test-Path ".env")) {
    if (Test-Path ".env.example") {
        Write-Host "⚠️  .env Datei nicht gefunden. Erstelle von .env.example..." -ForegroundColor Yellow
        Copy-Item ".env.example" ".env"
        Write-Host "✅ .env Datei erstellt" -ForegroundColor Green
        Write-Host "🔐 WICHTIG: Bitte .env bearbeiten und sichere Passwörter setzen!" -ForegroundColor Red
    } else {
        Write-Host "❌ Weder .env noch .env.example gefunden!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✅ .env Datei gefunden" -ForegroundColor Green
}

# docker-compose.yml prüfen
if (!(Test-Path "docker-compose.yml")) {
    Write-Host "❌ docker-compose.yml nicht gefunden!" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✅ docker-compose.yml gefunden" -ForegroundColor Green
}

# Dockerfile Checks
$requiredDockerfiles = @(
    "src/Backend/AuthService/Dockerfile",
    "src/Backend/MessageService/Dockerfile", 
    "src/Backend/GatewayService/Dockerfile",
    "src/Backend/UserService/Dockerfile",
    "src/Backend/KeyManagementService/Dockerfile",
    "src/Backend/NotificationService/Dockerfile",
    "src/Backend/FileTransferService/Dockerfile",
    "src/Backend/AuditLogService/Dockerfile",
    "src/Backend/CryptoService/Dockerfile"
)

$missingDockerfiles = @()
foreach ($dockerfile in $requiredDockerfiles) {
    if (!(Test-Path $dockerfile)) {
        $missingDockerfiles += $dockerfile
    }
}

if ($missingDockerfiles.Count -gt 0) {
    Write-Host "❌ Fehlende Dockerfiles:" -ForegroundColor Red
    foreach ($missing in $missingDockerfiles) {
        Write-Host "   - $missing" -ForegroundColor Red
    }
    exit 1
} else {
    Write-Host "✅ Alle Dockerfiles gefunden (9/9)" -ForegroundColor Green
}

# Schritt 3: Alte Container aufräumen
Write-Host "`n🧹 Schritt 3: Alte Container aufräumen" -ForegroundColor Yellow
Write-Host "=======================================" -ForegroundColor Yellow

Write-Host "🛑 Stoppe alte Messenger Container..." -ForegroundColor Cyan
try {
    docker-compose down 2>$null
    Write-Host "✅ Alte Container gestoppt" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Keine alten Container zu stoppen" -ForegroundColor Yellow
}

# Optional: Bereinige verwaiste Container
Write-Host "🧹 Bereinige verwaiste Container..." -ForegroundColor Cyan
try {
    $prunedContainers = docker container prune -f 2>$null
    if ($prunedContainers -match "deleted") {
        Write-Host "✅ Verwaiste Container bereinigt" -ForegroundColor Green
    } else {
        Write-Host "ℹ️  Keine verwaisten Container gefunden" -ForegroundColor Blue
    }
} catch {
    Write-Host "⚠️  Container-Bereinigung übersprungen" -ForegroundColor Yellow
}

# Schritt 4: Images bauen und Services starten
Write-Host "`n🚀 Schritt 4: Services bauen und starten" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Yellow

Write-Host "📦 Baue Docker Images..." -ForegroundColor Cyan
Write-Host "⏳ Dies kann 5-10 Minuten dauern (erstes Mal)..." -ForegroundColor Yellow

try {
    # Verwende docker-compose build mit Progress-Anzeige
    docker-compose build --parallel
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker Images erfolgreich gebaut" -ForegroundColor Green
    } else {
        Write-Host "❌ Fehler beim Bauen der Docker Images" -ForegroundColor Red
        Write-Host "🔧 Versuche Logs anzuzeigen..." -ForegroundColor Yellow
        docker-compose build --no-cache
        exit 1
    }
} catch {
    Write-Host "❌ Kritischer Fehler beim Docker Build: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🚀 Starte alle Services..." -ForegroundColor Cyan
try {
    docker-compose up -d
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Services gestartet" -ForegroundColor Green
    } else {
        Write-Host "❌ Fehler beim Starten der Services" -ForegroundColor Red
        docker-compose logs
        exit 1
    }
} catch {
    Write-Host "❌ Kritischer Fehler beim Service Start: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Schritt 5: Warten und Health Checks
Write-Host "`n⏳ Schritt 5: Warte auf Service-Start" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Yellow

Write-Host "⏳ Warte 60 Sekunden auf vollständigen Service-Start..." -ForegroundColor Cyan
$waitTime = 60
for ($i = 1; $i -le $waitTime; $i++) {
    Write-Progress -Activity "Warte auf Services" -Status "$i/$waitTime Sekunden" -PercentComplete (($i / $waitTime) * 100)
    Start-Sleep -Seconds 1
}
Write-Progress -Completed -Activity "Warte auf Services"

# Schritt 6: Service Status prüfen
Write-Host "`n🔍 Schritt 6: Service Status" -ForegroundColor Yellow
Write-Host "=============================" -ForegroundColor Yellow

Write-Host "🐳 Container Status:" -ForegroundColor Cyan
try {
    docker-compose ps
} catch {
    Write-Host "❌ Konnte Container Status nicht abrufen" -ForegroundColor Red
}

Write-Host "`n🔌 Port Status:" -ForegroundColor Cyan
$ports = @(5432, 6379, 5672, 5000, 5001, 5002, 5003, 5004, 5005, 5006, 5007, 5008)
foreach ($port in $ports) {
    try {
        $connection = Test-NetConnection -ComputerName "localhost" -Port $port -WarningAction SilentlyContinue
        if ($connection.TcpTestSucceeded) {
            Write-Host "✅ Port $port verfügbar" -ForegroundColor Green
        } else {
            Write-Host "❌ Port $port nicht verfügbar" -ForegroundColor Red
        }
    } catch {
        Write-Host "⚠️  Port $port - Fehler beim Test" -ForegroundColor Yellow
    }
}

# Schritt 7: API Health Checks
Write-Host "`n🩺 Schritt 7: API Health Checks" -ForegroundColor Yellow
Write-Host "=================================" -ForegroundColor Yellow

$healthEndpoints = @(
    @{Name="Gateway"; Url="http://localhost:5000/health"},
    @{Name="AuthService"; Url="http://localhost:5001/health"},
    @{Name="MessageService"; Url="http://localhost:5002/health"},
    @{Name="CryptoService"; Url="http://localhost:5003/health"},
    @{Name="NotificationService"; Url="http://localhost:5004/health"},
    @{Name="KeyManagementService"; Url="http://localhost:5005/health"},
    @{Name="UserService"; Url="http://localhost:5006/health"},
    @{Name="FileTransferService"; Url="http://localhost:5007/health"},
    @{Name="AuditLogService"; Url="http://localhost:5008/health"}
)

$healthyServices = 0
foreach ($endpoint in $healthEndpoints) {
    try {
        $response = Invoke-RestMethod -Uri $endpoint.Url -Method Get -TimeoutSec 10
        if ($response.status -eq "Healthy") {
            Write-Host "✅ $($endpoint.Name) - Healthy" -ForegroundColor Green
            $healthyServices++
        } else {
            Write-Host "⚠️  $($endpoint.Name) - Status: $($response.status)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ $($endpoint.Name) - Nicht verfügbar" -ForegroundColor Red
    }
}

# Schritt 8: Zusammenfassung
Write-Host "`n📊 Setup Zusammenfassung" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan

$totalServices = $healthEndpoints.Count
$successRate = [math]::Round(($healthyServices / $totalServices) * 100, 1)

Write-Host "Gesunde Services: $healthyServices/$totalServices" -ForegroundColor White
Write-Host "Erfolgsrate: $successRate%" -ForegroundColor $(if ($successRate -ge 90) { "Green" } elseif ($successRate -ge 70) { "Yellow" } else { "Red" })

# URLs anzeigen
Write-Host "`n🌐 Service URLs:" -ForegroundColor Cyan
Write-Host "API Gateway:        http://localhost:5000" -ForegroundColor Yellow
Write-Host "Auth Service:       http://localhost:5001" -ForegroundColor Yellow  
Write-Host "Message Service:    http://localhost:5002" -ForegroundColor Yellow
Write-Host "Crypto Service:     http://localhost:5003" -ForegroundColor Yellow
Write-Host "Notification:       http://localhost:5004" -ForegroundColor Yellow
Write-Host "Key Management:     http://localhost:5005" -ForegroundColor Yellow
Write-Host "User Service:       http://localhost:5006" -ForegroundColor Yellow
Write-Host "File Transfer:      http://localhost:5007" -ForegroundColor Yellow
Write-Host "Audit Log:          http://localhost:5008" -ForegroundColor Yellow
Write-Host "RabbitMQ UI:        http://localhost:15672 (user: messenger)" -ForegroundColor Yellow

# Nächste Schritte
Write-Host "`n🚀 Nächste Schritte:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

if ($successRate -ge 90) {
    Write-Host "🎉 Setup erfolgreich! Alle Services laufen." -ForegroundColor Green
    Write-Host "1. Frontend bauen:    .\build-client.bat" -ForegroundColor White
    Write-Host "2. Setup testen:      .\test-docker.ps1" -ForegroundColor White
    Write-Host "3. App starten:       .\publish\MessengerClient\MessengerClient.exe" -ForegroundColor White
    Write-Host "4. Logs anzeigen:     docker-compose logs -f" -ForegroundColor White
} elseif ($successRate -ge 70) {
    Write-Host "⚠️  Setup teilweise erfolgreich ($successRate%)" -ForegroundColor Yellow
    Write-Host "1. Fehler prüfen:     docker-compose logs -f" -ForegroundColor White
    Write-Host "2. Services neustarten: docker-compose restart" -ForegroundColor White
    Write-Host "3. Setup erneut testen: .\test-docker.ps1" -ForegroundColor White
} else {
    Write-Host "❌ Setup fehlgeschlagen ($successRate%)" -ForegroundColor Red
    Write-Host "🔧 Empfohlene Schritte:" -ForegroundColor Yellow
    Write-Host "1. Logs prüfen:       docker-compose logs" -ForegroundColor White
    Write-Host "2. Services stoppen:  docker-compose down" -ForegroundColor White
    Write-Host "3. Cleanup:           docker system prune -f" -ForegroundColor White
    Write-Host "4. Erneut versuchen:  .\setup-docker.ps1" -ForegroundColor White
}

Write-Host "`n✅ Docker Setup für Secure Messenger abgeschlossen!" -ForegroundColor Green

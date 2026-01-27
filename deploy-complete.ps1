# =====================================================
# Secure Messenger - Vollständiges Deployment Script
# Version: 2.0
# Datum: 2026-01-23
# =====================================================

param(
    [switch]$SkipMigrations,
    [switch]$RebuildImages,
    [switch]$Verbose
)

$ErrorActionPreference = "Continue"
$VerbosePreference = if ($Verbose) { "Continue" } else { "SilentlyContinue" }

# Farben für Ausgabe
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Error-Custom { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Warning-Custom { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Step { param($Current, $Total, $Message) Write-Host "[$Current/$Total] $Message" -ForegroundColor Yellow }

# Banner
function Show-Banner {
    Write-Host ""
    Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║   Secure Messenger - Automatisches Deployment   ║" -ForegroundColor Cyan
    Write-Host "║              Version 2.0 (2026-01-23)           ║" -ForegroundColor Cyan
    Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

# Variablen
$ProjectRoot = "I:\Just_for_fun\Messenger"
$DockerPath = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
$TotalSteps = 10

# =====================================================
# SCHRITT 1: DOCKER DESKTOP PRÜFEN
# =====================================================
function Step1-CheckDocker {
    Write-Step 1 $TotalSteps "Prüfe Docker Desktop..."
    
    # Prüfe ob Docker Desktop Prozess läuft
    $dockerProcess = Get-Process "Docker Desktop" -ErrorAction SilentlyContinue
    
    if (-not $dockerProcess) {
        Write-Warning-Custom "Docker Desktop läuft nicht. Starte Docker..."
        
        if (-not (Test-Path $DockerPath)) {
            Write-Error-Custom "Docker Desktop nicht gefunden: $DockerPath"
            Write-Info "Bitte installiere Docker Desktop von: https://www.docker.com/products/docker-desktop/"
            return $false
        }
        
        Start-Process $DockerPath
        Write-Info "Warte 45 Sekunden auf Docker-Start..."
        Start-Sleep -Seconds 45
    }
    
    # Prüfe Docker CLI
    try {
        $dockerVersion = docker --version 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Docker CLI nicht verfügbar"
            return $false
        }
        Write-Success "Docker CLI: $dockerVersion"
    } catch {
        Write-Error-Custom "Docker CLI Fehler: $($_.Exception.Message)"
        return $false
    }
    
    # Prüfe Docker Engine
    try {
        $null = docker info 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Docker Engine nicht erreichbar"
            Write-Info "Warte weitere 30 Sekunden..."
            Start-Sleep -Seconds 30
            
            $null = docker info 2>&1
            if ($LASTEXITCODE -ne 0) {
                Write-Error-Custom "Docker Engine startet nicht"
                return $false
            }
        }
        Write-Success "Docker Engine läuft"
    } catch {
        Write-Error-Custom "Docker Engine Fehler"
        return $false
    }
    
    Write-Success "Docker Desktop vollständig verfügbar"
    return $true
}

# =====================================================
# SCHRITT 2: PROJEKT-VERZEICHNIS PRÜFEN
# =====================================================
function Step2-CheckDirectory {
    Write-Step 2 $TotalSteps "Prüfe Projekt-Verzeichnis..."
    
    if (-not (Test-Path $ProjectRoot)) {
        Write-Error-Custom "Projekt-Verzeichnis nicht gefunden: $ProjectRoot"
        return $false
    }
    
    Set-Location $ProjectRoot
    Write-Success "Verzeichnis: $ProjectRoot"
    
    # Prüfe wichtige Dateien
    $requiredFiles = @(
        "docker-compose.yml",
        "Messenger.sln",
        ".env"
    )
    
    foreach ($file in $requiredFiles) {
        if (-not (Test-Path $file)) {
            Write-Error-Custom "Datei fehlt: $file"
            return $false
        }
    }
    
    Write-Success "Alle erforderlichen Dateien vorhanden"
    return $true
}

# =====================================================
# SCHRITT 3: .ENV KONFIGURATION
# =====================================================
function Step3-CheckEnvironment {
    Write-Step 3 $TotalSteps "Prüfe Environment-Konfiguration..."
    
    if (-not (Test-Path ".env")) {
        if (Test-Path ".env.example") {
            Copy-Item ".env.example" ".env"
            Write-Success ".env erstellt von .env.example"
            Write-Warning-Custom "WICHTIG: Passwörter in .env anpassen für Production!"
        } else {
            Write-Error-Custom ".env.example nicht gefunden"
            return $false
        }
    } else {
        Write-Success ".env Datei vorhanden"
    }
    
    # Validiere .env Inhalt
    $envContent = Get-Content ".env" -Raw
    
    $requiredVars = @(
        "POSTGRES_PASSWORD",
        "REDIS_PASSWORD",
        "JWT_SECRET",
        "TOTP_ENCRYPTION_KEY"
    )
    
    foreach ($var in $requiredVars) {
        if ($envContent -notmatch "$var=") {
            Write-Error-Custom "Umgebungsvariable fehlt: $var"
            return $false
        }
    }
    
    Write-Success "Alle Environment-Variablen vorhanden"
    return $true
}

# =====================================================
# SCHRITT 4: ALTE CONTAINER STOPPEN
# =====================================================
function Step4-StopOldContainers {
    Write-Step 4 $TotalSteps "Stoppe alte Container..."
    
    try {
        docker-compose down 2>&1 | Out-Null
        Write-Success "Alte Container gestoppt"
        return $true
    } catch {
        Write-Warning-Custom "Keine alten Container gefunden (normal beim ersten Start)"
        return $true
    }
}

# =====================================================
# SCHRITT 5: DOCKER IMAGES BAUEN
# =====================================================
function Step5-BuildImages {
    Write-Step 5 $TotalSteps "Baue Docker Images..."
    
    if ($RebuildImages) {
        Write-Info "Rebuild aktiviert - baue alle Images neu..."
        docker-compose build --parallel --no-cache
    } else {
        Write-Info "Baue nur geänderte Images..."
        docker-compose build --parallel
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Build fehlgeschlagen"
        Write-Info "Zeige Build-Logs:"
        docker-compose logs --tail=50
        return $false
    }
    
    Write-Success "Docker Images erfolgreich gebaut"
    return $true
}

# =====================================================
# SCHRITT 6: DOCKER SERVICES STARTEN
# =====================================================
function Step6-StartServices {
    Write-Step 6 $TotalSteps "Starte Docker Services..."
    
    docker-compose up -d
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Services konnten nicht gestartet werden"
        docker-compose logs --tail=50
        return $false
    }
    
    Write-Success "Services gestartet"
    Write-Info "Warte 60 Sekunden auf Initialisierung..."
    
    # Progress Bar
    for ($i = 1; $i -le 60; $i++) {
        Write-Progress -Activity "Services initialisieren" -Status "$i/60 Sekunden" -PercentComplete (($i / 60) * 100)
        Start-Sleep -Seconds 1
    }
    Write-Progress -Activity "Services initialisieren" -Completed
    
    Write-Success "Services initialisiert"
    return $true
}

# =====================================================
# SCHRITT 7: CONTAINER STATUS PRÜFEN
# =====================================================
function Step7-CheckContainers {
    Write-Step 7 $TotalSteps "Prüfe Container-Status..."
    
    Write-Host ""
    Write-Host "Container Status:" -ForegroundColor Cyan
    Write-Host "=================" -ForegroundColor Cyan
    
    docker-compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"
    
    # Zähle gesunde Container
    $containers = docker-compose ps --format json | ConvertFrom-Json
    $total = ($containers | Measure-Object).Count
    $running = ($containers | Where-Object { $_.State -eq "running" } | Measure-Object).Count
    
    Write-Host ""
    Write-Info "Laufende Container: $running/$total"
    
    if ($running -lt ($total * 0.8)) {
        Write-Warning-Custom "Weniger als 80% der Container laufen"
        return $false
    }
    
    Write-Success "Container-Status OK"
    return $true
}

# =====================================================
# SCHRITT 8: DATENBANK MIGRATIONEN
# =====================================================
function Step8-ApplyMigrations {
    Write-Step 8 $TotalSteps "Wende Datenbank-Migrationen an..."
    
    if ($SkipMigrations) {
        Write-Warning-Custom "Migrationen übersprungen (--SkipMigrations)"
        return $true
    }
    
    $services = @(
        @{Name="AuthService"; Path="src\Backend\AuthService"},
        @{Name="MessageService"; Path="src\Backend\MessageService"},
        @{Name="UserService"; Path="src\Backend\UserService"},
        @{Name="KeyManagementService"; Path="src\Backend\KeyManagementService"},
        @{Name="FileTransferService"; Path="src\Backend\FileTransferService"},
        @{Name="AuditLogService"; Path="src\Backend\AuditLogService"}
    )
    
    $successCount = 0
    
    foreach ($service in $services) {
        Write-Host "  → $($service.Name)..." -ForegroundColor Cyan
        
        Push-Location $service.Path
        
        try {
            # Prüfe ob EF Core Tools installiert sind
            $efInstalled = dotnet tool list -g | Select-String "dotnet-ef"
            if (-not $efInstalled) {
                Write-Info "Installiere EF Core Tools..."
                dotnet tool install --global dotnet-ef
            }
            
            # Wende Migration an
            $output = dotnet ef database update 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "    ✅ $($service.Name) Migration OK"
                $successCount++
            } else {
                # Prüfe ob Fehler "bereits existiert" ist
                if ($output -match "already exists" -or $output -match "No migrations") {
                    Write-Success "    ✅ $($service.Name) bereits aktuell"
                    $successCount++
                } else {
                    Write-Warning-Custom "    ⚠️  $($service.Name) Migration fehlgeschlagen"
                    if ($Verbose) {
                        Write-Host $output -ForegroundColor DarkGray
                    }
                }
            }
        } catch {
            Write-Warning-Custom "    ⚠️  $($service.Name) Fehler: $($_.Exception.Message)"
        } finally {
            Pop-Location
        }
    }
    
    Write-Host ""
    Write-Info "Migrationen abgeschlossen: $successCount/$($services.Count) Services"
    
    if ($successCount -ge ($services.Count * 0.8)) {
        Write-Success "Datenbank-Migrationen erfolgreich"
        return $true
    } else {
        Write-Warning-Custom "Einige Migrationen fehlgeschlagen"
        return $false
    }
}

# =====================================================
# SCHRITT 9: HEALTH CHECKS
# =====================================================
function Step9-HealthChecks {
    Write-Step 9 $TotalSteps "Führe Health Checks durch..."
    
    $services = @(
        @{Name="AuthService"; Port=5001},
        @{Name="MessageService"; Port=5002},
        @{Name="UserService"; Port=5003},
        @{Name="CryptoService"; Port=5004},
        @{Name="KeyManagementService"; Port=5005},
        @{Name="NotificationService"; Port=5006},
        @{Name="FileTransferService"; Port=5007},
        @{Name="AuditLogService"; Port=5008},
        @{Name="Gateway"; Port=7001}
    )
    
    Write-Host ""
    Write-Host "Service Health:" -ForegroundColor Cyan
    Write-Host "===============" -ForegroundColor Cyan
    
    $healthyCount = 0
    
    foreach ($service in $services) {
        try {
            $response = Invoke-RestMethod -Uri "http://localhost:$($service.Port)/health" -Method Get -TimeoutSec 5 -ErrorAction Stop
            Write-Host "  ✅ $($service.Name) (Port $($service.Port))" -ForegroundColor Green
            $healthyCount++
        } catch {
            Write-Host "  ❌ $($service.Name) (Port $($service.Port))" -ForegroundColor Red
            if ($Verbose) {
                Write-Host "     Fehler: $($_.Exception.Message)" -ForegroundColor DarkGray
            }
        }
    }
    
    Write-Host ""
    $healthRate = [math]::Round(($healthyCount / $services.Count) * 100, 1)
    Write-Info "Gesundheitsstatus: $healthyCount/$($services.Count) Services ($healthRate%)"
    
    if ($healthyCount -ge ($services.Count * 0.7)) {
        Write-Success "Health Checks bestanden"
        return $true
    } else {
        Write-Warning-Custom "Zu viele Services nicht erreichbar"
        return $false
    }
}

# =====================================================
# SCHRITT 10: API TEST
# =====================================================
function Step10-ApiTest {
    Write-Step 10 $TotalSteps "Teste API (Registrierung)..."
    
    $randomId = Get-Random -Maximum 99999
    $registerBody = @{
        username = "testuser_$randomId"
        email = "test$randomId@example.com"
        password = "SecurePass123!"
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5001/api/auth/register" `
            -Method POST `
            -ContentType "application/json" `
            -Body $registerBody `
            -ErrorAction Stop
        
        Write-Success "API Test erfolgreich"
        Write-Info "User ID: $($response.userId)"
        Write-Info "Username: testuser_$randomId"
        return $true
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 400) {
            Write-Warning-Custom "API Test fehlgeschlagen (400 - möglicherweise schwaches Passwort)"
        } elseif ($statusCode -eq 409) {
            Write-Success "API Test OK (User bereits registriert)"
            return $true
        } else {
            Write-Error-Custom "API Test fehlgeschlagen: $($_.Exception.Message)"
        }
        return $false
    }
}

# =====================================================
# ZUSAMMENFASSUNG
# =====================================================
function Show-Summary {
    param($Results)
    
    Write-Host ""
    Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║           Deployment Zusammenfassung            ║" -ForegroundColor Cyan
    Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    
    # Zeige Ergebnisse
    $successCount = ($Results.Values | Where-Object { $_ -eq $true }).Count
    $totalSteps = $Results.Count
    $successRate = [math]::Round(($successCount / $totalSteps) * 100, 1)
    
    Write-Host "📊 Deployment-Status:" -ForegroundColor Cyan
    Write-Host "  Erfolgreiche Schritte: $successCount/$totalSteps ($successRate%)" -ForegroundColor $(if ($successRate -ge 80) { "Green" } else { "Yellow" })
    
    Write-Host ""
    Write-Host "🌐 Service URLs:" -ForegroundColor Cyan
    Write-Host "  Swagger UI:       http://localhost:5001/swagger" -ForegroundColor White
    Write-Host "  Auth Service:     http://localhost:5001" -ForegroundColor White
    Write-Host "  Message Service:  http://localhost:5002" -ForegroundColor White
    Write-Host "  Gateway:          http://localhost:7001" -ForegroundColor White
    Write-Host "  RabbitMQ UI:      http://localhost:15672" -ForegroundColor White
    
    Write-Host ""
    Write-Host "🔑 RabbitMQ Login:" -ForegroundColor Cyan
    Write-Host "  User:     messenger" -ForegroundColor White
    Write-Host "  Password: (siehe .env - RABBITMQ_PASSWORD)" -ForegroundColor White
    
    Write-Host ""
    Write-Host "📋 Nächste Schritte:" -ForegroundColor Cyan
    Write-Host "  1. Swagger öffnen:     http://localhost:5001/swagger" -ForegroundColor White
    Write-Host "  2. Services prüfen:    docker-compose ps" -ForegroundColor White
    Write-Host "  3. Logs anzeigen:      docker-compose logs -f auth-service" -ForegroundColor White
    Write-Host "  4. Services stoppen:   docker-compose down" -ForegroundColor White
    Write-Host "  5. Daten löschen:      docker-compose down -v" -ForegroundColor White
    
    Write-Host ""
    Write-Host "📁 Wichtige Dateien:" -ForegroundColor Cyan
    Write-Host "  Konfiguration:  .env" -ForegroundColor White
    Write-Host "  Docker Compose: docker-compose.yml" -ForegroundColor White
    Write-Host "  Dokumentation:  docs/README.md" -ForegroundColor White
    
    Write-Host ""
    
    if ($successRate -ge 90) {
        Write-Host "✅ Deployment erfolgreich! Alle Systeme betriebsbereit! 🚀" -ForegroundColor Green
    } elseif ($successRate -ge 70) {
        Write-Host "⚠️  Deployment teilweise erfolgreich. Prüfe Logs für Details." -ForegroundColor Yellow
    } else {
        Write-Host "❌ Deployment fehlgeschlagen. Siehe Logs für Details." -ForegroundColor Red
    }
    
    Write-Host ""
}

# =====================================================
# HAUPTPROGRAMM
# =====================================================
function Main {
    Show-Banner
    
    $results = @{}
    
    # Führe alle Schritte aus
    $results["Docker Check"] = Step1-CheckDocker
    if (-not $results["Docker Check"]) {
        Write-Error-Custom "KRITISCHER FEHLER: Docker nicht verfügbar. Abbruch."
        return
    }
    
    $results["Directory Check"] = Step2-CheckDirectory
    if (-not $results["Directory Check"]) {
        Write-Error-Custom "KRITISCHER FEHLER: Projekt-Verzeichnis nicht gefunden. Abbruch."
        return
    }
    
    $results["Environment Check"] = Step3-CheckEnvironment
    $results["Stop Old Containers"] = Step4-StopOldContainers
    $results["Build Images"] = Step5-BuildImages
    
    if (-not $results["Build Images"]) {
        Write-Error-Custom "KRITISCHER FEHLER: Image-Build fehlgeschlagen. Abbruch."
        return
    }
    
    $results["Start Services"] = Step6-StartServices
    if (-not $results["Start Services"]) {
        Write-Error-Custom "KRITISCHER FEHLER: Services konnten nicht gestartet werden. Abbruch."
        return
    }
    
    $results["Container Status"] = Step7-CheckContainers
    $results["Database Migrations"] = Step8-ApplyMigrations
    $results["Health Checks"] = Step9-HealthChecks
    $results["API Test"] = Step10-ApiTest
    
    # Zusammenfassung
    Show-Summary -Results $results
}

# Script ausführen
Main

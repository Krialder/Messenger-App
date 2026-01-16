# DockerSetup.psm1 - Zentrale Docker Setup Funktionen für Secure Messenger

function Test-DockerInstallation {
    <#
    .SYNOPSIS
    Prüft ob Docker Desktop installiert und verfügbar ist
    #>
    
    Write-Host "🐳 Prüfe Docker Installation..." -ForegroundColor Cyan
    
    # Docker Desktop Prozess
    $dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
    if (-not $dockerProcess) {
        Write-Host "❌ Docker Desktop läuft nicht" -ForegroundColor Red
        return $false
    }
    
    Write-Host "✅ Docker Desktop Prozess gefunden" -ForegroundColor Green
    
    # Docker CLI
    try {
        $dockerVersion = docker --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker CLI: $dockerVersion" -ForegroundColor Green
        } else {
            Write-Host "❌ Docker CLI nicht verfügbar" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Docker CLI Fehler: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    # Docker Engine
    try {
        $dockerInfo = docker info --format "{{.ServerVersion}}" 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker Engine: $dockerInfo" -ForegroundColor Green
        } else {
            Write-Host "❌ Docker Engine nicht erreichbar" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Docker Engine Fehler" -ForegroundColor Red
        return $false
    }
    
    return $true
}

function Start-DockerDesktop {
    <#
    .SYNOPSIS
    Startet Docker Desktop und wartet auf Verfügbarkeit
    #>
    
    Write-Host "🚀 Starte Docker Desktop..." -ForegroundColor Cyan
    
    $dockerPaths = @(
        "$env:ProgramFiles\Docker\Docker\Docker Desktop.exe",
        "${env:ProgramFiles(x86)}\Docker\Docker\Docker Desktop.exe"
    )
    
    foreach ($path in $dockerPaths) {
        if (Test-Path $path) {
            Start-Process $path
            Write-Host "⏳ Warte 45 Sekunden auf Docker Start..." -ForegroundColor Yellow
            Start-Sleep -Seconds 45
            return $true
        }
    }
    
    Write-Host "❌ Docker Desktop nicht gefunden" -ForegroundColor Red
    return $false
}

function Initialize-DockerEnvironment {
    <#
    .SYNOPSIS
    Prüft und erstellt .env Datei, validiert docker-compose.yml
    #>
    param(
        [string]$ProjectRoot = (Get-Location).Path
    )
    
    Write-Host "📋 Prüfe Projekt-Konfiguration..." -ForegroundColor Cyan
    
    # .env Datei
    $envFile = Join-Path $ProjectRoot ".env"
    $envExample = Join-Path $ProjectRoot ".env.example"
    
    if (-not (Test-Path $envFile)) {
        if (Test-Path $envExample) {
            Copy-Item $envExample $envFile
            Write-Host "✅ .env Datei erstellt von .env.example" -ForegroundColor Green
            Write-Host "🔐 WICHTIG: .env bearbeiten und Passwörter anpassen!" -ForegroundColor Yellow
        } else {
            Write-Host "❌ .env.example nicht gefunden!" -ForegroundColor Red
            return $false
        }
    }
    
    if (Test-Path $envFile) {
        Write-Host "✅ .env Datei vorhanden" -ForegroundColor Green
    }
    
    # docker-compose.yml
    $composeFile = Join-Path $ProjectRoot "docker-compose.yml"
    if (-not (Test-Path $composeFile)) {
        Write-Host "❌ docker-compose.yml nicht gefunden!" -ForegroundColor Red
        return $false
    }
    
    Write-Host "✅ docker-compose.yml vorhanden" -ForegroundColor Green
    return $true
}

function Start-DockerServices {
    <#
    .SYNOPSIS
    Startet alle Docker Services via docker-compose
    #>
    param(
        [switch]$Rebuild,
        [int]$WaitSeconds = 60
    )
    
    Write-Host "🐳 Stoppe alte Container..." -ForegroundColor Cyan
    docker-compose down 2>$null
    
    Write-Host "🚀 Starte Services..." -ForegroundColor Cyan
    
    if ($Rebuild) {
        Write-Host "🔨 Rebuild aktiviert - baue Images neu..." -ForegroundColor Yellow
        docker-compose build --parallel
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build fehlgeschlagen" -ForegroundColor Red
            return $false
        }
    }
    
    docker-compose up -d
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Services konnten nicht gestartet werden" -ForegroundColor Red
        docker-compose logs
        return $false
    }
    
    Write-Host "✅ Services gestartet" -ForegroundColor Green
    Write-Host "⏳ Warte $WaitSeconds Sekunden auf vollständigen Start..." -ForegroundColor Yellow
    Start-Sleep -Seconds $WaitSeconds
    
    return $true
}

function Test-ServiceHealth {
    <#
    .SYNOPSIS
    Testet Health Endpoints aller Services
    #>
    
    Write-Host "🩺 Prüfe Service Health..." -ForegroundColor Cyan
    
    $services = @(
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
    
    $healthy = 0
    foreach ($service in $services) {
        try {
            $response = Invoke-RestMethod -Uri $service.Url -Method Get -TimeoutSec 10
            if ($response.status -eq "Healthy") {
                Write-Host "  ✅ $($service.Name)" -ForegroundColor Green
                $healthy++
            } else {
                Write-Host "  ⚠️  $($service.Name) - $($response.status)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "  ❌ $($service.Name)" -ForegroundColor Red
        }
    }
    
    $total = $services.Count
    $rate = [math]::Round(($healthy / $total) * 100, 1)
    
    Write-Host "`n📊 Gesundheitsstatus: $healthy/$total Services ($rate%)" -ForegroundColor Cyan
    
    return @{
        Healthy = $healthy
        Total = $total
        Rate = $rate
        Success = ($rate -ge 80)
    }
}

function Stop-DockerServices {
    <#
    .SYNOPSIS
    Stoppt alle Docker Services
    #>
    
    Write-Host "🛑 Stoppe Docker Services..." -ForegroundColor Yellow
    docker-compose down
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Services gestoppt" -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ Fehler beim Stoppen" -ForegroundColor Red
        return $false
    }
}

function Remove-DockerResources {
    <#
    .SYNOPSIS
    Bereinigt Docker Ressourcen (Container, Images, Volumes, Networks)
    #>
    param(
        [switch]$IncludeVolumes,
        [switch]$Force
    )
    
    if (-not $Force) {
        $confirm = Read-Host "⚠️  Docker Bereinigung löscht ungenutzte Ressourcen. Fortfahren? (j/N)"
        if ($confirm -ne "j" -and $confirm -ne "J") {
            Write-Host "❌ Abgebrochen" -ForegroundColor Yellow
            return $false
        }
    }
    
    Write-Host "🧹 Bereinige Docker Ressourcen..." -ForegroundColor Cyan
    
    # Stoppe Services
    docker-compose down
    
    # Bereinige Container
    Write-Host "🧹 Container..." -ForegroundColor Yellow
    docker container prune -f
    
    # Bereinige Images
    Write-Host "🧹 Images..." -ForegroundColor Yellow
    docker image prune -f
    
    # Bereinige Networks
    Write-Host "🧹 Networks..." -ForegroundColor Yellow
    docker network prune -f
    
    # Optional: Volumes
    if ($IncludeVolumes) {
        Write-Host "🧹 Volumes..." -ForegroundColor Yellow
        docker volume prune -f
    }
    
    Write-Host "✅ Bereinigung abgeschlossen" -ForegroundColor Green
    return $true
}

function Show-DockerStatus {
    <#
    .SYNOPSIS
    Zeigt detaillierten Docker Status an
    #>
    
    Write-Host "📊 Docker Status" -ForegroundColor Cyan
    Write-Host "================" -ForegroundColor Cyan
    
    # Container Status
    Write-Host "`n🐳 Container:" -ForegroundColor Yellow
    docker-compose ps
    
    # Port Status
    Write-Host "`n🔌 Ports:" -ForegroundColor Yellow
    $ports = @(5000, 5001, 5002, 5003, 5004, 5005, 5006, 5007, 5008, 5432, 6379, 5672)
    foreach ($port in $ports) {
        $conn = Test-NetConnection -ComputerName "localhost" -Port $port -WarningAction SilentlyContinue -InformationLevel Quiet
        if ($conn) {
            Write-Host "  ✅ Port $port" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Port $port" -ForegroundColor Red
        }
    }
    
    # System Resources
    Write-Host "`n💾 Resources:" -ForegroundColor Yellow
    docker system df --format "table {{.Type}}\t{{.Total}}\t{{.Active}}\t{{.Size}}"
}

# Export Funktionen
Export-ModuleMember -Function @(
    'Test-DockerInstallation',
    'Start-DockerDesktop',
    'Initialize-DockerEnvironment',
    'Start-DockerServices',
    'Test-ServiceHealth',
    'Stop-DockerServices',
    'Remove-DockerResources',
    'Show-DockerStatus'
)

# Setup-Docker.ps1 - Docker Setup Script für Secure Messenger
# Wird von scripts/batch/setup.bat aufgerufen

$ErrorActionPreference = "Stop"
$PSScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$ModulesPath = Join-Path $ProjectRoot "scripts\powershell"

# Module laden
Import-Module (Join-Path $ModulesPath "Common.psm1") -Force
Import-Module (Join-Path $ModulesPath "DockerSetup.psm1") -Force

# Wechsle ins Projekt-Root
Set-Location $ProjectRoot

try {
    # Schritt 1: Docker Prüfung
    Write-Header "Schritt 1: Docker Prüfung"
    $dockerOk = Test-DockerInstallation
    
    if (-not $dockerOk) {
        Write-Warning "Starte Docker Desktop..."
        $started = Start-DockerDesktop
        if (-not $started) {
            Write-Error "Docker konnte nicht gestartet werden"
            exit 1
        }
    }
    
    # Schritt 2: Umgebung initialisieren
    Write-Header "Schritt 2: Umgebung initialisieren"
    $envOk = Initialize-DockerEnvironment
    if (-not $envOk) {
        exit 1
    }
    
    # Schritt 3: Services starten
    Write-Header "Schritt 3: Services starten"
    $rebuild = Confirm-Action "Images neu bauen?" -DefaultYes:$false
    $servicesOk = Start-DockerServices -Rebuild:$rebuild -WaitSeconds 60
    if (-not $servicesOk) {
        exit 1
    }
    
    # Schritt 4: Health Checks
    Write-Header "Schritt 4: Health Checks"
    $health = Test-ServiceHealth
    
    # Zusammenfassung
    Write-Header "Setup abgeschlossen"
    if ($health.Success) {
        Write-Success "Alle Services laufen ($($health.Rate)% gesund)"
        Write-Info "Gateway: http://localhost:5000"
        Write-Info "RabbitMQ UI: http://localhost:15672"
    } else {
        Write-Warning "Nur $($health.Healthy)/$($health.Total) Services gesund"
        Write-Info "Prüfe Logs: docker-compose logs"
    }
    
    exit 0
    
} catch {
    Write-Error "Fehler im Setup: $($_.Exception.Message)"
    exit 1
}

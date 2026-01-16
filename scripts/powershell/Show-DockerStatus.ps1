# Show-DockerStatus.ps1 - Docker Status Script für Secure Messenger
# Wird von scripts/batch/status.bat aufgerufen

$ErrorActionPreference = "Stop"
$PSScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$ModulesPath = Join-Path $ProjectRoot "scripts\powershell"

# Module laden
Import-Module (Join-Path $ModulesPath "DockerSetup.psm1") -Force

# Wechsle ins Projekt-Root
Set-Location $ProjectRoot

try {
    Show-DockerStatus
    exit 0
    
} catch {
    Write-Host "Fehler beim Status-Abruf: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

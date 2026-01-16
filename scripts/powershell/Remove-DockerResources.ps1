# Remove-DockerResources.ps1 - Docker Cleanup Script für Secure Messenger
# Wird von scripts/batch/cleanup.bat aufgerufen

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
    Write-Header "Docker Cleanup"
    
    Stop-DockerServices
    Remove-DockerResources -Force
    
    Write-Success "Cleanup abgeschlossen"
    exit 0
    
} catch {
    Write-Error "Fehler beim Cleanup: $($_.Exception.Message)"
    exit 1
}

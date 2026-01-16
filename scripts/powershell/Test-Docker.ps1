# Test-Docker.ps1 - Test Script für Secure Messenger
# Wird von scripts/batch/test.bat aufgerufen

$ErrorActionPreference = "Stop"
$PSScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$ModulesPath = Join-Path $ProjectRoot "scripts\powershell"

# Module laden
Import-Module (Join-Path $ModulesPath "Common.psm1") -Force
Import-Module (Join-Path $ModulesPath "TestRunner.psm1") -Force
Import-Module (Join-Path $ModulesPath "DockerSetup.psm1") -Force

# Wechsle ins Projekt-Root
Set-Location $ProjectRoot

try {
    # Docker Services Tests
    Write-Header "Docker Services Tests"
    $dockerTestResults = Invoke-DockerTests
    
    # Unit Tests
    Write-Header "Unit Tests"
    $unitTestsPassed = Invoke-UnitTests -Filter "Category=Unit"
    
    # Test Zusammenfassung
    Write-Header "Test Zusammenfassung"
    if ($dockerTestResults.OverallSuccess -and $unitTestsPassed) {
        Write-Success "Alle Tests bestanden!"
        exit 0
    } else {
        Write-Error "Einige Tests fehlgeschlagen"
        exit 1
    }
    
} catch {
    Write-Error "Fehler bei Tests: $($_.Exception.Message)"
    exit 1
}

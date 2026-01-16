$ErrorActionPreference = "Continue"

Write-Host "Testing PowerShell Modules..." -ForegroundColor Cyan

# Test DockerSetup.psm1
Write-Host "`nTesting DockerSetup.psm1..." -ForegroundColor Yellow
try {
    Import-Module ".\scripts\powershell\DockerSetup.psm1" -Force -ErrorAction Stop
    Write-Host "  [OK] DockerSetup.psm1 loaded" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] DockerSetup.psm1 failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Line: $($_.InvocationInfo.ScriptLineNumber)" -ForegroundColor Red
}

# Test Common.psm1
Write-Host "`nTesting Common.psm1..." -ForegroundColor Yellow
try {
    Import-Module ".\scripts\powershell\Common.psm1" -Force -ErrorAction Stop
    Write-Host "  [OK] Common.psm1 loaded" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Common.psm1 failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test TestRunner.psm1
Write-Host "`nTesting TestRunner.psm1..." -ForegroundColor Yellow
try {
    Import-Module ".\scripts\powershell\TestRunner.psm1" -Force -ErrorAction Stop
    Write-Host "  [OK] TestRunner.psm1 loaded" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] TestRunner.psm1 failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nModule test complete." -ForegroundColor Cyan

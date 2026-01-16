# Test-Deployment.ps1
# Testet das Windows Production Deployment ohne tatsächliche Ausführung
# Verwendung: powershell -ExecutionPolicy Bypass -File scripts\windows\Test-Deployment.ps1

$ErrorActionPreference = "Stop"

Write-Host "╔══════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Deployment Validation Test             ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$testResults = @()

# Test 1: Check PowerShell Scripts
Write-Host "[TEST 1/8] Checking PowerShell Scripts..." -ForegroundColor Yellow
$requiredScripts = @(
    "scripts\windows\generate-secrets.ps1",
    "scripts\windows\Deploy-Production.ps1",
    "scripts\windows\backup-database.ps1"
)

foreach ($script in $requiredScripts) {
    if (Test-Path $script) {
        Write-Host "  ✅ $script" -ForegroundColor Green
        $testResults += @{Test="Script: $script"; Result="PASS"}
    } else {
        Write-Host "  ❌ $script NOT FOUND" -ForegroundColor Red
        $testResults += @{Test="Script: $script"; Result="FAIL"}
    }
}

# Test 2: Check Docker Compose Files
Write-Host "`n[TEST 2/8] Checking Docker Compose Files..." -ForegroundColor Yellow
$composeFiles = @(
    "docker-compose.yml",
    "docker-compose.prod.yml",
    "docker-compose.lan.yml"
)

foreach ($file in $composeFiles) {
    if (Test-Path $file) {
        Write-Host "  ✅ $file" -ForegroundColor Green
        $testResults += @{Test="Compose: $file"; Result="PASS"}
    } else {
        Write-Host "  ❌ $file NOT FOUND" -ForegroundColor Red
        $testResults += @{Test="Compose: $file"; Result="FAIL"}
    }
}

# Test 3: Check Nginx Configs
Write-Host "`n[TEST 3/8] Checking Nginx Configs..." -ForegroundColor Yellow
$nginxConfigs = @(
    "nginx\nginx.conf",
    "nginx\nginx-lan.conf"
)

foreach ($config in $nginxConfigs) {
    if (Test-Path $config) {
        Write-Host "  ✅ $config" -ForegroundColor Green
        $testResults += @{Test="Nginx: $config"; Result="PASS"}
    } else {
        Write-Host "  ❌ $config NOT FOUND" -ForegroundColor Red
        $testResults += @{Test="Nginx: $config"; Result="FAIL"}
    }
}

# Test 4: Check Launcher Scripts
Write-Host "`n[TEST 4/8] Checking Launcher Scripts..." -ForegroundColor Yellow
$launchers = @(
    "deploy.bat",
    "setup.bat",
    "test.bat",
    "status.bat",
    "cleanup.bat"
)

foreach ($launcher in $launchers) {
    if (Test-Path $launcher) {
        Write-Host "  ✅ $launcher" -ForegroundColor Green
        $testResults += @{Test="Launcher: $launcher"; Result="PASS"}
    } else {
        Write-Host "  ❌ $launcher NOT FOUND" -ForegroundColor Red
        $testResults += @{Test="Launcher: $launcher"; Result="FAIL"}
    }
}

# Test 5: Check Documentation
Write-Host "`n[TEST 5/8] Checking Documentation..." -ForegroundColor Yellow
$docs = @(
    "docs\WINDOWS_DEPLOYMENT.md",
    "docs\PRODUCTION_DEPLOYMENT.md",
    "scripts\windows\README.md",
    "WINDOWS_DEPLOYMENT_SUMMARY.md"
)

foreach ($doc in $docs) {
    if (Test-Path $doc) {
        Write-Host "  ✅ $doc" -ForegroundColor Green
        $testResults += @{Test="Doc: $doc"; Result="PASS"}
    } else {
        Write-Host "  ❌ $doc NOT FOUND" -ForegroundColor Red
        $testResults += @{Test="Doc: $doc"; Result="FAIL"}
    }
}

# Test 6: Check Environment Templates
Write-Host "`n[TEST 6/8] Checking Environment Templates..." -ForegroundColor Yellow
$envFiles = @(
    ".env.example",
    ".env.production.example"
)

foreach ($env in $envFiles) {
    if (Test-Path $env) {
        Write-Host "  ✅ $env" -ForegroundColor Green
        $testResults += @{Test="Env: $env"; Result="PASS"}
    } else {
        Write-Host "  ❌ $env NOT FOUND" -ForegroundColor Red
        $testResults += @{Test="Env: $env"; Result="FAIL"}
    }
}

# Test 7: Validate PowerShell Syntax
Write-Host "`n[TEST 7/8] Validating PowerShell Syntax..." -ForegroundColor Yellow
foreach ($script in $requiredScripts) {
    if (Test-Path $script) {
        try {
            $null = [System.Management.Automation.PSParser]::Tokenize((Get-Content $script -Raw), [ref]$null)
            Write-Host "  ✅ $script - Syntax OK" -ForegroundColor Green
            $testResults += @{Test="Syntax: $script"; Result="PASS"}
        } catch {
            Write-Host "  ❌ $script - Syntax ERROR: $($_.Exception.Message)" -ForegroundColor Red
            $testResults += @{Test="Syntax: $script"; Result="FAIL"}
        }
    }
}

# Test 8: Check Docker Desktop
Write-Host "`n[TEST 8/8] Checking Docker Desktop..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Docker installed: $dockerVersion" -ForegroundColor Green
        $testResults += @{Test="Docker"; Result="PASS"}
    } else {
        Write-Host "  ⚠️  Docker not available - install required for deployment" -ForegroundColor Yellow
        $testResults += @{Test="Docker"; Result="WARN"}
    }
} catch {
    Write-Host "  ⚠️  Docker not installed - required for deployment" -ForegroundColor Yellow
    $testResults += @{Test="Docker"; Result="WARN"}
}

# Summary
Write-Host "`n╔══════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           Test Summary                   ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$passed = ($testResults | Where-Object {$_.Result -eq "PASS"}).Count
$failed = ($testResults | Where-Object {$_.Result -eq "FAIL"}).Count
$warnings = ($testResults | Where-Object {$_.Result -eq "WARN"}).Count
$total = $testResults.Count

Write-Host "Total Tests: $total" -ForegroundColor Cyan
Write-Host "  ✅ Passed: $passed" -ForegroundColor Green
Write-Host "  ❌ Failed: $failed" -ForegroundColor Red
Write-Host "  ⚠️  Warnings: $warnings" -ForegroundColor Yellow

Write-Host ""

if ($failed -eq 0) {
    Write-Host "╔══════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║   ✅ ALL TESTS PASSED - READY TO DEPLOY  ║" -ForegroundColor Green
    Write-Host "╚══════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Generate secrets: powershell -File scripts\windows\generate-secrets.ps1" -ForegroundColor Gray
    Write-Host "  2. Update nginx-lan.conf with your LAN IP" -ForegroundColor Gray
    Write-Host "  3. Run deployment: deploy.bat -SkipSSL" -ForegroundColor Gray
    exit 0
} else {
    Write-Host "╔══════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║   ❌ TESTS FAILED - FIX ERRORS FIRST     ║" -ForegroundColor Red
    Write-Host "╚══════════════════════════════════════════╝" -ForegroundColor Red
    exit 1
}

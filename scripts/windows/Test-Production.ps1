# Test-Production.ps1
# Production Service Tests fuer Windows 11 Server
# Testet alle laufenden Services

param(
    [string]$BaseUrl = "http://localhost",
    [switch]$Verbose
)

$ErrorActionPreference = "Continue"

# Farben
function Write-Success {
    param([string]$Message)
    Write-Host "[PASS] $Message" -ForegroundColor Green
}

function Write-Fail {
    param([string]$Message)
    Write-Host "[FAIL] $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Write-Test {
    param([string]$Message)
    Write-Host "[TEST] $Message" -ForegroundColor Yellow
}

# Header
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   Production Service Tests              " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$totalTests = 0
$passedTests = 0
$failedTests = 0

# Test Helper
function Test-Service {
    param(
        [string]$Name,
        [string]$Url,
        [int]$ExpectedStatus = 200
    )
    
    $script:totalTests++
    Write-Test "$Name"
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        
        if ($response.StatusCode -eq $ExpectedStatus) {
            Write-Success "$Name - HTTP $($response.StatusCode)"
            $script:passedTests++
            return $true
        } else {
            Write-Fail "$Name - Expected $ExpectedStatus, got $($response.StatusCode)"
            $script:failedTests++
            return $false
        }
    } catch {
        Write-Fail "$Name - $($_.Exception.Message)"
        $script:failedTests++
        return $false
    }
}

function Test-Docker {
    param([string]$ContainerName)
    
    $script:totalTests++
    Write-Test "Docker: $ContainerName"
    
    try {
        $status = docker inspect --format='{{.State.Status}}' $ContainerName 2>$null
        
        if ($status -eq "running") {
            Write-Success "$ContainerName is running"
            $script:passedTests++
            return $true
        } else {
            Write-Fail "$ContainerName is $status"
            $script:failedTests++
            return $false
        }
    } catch {
        Write-Fail "$ContainerName not found"
        $script:failedTests++
        return $false
    }
}

# Test 1: Docker Containers
Write-Host "`n=== DOCKER CONTAINER TESTS ===" -ForegroundColor Yellow
Write-Host ""

Test-Docker "messenger_postgres"
Test-Docker "messenger_redis"
Test-Docker "messenger_rabbitmq"
Test-Docker "messenger_auth_service"
Test-Docker "messenger_user_service"
Test-Docker "messenger_message_service"

# Test 2: Service Health Checks
Write-Host "`n=== SERVICE HEALTH TESTS ===" -ForegroundColor Yellow
Write-Host ""

Test-Service "Auth Service Health" "http://localhost:5001/health"
Test-Service "User Service Health" "http://localhost:5003/health"

# Test 3: Database Connectivity
Write-Host "`n=== DATABASE TESTS ===" -ForegroundColor Yellow
Write-Host ""

$totalTests++
Write-Test "PostgreSQL Databases"

try {
    $dbs = docker exec messenger_postgres psql -U messenger_admin -d postgres -t -c "\l" 2>$null
    
    if ($dbs -match "messenger_auth" -and $dbs -match "messenger_messages") {
        Write-Success "All required databases exist"
        $passedTests++
    } else {
        Write-Fail "Some databases missing"
        $failedTests++
    }
} catch {
    Write-Fail "Database check failed: $($_.Exception.Message)"
    $failedTests++
}

# Test 4: Redis Connectivity
Write-Host "`n=== REDIS TESTS ===" -ForegroundColor Yellow
Write-Host ""

$totalTests++
Write-Test "Redis PING"

try {
    # Get Redis password from docker inspect
    $redisCmd = docker inspect messenger_redis --format='{{.Config.Cmd}}' 2>$null | Out-String
    
    if ($redisCmd -match '--requirepass\s+(\S+)') {
        $redisPassword = $matches[1].Trim(']')
        $ping = docker exec messenger_redis redis-cli -a "$redisPassword" --no-auth-warning PING 2>&1 | Select-Object -Last 1
    } else {
        $ping = docker exec messenger_redis redis-cli PING 2>&1 | Select-Object -Last 1
    }
    
    if ($ping -match "PONG") {
        Write-Success "Redis responding"
        $passedTests++
    } else {
        Write-Fail "Redis not responding (Response: $ping)"
        $failedTests++
    }
} catch {
    Write-Fail "Redis check failed: $($_.Exception.Message)"
    $failedTests++
}

# Test 5: RabbitMQ
Write-Host "`n=== RABBITMQ TESTS ===" -ForegroundColor Yellow
Write-Host ""

Test-Service "RabbitMQ Management" "http://localhost:15672" 200

# Test 6: API Endpoints (if verbose)
if ($Verbose) {
    Write-Host "`n=== API ENDPOINT TESTS ===" -ForegroundColor Yellow
    Write-Host ""
    
    # Test Registration (should fail without valid data, but endpoint should exist)
    $totalTests++
    Write-Test "Auth Registration Endpoint"
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5001/api/auth/register" `
            -Method POST `
            -ContentType "application/json" `
            -Body '{}' `
            -UseBasicParsing `
            -ErrorAction SilentlyContinue `
            -SkipHttpErrorCheck
        
        if ($response.StatusCode -in @(400, 200, 201)) {
            Write-Success "Registration endpoint accessible"
            $passedTests++
        } else {
            Write-Fail "Unexpected status: $($response.StatusCode)"
            $failedTests++
        }
    } catch {
        Write-Fail "Endpoint not accessible: $($_.Exception.Message)"
        $failedTests++
    }
}

# Summary
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "           TEST SUMMARY                   " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Total Tests:  $totalTests" -ForegroundColor White
Write-Host "Passed:       $passedTests" -ForegroundColor Green
Write-Host "Failed:       $failedTests" -ForegroundColor Red

$passRate = [math]::Round(($passedTests / $totalTests) * 100, 1)
Write-Host "Pass Rate:    $passRate%" -ForegroundColor $(if ($passRate -ge 80) { "Green" } elseif ($passRate -ge 50) { "Yellow" } else { "Red" })

Write-Host ""

if ($failedTests -eq 0) {
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host "   ALL TESTS PASSED!                     " -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Green
    exit 0
} else {
    Write-Host "==========================================" -ForegroundColor Red
    Write-Host "   SOME TESTS FAILED                     " -ForegroundColor Red
    Write-Host "==========================================" -ForegroundColor Red
    exit 1
}

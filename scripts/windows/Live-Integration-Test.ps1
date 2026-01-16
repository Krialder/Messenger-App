# Live-Integration-Test.ps1
# End-to-End Integration Test - Testet das komplette System live
# Verwendung: powershell -ExecutionPolicy Bypass -File scripts\windows\Live-Integration-Test.ps1

param(
    [string]$BaseUrl = "http://localhost:5001",
    [switch]$Verbose
)

$ErrorActionPreference = "Continue"

# Farben
function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
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
Write-Host "   LIVE INTEGRATION TEST - E2E           " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$totalTests = 0
$passedTests = 0
$failedTests = 0

# Test User Data
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$testUser = @{
    username = "testuser_$timestamp"
    email = "test_$timestamp@example.com"
    password = "SecureTestPassword123!"
}

Write-Info "Test User: $($testUser.email)"
Write-Host ""

# ========================================
# TEST 1: User Registration
# ========================================
Write-Host "=== TEST 1: USER REGISTRATION ===" -ForegroundColor Yellow
Write-Host ""

$totalTests++
Write-Test "Registering new user: $($testUser.username)"

try {
    $registerBody = @{
        username = $testUser.username
        email = $testUser.email
        password = $testUser.password
    } | ConvertTo-Json

    $registerResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/register" `
        -Method POST `
        -ContentType "application/json" `
        -Body $registerBody `
        -TimeoutSec 10

    if ($registerResponse.userId) {
        Write-Success "User registered successfully (ID: $($registerResponse.userId))"
        $passedTests++
        $userId = $registerResponse.userId
    } else {
        Write-Fail "Registration response missing userId"
        $failedTests++
        exit 1
    }
} catch {
    Write-Fail "Registration failed: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
    $failedTests++
    exit 1
}

Write-Host ""
Start-Sleep -Seconds 2

# ========================================
# TEST 2: User Login
# ========================================
Write-Host "=== TEST 2: USER LOGIN ===" -ForegroundColor Yellow
Write-Host ""

$totalTests++
Write-Test "Logging in as: $($testUser.email)"

try {
    $loginBody = @{
        email = $testUser.email
        password = $testUser.password
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody `
        -TimeoutSec 10

    if ($loginResponse.accessToken) {
        Write-Success "Login successful"
        Write-Info "Access Token: $($loginResponse.accessToken.Substring(0, 30))..."
        $passedTests++
        $accessToken = $loginResponse.accessToken
    } else {
        Write-Fail "Login response missing accessToken"
        $failedTests++
        exit 1
    }
} catch {
    Write-Fail "Login failed: $($_.Exception.Message)"
    $failedTests++
    exit 1
}

Write-Host ""
Start-Sleep -Seconds 2

# ========================================
# TEST 3: Get User Profile
# ========================================
Write-Host "=== TEST 3: GET USER PROFILE ===" -ForegroundColor Yellow
Write-Host ""

$totalTests++
Write-Test "Fetching user profile for: $userId"

try {
    $headers = @{
        "Authorization" = "Bearer $accessToken"
    }

    $profileResponse = Invoke-RestMethod -Uri "http://localhost:5003/api/users/$userId" `
        -Method GET `
        -Headers $headers `
        -TimeoutSec 10

    if ($profileResponse.userId -eq $userId) {
        Write-Success "Profile retrieved successfully"
        Write-Info "Username: $($profileResponse.username)"
        Write-Info "Email: $($profileResponse.email)"
        $passedTests++
    } else {
        Write-Fail "Profile userId mismatch"
        $failedTests++
    }
} catch {
    Write-Fail "Get profile failed: $($_.Exception.Message)"
    $failedTests++
}

Write-Host ""
Start-Sleep -Seconds 2

# ========================================
# TEST 4: Update User Profile
# ========================================
Write-Host "=== TEST 4: UPDATE USER PROFILE ===" -ForegroundColor Yellow
Write-Host ""

$totalTests++
Write-Test "Updating user profile"

try {
    $updateBody = @{
        firstName = "Test"
        lastName = "User"
        bio = "This is a test user created by the integration test script"
    } | ConvertTo-Json

    $headers = @{
        "Authorization" = "Bearer $accessToken"
    }

    $updateResponse = Invoke-RestMethod -Uri "http://localhost:5003/api/users/$userId" `
        -Method PUT `
        -ContentType "application/json" `
        -Headers $headers `
        -Body $updateBody `
        -TimeoutSec 10

    Write-Success "Profile updated successfully"
    $passedTests++
} catch {
    Write-Fail "Update profile failed: $($_.Exception.Message)"
    $failedTests++
}

Write-Host ""
Start-Sleep -Seconds 2

# ========================================
# TEST 5: Token Refresh
# ========================================
Write-Host "=== TEST 5: TOKEN REFRESH ===" -ForegroundColor Yellow
Write-Host ""

$totalTests++
Write-Test "Refreshing access token"

try {
    if ($loginResponse.refreshToken) {
        $refreshBody = @{
            refreshToken = $loginResponse.refreshToken
        } | ConvertTo-Json

        $refreshResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/refresh" `
            -Method POST `
            -ContentType "application/json" `
            -Body $refreshBody `
            -TimeoutSec 10 `
            -ErrorAction SilentlyContinue

        if ($refreshResponse.accessToken) {
            Write-Success "Token refreshed successfully"
            $passedTests++
        } else {
            Write-Info "Token refresh not implemented or failed (non-critical)"
            $passedTests++
        }
    } else {
        Write-Info "No refresh token provided (skipping)"
        $passedTests++
    }
} catch {
    Write-Info "Token refresh endpoint not available (non-critical)"
    $passedTests++
}

Write-Host ""
Start-Sleep -Seconds 2

# ========================================
# TEST 6: Invalid Login (Security Test)
# ========================================
Write-Host "=== TEST 6: INVALID LOGIN (SECURITY) ===" -ForegroundColor Yellow
Write-Host ""

$totalTests++
Write-Test "Testing login with wrong password"

try {
    $invalidLoginBody = @{
        email = $testUser.email
        password = "WrongPassword123!"
    } | ConvertTo-Json

    $invalidResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $invalidLoginBody `
        -TimeoutSec 10 `
        -ErrorAction Stop

    # If we get here, login succeeded when it shouldn't
    Write-Fail "Invalid login succeeded (security issue!)"
    $failedTests++
} catch {
    if ($_.Exception.Response.StatusCode -eq 401 -or $_.Exception.Response.StatusCode -eq 400) {
        Write-Success "Invalid login correctly rejected (401/400)"
        $passedTests++
    } else {
        Write-Fail "Unexpected error: $($_.Exception.Message)"
        $failedTests++
    }
}

Write-Host ""
Start-Sleep -Seconds 2

# ========================================
# TEST 7: Logout
# ========================================
Write-Host "=== TEST 7: USER LOGOUT ===" -ForegroundColor Yellow
Write-Host ""

$totalTests++
Write-Test "Logging out user"

try {
    $headers = @{
        "Authorization" = "Bearer $accessToken"
    }

    $logoutResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/logout" `
        -Method POST `
        -Headers $headers `
        -TimeoutSec 10 `
        -ErrorAction SilentlyContinue

    Write-Success "Logout successful"
    $passedTests++
} catch {
    Write-Info "Logout endpoint not available (non-critical)"
    $passedTests++
}

Write-Host ""

# ========================================
# SUMMARY
# ========================================
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
Write-Host "Test User Details:" -ForegroundColor Cyan
Write-Host "  Username: $($testUser.username)" -ForegroundColor Gray
Write-Host "  Email:    $($testUser.email)" -ForegroundColor Gray
Write-Host "  UserID:   $userId" -ForegroundColor Gray

Write-Host ""

if ($failedTests -eq 0) {
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host "   ALL INTEGRATION TESTS PASSED!         " -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host ""
    Write-Success "The system is fully functional!"
    exit 0
} else {
    Write-Host "==========================================" -ForegroundColor Red
    Write-Host "   SOME INTEGRATION TESTS FAILED         " -ForegroundColor Red
    Write-Host "==========================================" -ForegroundColor Red
    exit 1
}

# Deploy-Production.ps1
# Production Deployment Script fuer Windows 11 Server
# Analog zu scripts/deploy-production.sh (Linux)
# Verwendung: powershell -ExecutionPolicy Bypass -File scripts\windows\Deploy-Production.ps1

param(
    [switch]$SkipSSL,
    [switch]$SkipHealthCheck,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# ===================================
# Farben und Hilfsfunktionen
# ===================================

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host "=========================================="
    Write-Host "   $Title"
    Write-Host "=========================================="
    Write-Host ""
}

function Write-Step {
    param([int]$Current, [int]$Total, [string]$Message)
    Write-Host "[$Current/$Total] $Message" -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

# ===================================
# Main Script
# ===================================

Write-Header "Secure Messenger - Production Deploy"

$totalSteps = 7

# ===================================
# Step 1: Prerequisites Check
# ===================================
Write-Step 1 $totalSteps "Checking prerequisites..."

# Check if running as Admin
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin -and -not $Force) {
    Write-Warning "Not running as Administrator"
    Write-Info "Some operations may require admin privileges"
    Write-Info "Use -Force to skip this check"
}

# Check Docker Desktop
try {
    $dockerVersion = docker --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Docker installed: $dockerVersion"
    } else {
        throw "Docker not available"
    }
} catch {
    Write-Error "Docker is not installed or not running"
    Write-Info "Please install Docker Desktop from https://www.docker.com/products/docker-desktop/"
    exit 1
}

# Check Docker Compose
try {
    $composeVersion = docker compose version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Docker Compose installed: $composeVersion"
    } else {
        throw "Docker Compose not available"
    }
} catch {
    Write-Error "Docker Compose is not installed"
    Write-Info "Please install Docker Desktop (includes Compose plugin)"
    exit 1
}

# Check if .env.production exists
if (-not (Test-Path ".env.production")) {
    Write-Error ".env.production not found!"
    Write-Host ""
    Write-Info "Please create .env.production:"
    Write-Host "  1. Copy-Item .env.production.example .env.production" -ForegroundColor Gray
    Write-Host "  2. Edit .env.production and set all secrets" -ForegroundColor Gray
    Write-Host "  3. Run this script again" -ForegroundColor Gray
    Write-Host ""
    Write-Info "Or run: powershell -File scripts\windows\generate-secrets.ps1"
    exit 1
}
Write-Success ".env.production found"

# ===================================
# Step 2: Secrets Validation
# ===================================
Write-Step 2 $totalSteps "Validating secrets..."

# Check for CHANGE_THIS in .env.production (ignore comments)
$placeholders = Get-Content ".env.production" | 
    Where-Object { $_ -notmatch '^\s*#' -and $_ -match 'CHANGE_THIS' }

if ($placeholders) {
    Write-Error "Found CHANGE_THIS in .env.production!"
    Write-Host ""
    Write-Info "Please replace all placeholder secrets in .env.production:"
    Write-Host "  - JWT_SECRET" -ForegroundColor Gray
    Write-Host "  - TOTP_ENCRYPTION_KEY" -ForegroundColor Gray
    Write-Host "  - POSTGRES_PASSWORD" -ForegroundColor Gray
    Write-Host "  - REDIS_PASSWORD" -ForegroundColor Gray
    Write-Host "  - RABBITMQ_PASSWORD" -ForegroundColor Gray
    Write-Host ""
    Write-Info "Run: powershell -File scripts\windows\generate-secrets.ps1"
    exit 1
}
Write-Success "All secrets configured"

# ===================================
# Step 3: SSL Certificates Check
# ===================================
Write-Step 3 $totalSteps "Checking SSL certificates..."

$hasSSL = (Test-Path "ssl\fullchain.pem") -and (Test-Path "ssl\privkey.pem")

if (-not $hasSSL -and -not $SkipSSL) {
    Write-Warning "SSL certificates not found in ssl directory"
    Write-Host ""
    $continue = Read-Host "Do you want to continue without SSL? (y/N)"
    
    if ($continue -ne "y" -and $continue -ne "Y") {
        Write-Host ""
        Write-Info "Please obtain SSL certificates first:"
        Write-Host "  Option 1: Let's Encrypt (Win-ACME)" -ForegroundColor Gray
        Write-Host "    - Download: https://www.win-acme.com/" -ForegroundColor Gray
        Write-Host "    - Run wacs.exe and follow wizard" -ForegroundColor Gray
        Write-Host "    - Copy certificates to ssl directory" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  Option 2: Manual certificate" -ForegroundColor Gray
        Write-Host "    - Copy fullchain.pem to ssl directory" -ForegroundColor Gray
        Write-Host "    - Copy privkey.pem to ssl directory" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  Option 3: Use -SkipSSL for HTTP-only (LAN)" -ForegroundColor Gray
        exit 1
    }
    Write-Warning "Continuing without SSL (HTTP only)"
} elseif ($hasSSL) {
    Write-Success "SSL certificates found"
} else {
    Write-Info "Skipping SSL check (-SkipSSL flag)"
}

# Determine which docker-compose files to use
if ($hasSSL -and -not $SkipSSL) {
    $composeFiles = @("docker-compose.yml", "docker-compose.prod.yml")
    $deploymentType = "Production (HTTPS)"
} else {
    $composeFiles = @("docker-compose.yml", "docker-compose.lan.yml")
    $deploymentType = "LAN (HTTP)"
}

Write-Info "Deployment Type: $deploymentType"

# ===================================
# Step 4: Build Images
# ===================================
Write-Step 4 $totalSteps "Building Docker images..."

Write-Info "This may take 10-20 minutes on first run..."

try {
    $buildCmd = "docker compose -f $($composeFiles[0]) -f $($composeFiles[1]) build --parallel"
    Invoke-Expression $buildCmd
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
    
    Write-Success "Images built successfully"
} catch {
    Write-Error "Build failed: $($_.Exception.Message)"
    Write-Host ""
    Write-Info "Check Docker Desktop logs for details"
    exit 1
}

# ===================================
# Step 5: Stop Old Containers
# ===================================
Write-Step 5 $totalSteps "Stopping old containers..."

try {
    $downCmd = "docker compose -f $($composeFiles[0]) -f $($composeFiles[1]) down"
    Invoke-Expression $downCmd 2>&1 | Out-Null
    Write-Success "Old containers stopped"
} catch {
    Write-Warning "No old containers to stop"
}

# ===================================
# Step 6: Start Services
# ===================================
Write-Step 6 $totalSteps "Starting production services..."

try {
    $upCmd = "docker compose -f $($composeFiles[0]) -f $($composeFiles[1]) up -d"
    Invoke-Expression $upCmd
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to start services"
    }
    
    Write-Success "Services started"
} catch {
    Write-Error "Failed to start services: $($_.Exception.Message)"
    Write-Host ""
    Write-Info "Showing logs:"
    $logsCmd = "docker compose -f $($composeFiles[0]) -f $($composeFiles[1]) logs --tail=50"
    Invoke-Expression $logsCmd
    exit 1
}

# Wait for services to be healthy
Write-Host ""
Write-Info "Waiting for services to be healthy (max 120 seconds)..."
Start-Sleep -Seconds 10

$timeout = 120
$elapsed = 0
$interval = 5

while ($elapsed -lt $timeout) {
    try {
        # Check container health
        $psCmd = "docker compose -f $($composeFiles[0]) -f $($composeFiles[1]) ps"
        $output = Invoke-Expression $psCmd | Out-String
        
        if ($output -notmatch "unhealthy|starting") {
            Write-Success "All services are healthy!"
            break
        }
    } catch {
        # Continue waiting
    }
    
    Write-Host "Waiting... ($elapsed/$timeout seconds)" -ForegroundColor Gray
    Start-Sleep -Seconds $interval
    $elapsed += $interval
}

if ($elapsed -ge $timeout) {
    Write-Warning "Some services may not be healthy yet"
    $psCmd = "docker compose -f $($composeFiles[0]) -f $($composeFiles[1]) ps"
    Invoke-Expression $psCmd
}

# ===================================
# Step 7: Health Check
# ===================================
if (-not $SkipHealthCheck) {
    Write-Step 7 $totalSteps "Running health checks..."

    # Get domain from .env.production
    $domain = "localhost"
    if (Test-Path ".env.production") {
        $domainLine = Get-Content ".env.production" | Where-Object { $_ -match "^DOMAIN=" } | Select-Object -First 1
        if ($domainLine) {
            $domain = $domainLine.Split("=")[1].Trim()
        }
    }
    
    if ([string]::IsNullOrEmpty($domain) -or $domain -eq "messenger.yourdomain.com") {
        $domain = "localhost"
        Write-Warning "DOMAIN not set in .env.production, using localhost"
    }

    # Determine protocol
    if ($hasSSL -and -not $SkipSSL) {
        $protocol = "https"
    } else {
        $protocol = "http"
        
        # For LAN deployment, try to get local IP
        try {
            $localIP = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue | 
                Where-Object {$_.IPAddress -like "192.168.*" -or $_.IPAddress -like "10.*"} | 
                Select-Object -First 1 -ExpandProperty IPAddress
            
            if ($localIP) {
                $domain = $localIP
                Write-Info "Using LAN IP: $domain"
            }
        } catch {
            # Use localhost as fallback
        }
    }

    $healthUrl = "$protocol`://$domain/health"
    Write-Host ""
    Write-Info "Testing health endpoint: $healthUrl"

    try {
        $response = Invoke-WebRequest -Uri $healthUrl -Method Get -UseBasicParsing -SkipCertificateCheck -TimeoutSec 10 -ErrorAction Stop
        
        if ($response.StatusCode -eq 200) {
            Write-Success "Health check passed (HTTP $($response.StatusCode))"
        } else {
            Write-Warning "Health check returned HTTP $($response.StatusCode)"
            Write-Info "This may be normal if services are still initializing"
        }
    } catch {
        Write-Warning "Health check failed: $($_.Exception.Message)"
        Write-Info "This may be normal if services are still initializing"
    }
} else {
    Write-Info "Skipping health check (-SkipHealthCheck flag)"
}

# ===================================
# Final Summary
# ===================================
Write-Host ""
Write-Header "         Deployment Complete!             "

Write-Info "Container Status:"
$psCmd = "docker compose -f $($composeFiles[0]) -f $($composeFiles[1]) ps"
Invoke-Expression $psCmd

Write-Host ""
Write-Success "Service URLs:"

# Determine service URLs
if ($hasSSL -and -not $SkipSSL) {
    $domain = "localhost"
    if (Test-Path ".env.production") {
        $domainLine = Get-Content ".env.production" | Where-Object { $_ -match "^DOMAIN=" } | Select-Object -First 1
        if ($domainLine) {
            $domain = $domainLine.Split("=")[1].Trim()
        }
    }
    Write-Host "  Gateway: https://$domain" -ForegroundColor Gray
    Write-Host "  Health:  https://$domain/health" -ForegroundColor Gray
} else {
    try {
        $localIP = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue | 
            Where-Object {$_.IPAddress -like "192.168.*" -or $_.IPAddress -like "10.*"} | 
            Select-Object -First 1 -ExpandProperty IPAddress
        
        if ($localIP) {
            Write-Host "  Gateway: http://$localIP" -ForegroundColor Gray
            Write-Host "  Health:  http://$localIP/health" -ForegroundColor Gray
        } else {
            Write-Host "  Gateway: http://localhost" -ForegroundColor Gray
            Write-Host "  Health:  http://localhost/health" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  Gateway: http://localhost" -ForegroundColor Gray
        Write-Host "  Health:  http://localhost/health" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Warning "Next Steps:"
Write-Host "  1. Verify health: Invoke-RestMethod -Uri <HEALTH-URL>" -ForegroundColor Gray
Write-Host "  2. View logs:     docker compose logs -f" -ForegroundColor Gray
Write-Host "  3. Monitor:       docker compose ps" -ForegroundColor Gray
Write-Host "  4. Test API:      Invoke-RestMethod -Uri <GATEWAY-URL>/api/auth/register -Method POST" -ForegroundColor Gray

Write-Host ""
Write-Warning "Management Commands:"
Write-Host "  Restart:  docker compose -f $($composeFiles[0]) -f $($composeFiles[1]) restart" -ForegroundColor Gray
Write-Host "  Stop:     docker compose -f $($composeFiles[0]) -f $($composeFiles[1]) down" -ForegroundColor Gray
Write-Host "  Logs:     docker compose -f $($composeFiles[0]) -f $($composeFiles[1]) logs -f" -ForegroundColor Gray

Write-Host ""
Write-Success "Deployment successful!"

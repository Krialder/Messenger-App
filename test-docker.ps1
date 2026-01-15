# Test Docker Setup - Secure Messenger
# Führt umfassende Tests für alle Services aus

Write-Host "🧪 Docker Setup Tests" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan

# 1. Docker Status prüfen
Write-Host "`n1️⃣ Docker Status" -ForegroundColor Yellow
Write-Host "==================" -ForegroundColor Yellow

$containerStatus = docker-compose ps -q
if (-not $containerStatus) {
    Write-Host "❌ Keine Docker Container laufen!" -ForegroundColor Red
    Write-Host "🚀 Starte Setup: .\setup-docker.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Docker Container sind aktiv" -ForegroundColor Green

# 2. Service Connectivity Tests
Write-Host "`n2️⃣ Service Connectivity" -ForegroundColor Yellow
Write-Host "========================" -ForegroundColor Yellow

$services = @(
    @{Name="PostgreSQL"; Url=""; Port=5432; Type="tcp"},
    @{Name="Redis"; Url=""; Port=6379; Type="tcp"},
    @{Name="RabbitMQ"; Url=""; Port=5672; Type="tcp"},
    @{Name="API Gateway"; Url="http://localhost:5000/health"; Port=5000; Type="http"},
    @{Name="AuthService"; Url="http://localhost:5001/health"; Port=5001; Type="http"},
    @{Name="MessageService"; Url="http://localhost:5002/health"; Port=5002; Type="http"},
    @{Name="CryptoService"; Url="http://localhost:5003/health"; Port=5003; Type="http"},
    @{Name="NotificationService"; Url="http://localhost:5004/health"; Port=5004; Type="http"},
    @{Name="KeyManagementService"; Url="http://localhost:5005/health"; Port=5005; Type="http"},
    @{Name="UserService"; Url="http://localhost:5006/health"; Port=5006; Type="http"},
    @{Name="FileTransferService"; Url="http://localhost:5007/health"; Port=5007; Type="http"},
    @{Name="AuditLogService"; Url="http://localhost:5008/health"; Port=5008; Type="http"}
)

$passedTests = 0
$totalTests = $services.Count

foreach ($service in $services) {
    try {
        if ($service.Type -eq "http") {
            $response = Invoke-RestMethod -Uri $service.Url -Method Get -TimeoutSec 10
            if ($response.status -eq "Healthy") {
                Write-Host "✅ $($service.Name)" -ForegroundColor Green
                $passedTests++
            } else {
                Write-Host "⚠️  $($service.Name) - Status: $($response.status)" -ForegroundColor Yellow
            }
        } else {
            $connection = Test-NetConnection -ComputerName "localhost" -Port $service.Port -WarningAction SilentlyContinue
            if ($connection.TcpTestSucceeded) {
                Write-Host "✅ $($service.Name)" -ForegroundColor Green
                $passedTests++
            } else {
                Write-Host "❌ $($service.Name)" -ForegroundColor Red
            }
        }
    } catch {
        Write-Host "❌ $($service.Name) - Fehler: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# 3. API Funktionalitätstests
Write-Host "`n3️⃣ API Funktionalität" -ForegroundColor Yellow
Write-Host "======================" -ForegroundColor Yellow

# Test 1: Gateway verfügbar?
try {
    $gatewayResponse = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get -TimeoutSec 5
    Write-Host "✅ Gateway Health Check" -ForegroundColor Green
} catch {
    Write-Host "❌ Gateway Health Check" -ForegroundColor Red
}

# Test 2: Auth Service - Registration endpoint
try {
    $testUser = @{
        username = "dockertest_$(Get-Random)"
        email = "dockertest_$(Get-Random)@test.com"
        password = "TestPassword123!"
    } | ConvertTo-Json

    $authResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" -Method Post -Body $testUser -ContentType "application/json" -TimeoutSec 10
    Write-Host "✅ Auth Service - Registration Endpoint" -ForegroundColor Green
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✅ Auth Service - Registration Endpoint (400 = Validation aktiv)" -ForegroundColor Green
    } else {
        Write-Host "❌ Auth Service - Registration Endpoint: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 3: Message Service Verfügbarkeit
try {
    $messageResponse = Invoke-RestMethod -Uri "http://localhost:5002/health" -Method Get -TimeoutSec 5
    Write-Host "✅ Message Service Health" -ForegroundColor Green
} catch {
    Write-Host "❌ Message Service Health" -ForegroundColor Red
}

# Test 4: Crypto Service Verfügbarkeit
try {
    $cryptoResponse = Invoke-RestMethod -Uri "http://localhost:5003/health" -Method Get -TimeoutSec 5
    Write-Host "✅ Crypto Service Health" -ForegroundColor Green
} catch {
    Write-Host "❌ Crypto Service Health" -ForegroundColor Red
}

# 4. Database Tests
Write-Host "`n4️⃣ Database Connectivity" -ForegroundColor Yellow
Write-Host "=========================" -ForegroundColor Yellow

try {
    # Test PostgreSQL connection via docker
    $dbTest = docker exec messenger_postgres psql -U messenger_admin -d messenger_db -c "SELECT 1;" 2>$null
    if ($dbTest) {
        Write-Host "✅ PostgreSQL Connection" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL Connection" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ PostgreSQL Connection Test fehlgeschlagen" -ForegroundColor Red
}

try {
    # Test Redis connection
    $redisTest = docker exec messenger_redis redis-cli ping 2>$null
    if ($redisTest -eq "PONG") {
        Write-Host "✅ Redis Connection" -ForegroundColor Green
    } else {
        Write-Host "❌ Redis Connection" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Redis Connection Test fehlgeschlagen" -ForegroundColor Red
}

try {
    # Test RabbitMQ
    $rabbitTest = docker exec messenger_rabbitmq rabbitmq-diagnostics ping 2>$null
    if ($rabbitTest -match "Ping succeeded") {
        Write-Host "✅ RabbitMQ Connection" -ForegroundColor Green
    } else {
        Write-Host "❌ RabbitMQ Connection" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ RabbitMQ Connection Test fehlgeschlagen" -ForegroundColor Red
}

# 5. Zusammenfassung
Write-Host "`n📊 Test Zusammenfassung" -ForegroundColor Cyan
Write-Host "=======================" -ForegroundColor Cyan
Write-Host "Services getestet: $totalTests" -ForegroundColor White
Write-Host "Erfolgreich: $passedTests" -ForegroundColor Green
Write-Host "Fehlerhaft: $($totalTests - $passedTests)" -ForegroundColor Red

$successRate = [math]::Round(($passedTests / $totalTests) * 100, 1)
Write-Host "Erfolgsrate: $successRate%" -ForegroundColor $(if ($successRate -ge 80) { "Green" } elseif ($successRate -ge 60) { "Yellow" } else { "Red" })

if ($passedTests -eq $totalTests) {
    Write-Host "`n🎉 Alle Tests erfolgreich! Docker Setup ist bereit." -ForegroundColor Green
    Write-Host "🚀 Nächste Schritte:" -ForegroundColor Cyan
    Write-Host "   1. Frontend bauen: .\build-client.bat" -ForegroundColor White
    Write-Host "   2. Tests ausführen: .\test.bat" -ForegroundColor White
    Write-Host "   3. App starten: .\publish\MessengerClient\MessengerClient.exe" -ForegroundColor White
} elseif ($successRate -ge 80) {
    Write-Host "`n✅ Setup größtenteils erfolgreich ($successRate%)" -ForegroundColor Yellow
    Write-Host "⚠️  Prüfe die fehlerhaften Services und starte sie neu." -ForegroundColor Yellow
} else {
    Write-Host "`n❌ Kritische Fehler im Setup ($successRate%)" -ForegroundColor Red
    Write-Host "🔧 Empfohlene Lösung:" -ForegroundColor Yellow
    Write-Host "   1. docker-compose down" -ForegroundColor White
    Write-Host "   2. docker system prune -f" -ForegroundColor White
    Write-Host "   3. .\setup-docker.ps1" -ForegroundColor White
}

Write-Host "`n📋 Logs anzeigen:" -ForegroundColor Cyan
Write-Host "docker-compose logs -f [service-name]" -ForegroundColor White

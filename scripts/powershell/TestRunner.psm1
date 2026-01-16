# TestRunner.psm1 - Test-Funktionen für Secure Messenger

function Invoke-DockerTests {
    <#
    .SYNOPSIS
    Führt umfassende Docker/Service Tests aus
    #>
    
    Write-Host "🧪 Starte Docker Tests" -ForegroundColor Cyan
    Write-Host "======================" -ForegroundColor Cyan
    
    $results = @{
        ContainerCheck = $false
        PortCheck = $false
        HealthCheck = $false
        DatabaseCheck = $false
        OverallSuccess = $false
    }
    
    # 1. Container Status
    Write-Host "`n1️⃣ Container Status..." -ForegroundColor Yellow
    $containers = docker-compose ps -q
    if ($containers) {
        Write-Host "✅ Container laufen" -ForegroundColor Green
        $results.ContainerCheck = $true
    } else {
        Write-Host "❌ Keine Container aktiv" -ForegroundColor Red
        return $results
    }
    
    # 2. Port Checks
    Write-Host "`n2️⃣ Port Connectivity..." -ForegroundColor Yellow
    $ports = @(5000, 5001, 5002, 5003, 5432, 6379, 5672)
    $openPorts = 0
    foreach ($port in $ports) {
        $conn = Test-NetConnection -ComputerName "localhost" -Port $port -WarningAction SilentlyContinue -InformationLevel Quiet
        if ($conn) {
            Write-Host "  ✅ Port $port" -ForegroundColor Green
            $openPorts++
        } else {
            Write-Host "  ❌ Port $port" -ForegroundColor Red
        }
    }
    $results.PortCheck = ($openPorts -ge ($ports.Count * 0.8))
    
    # 3. Health Checks
    Write-Host "`n3️⃣ Service Health..." -ForegroundColor Yellow
    Import-Module "$PSScriptRoot\DockerSetup.psm1" -Force
    $healthResult = Test-ServiceHealth
    $results.HealthCheck = $healthResult.Success
    
    # 4. Database Tests
    Write-Host "`n4️⃣ Database Connectivity..." -ForegroundColor Yellow
    try {
        $pgTest = docker exec messenger_postgres psql -U messenger_admin -d messenger_db -c "SELECT 1;" 2>$null
        if ($pgTest) {
            Write-Host "  ✅ PostgreSQL" -ForegroundColor Green
        }
        
        $redisTest = docker exec messenger_redis redis-cli ping 2>$null
        if ($redisTest -eq "PONG") {
            Write-Host "  ✅ Redis" -ForegroundColor Green
        }
        
        $results.DatabaseCheck = $true
    } catch {
        Write-Host "  ❌ Database Tests fehlgeschlagen" -ForegroundColor Red
        $results.DatabaseCheck = $false
    }
    
    # Gesamtergebnis
    $results.OverallSuccess = $results.ContainerCheck -and 
                              $results.PortCheck -and 
                              $results.HealthCheck -and 
                              $results.DatabaseCheck
    
    Write-Host "`n📊 Test Zusammenfassung:" -ForegroundColor Cyan
    Write-Host "  Container:  $(if ($results.ContainerCheck) {'✅'} else {'❌'})" -ForegroundColor $(if ($results.ContainerCheck) {'Green'} else {'Red'})
    Write-Host "  Ports:      $(if ($results.PortCheck) {'✅'} else {'❌'})" -ForegroundColor $(if ($results.PortCheck) {'Green'} else {'Red'})
    Write-Host "  Health:     $(if ($results.HealthCheck) {'✅'} else {'❌'})" -ForegroundColor $(if ($results.HealthCheck) {'Green'} else {'Red'})
    Write-Host "  Database:   $(if ($results.DatabaseCheck) {'✅'} else {'❌'})" -ForegroundColor $(if ($results.DatabaseCheck) {'Green'} else {'Red'})
    
    if ($results.OverallSuccess) {
        Write-Host "`n🎉 Alle Tests bestanden!" -ForegroundColor Green
    } else {
        Write-Host "`n❌ Einige Tests fehlgeschlagen" -ForegroundColor Red
    }
    
    return $results
}

function Invoke-UnitTests {
    <#
    .SYNOPSIS
    Führt .NET Unit Tests aus
    #>
    param(
        [string]$ProjectRoot = (Get-Location).Path,
        [string]$Filter = "Category=Unit"
    )
    
    Write-Host "🧪 Führe Unit Tests aus..." -ForegroundColor Cyan
    
    $testProjects = Get-ChildItem -Path $ProjectRoot -Filter "*.Tests.csproj" -Recurse
    
    if ($testProjects.Count -eq 0) {
        Write-Host "⚠️  Keine Test-Projekte gefunden" -ForegroundColor Yellow
        return $true
    }
    
    Write-Host "📦 Gefundene Test-Projekte: $($testProjects.Count)" -ForegroundColor Yellow
    
    $allPassed = $true
    foreach ($project in $testProjects) {
        Write-Host "`n▶️  $($project.Name)" -ForegroundColor Cyan
        
        dotnet test $project.FullName --filter $Filter --verbosity minimal
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Tests fehlgeschlagen: $($project.Name)" -ForegroundColor Red
            $allPassed = $false
        } else {
            Write-Host "✅ Tests bestanden: $($project.Name)" -ForegroundColor Green
        }
    }
    
    return $allPassed
}

Export-ModuleMember -Function @(
    'Invoke-DockerTests',
    'Invoke-UnitTests'
)

# Development Setup ohne Docker für Secure Messenger
# Für Benutzer ohne Administrator-Rechte

Write-Host "🚀 Development Setup (ohne Docker)" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Für Benutzer ohne Admin-Rechte" -ForegroundColor Gray
Write-Host ""

# 1. .NET SDK prüfen
Write-Host "1️⃣ .NET SDK Status..." -ForegroundColor Yellow

try {
    $dotnetVersion = dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ .NET SDK gefunden: $dotnetVersion" -ForegroundColor Green
        
        # Prüfe ob .NET 9.0 verfügbar
        $dotnetInfo = dotnet --info 2>&1
        if ($dotnetInfo -like "*9.0*") {
            Write-Host "✅ .NET 9.0 verfügbar" -ForegroundColor Green
        } else {
            Write-Host "⚠️  .NET 9.0 nicht gefunden, aber $dotnetVersion funktioniert" -ForegroundColor Yellow
        }
    } else {
        Write-Host "❌ .NET SDK nicht gefunden" -ForegroundColor Red
        Write-Host "📥 Download: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
        return
    }
} catch {
    Write-Host "❌ .NET Check fehlgeschlagen: $($_.Exception.Message)" -ForegroundColor Red
    return
}

# 2. Projekt-Struktur prüfen
Write-Host "`n2️⃣ Projekt-Struktur..." -ForegroundColor Yellow

$requiredPaths = @(
    "src/Backend/AuthService",
    "src/Backend/MessageService", 
    "src/Backend/GatewayService",
    "src/Frontend/MessengerClient",
    "Messenger.sln"
)

$missingPaths = @()
foreach ($path in $requiredPaths) {
    if (Test-Path $path) {
        Write-Host "✅ $path" -ForegroundColor Green
    } else {
        Write-Host "❌ $path" -ForegroundColor Red
        $missingPaths += $path
    }
}

if ($missingPaths.Count -gt 0) {
    Write-Host "❌ Unvollständige Projekt-Struktur" -ForegroundColor Red
    return
}

# 3. Alternative Database - SQLite Setup
Write-Host "`n3️⃣ SQLite Database Setup..." -ForegroundColor Yellow

Write-Host "💡 Verwende SQLite statt PostgreSQL (kein Admin erforderlich)" -ForegroundColor Cyan

# SQLite Connection String für lokale Entwicklung
$sqliteConnectionString = "Data Source=messenger_dev.db"
Write-Host "📝 SQLite Connection: $sqliteConnectionString" -ForegroundColor Gray

# 4. Services die ohne Docker laufen können
Write-Host "`n4️⃣ Verfügbare Services für lokale Entwicklung..." -ForegroundColor Yellow

$localServices = @(
    @{Name="AuthService"; Path="src/Backend/AuthService"; Port=5001; Requirements="SQLite"},
    @{Name="MessageService"; Path="src/Backend/MessageService"; Port=5002; Requirements="SQLite + In-Memory Queue"},
    @{Name="UserService"; Path="src/Backend/UserService"; Port=5006; Requirements="SQLite"},
    @{Name="GatewayService"; Path="src/Backend/GatewayService"; Port=5000; Requirements="Keine"},
    @{Name="Frontend"; Path="src/Frontend/MessengerClient"; Port="N/A"; Requirements="Keine"}
)

foreach ($service in $localServices) {
    if (Test-Path $service.Path) {
        Write-Host "✅ $($service.Name) (Port: $($service.Port)) - $($service.Requirements)" -ForegroundColor Green
    } else {
        Write-Host "❌ $($service.Name) - Pfad nicht gefunden" -ForegroundColor Red
    }
}

# 5. Build Test
Write-Host "`n5️⃣ Build Test..." -ForegroundColor Yellow

Write-Host "🔨 Teste Solution Build..." -ForegroundColor Cyan
try {
    $buildOutput = dotnet build Messenger.sln --verbosity minimal 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Solution Build erfolgreich" -ForegroundColor Green
    } else {
        Write-Host "❌ Build Fehler:" -ForegroundColor Red
        Write-Host $buildOutput -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Build Test fehlgeschlagen: $($_.Exception.Message)" -ForegroundColor Red
}

# 6. Local Development Commands
Write-Host "`n6️⃣ Local Development Commands..." -ForegroundColor Yellow

Write-Host "🚀 Services einzeln starten:" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan

Write-Host "# AuthService (Terminal 1):" -ForegroundColor Yellow
Write-Host "cd src/Backend/AuthService" -ForegroundColor White
Write-Host "dotnet run" -ForegroundColor White
Write-Host ""

Write-Host "# GatewayService (Terminal 2):" -ForegroundColor Yellow  
Write-Host "cd src/Backend/GatewayService" -ForegroundColor White
Write-Host "dotnet run" -ForegroundColor White
Write-Host ""

Write-Host "# Frontend (Terminal 3):" -ForegroundColor Yellow
Write-Host "cd src/Frontend/MessengerClient" -ForegroundColor White
Write-Host "dotnet run" -ForegroundColor White
Write-Host ""

# 7. Development Scripts erstellen
Write-Host "📝 Erstelle Development Scripts..." -ForegroundColor Cyan

# AuthService Start Script
$authScript = @"
# Start AuthService für lokale Entwicklung
Write-Host "🔐 Starte AuthService..." -ForegroundColor Cyan
cd src/Backend/AuthService
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ConnectionStrings__DefaultConnection="Data Source=auth_dev.db"
dotnet run --urls "http://localhost:5001"
"@

$authScript | Out-File -FilePath "start-auth.ps1" -Encoding UTF8
Write-Host "✅ start-auth.ps1 erstellt" -ForegroundColor Green

# Gateway Start Script
$gatewayScript = @"
# Start GatewayService für lokale Entwicklung
Write-Host "🌐 Starte Gateway..." -ForegroundColor Cyan
cd src/Backend/GatewayService
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --urls "http://localhost:5000"
"@

$gatewayScript | Out-File -FilePath "start-gateway.ps1" -Encoding UTF8
Write-Host "✅ start-gateway.ps1 erstellt" -ForegroundColor Green

# Frontend Start Script
$frontendScript = @"
# Start Frontend für lokale Entwicklung
Write-Host "🖥️  Starte Frontend..." -ForegroundColor Cyan
cd src/Frontend/MessengerClient
dotnet run
"@

$frontendScript | Out-File -FilePath "start-frontend.ps1" -Encoding UTF8
Write-Host "✅ start-frontend.ps1 erstellt" -ForegroundColor Green

# 8. Zusammenfassung
Write-Host "`n📊 Development Setup (ohne Docker) bereit!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

Write-Host "`n🎯 Nächste Schritte:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan
Write-Host "1. Drei separate PowerShell-Fenster öffnen:" -ForegroundColor White
Write-Host "   • .\start-auth.ps1      (AuthService)" -ForegroundColor Yellow
Write-Host "   • .\start-gateway.ps1   (API Gateway)" -ForegroundColor Yellow
Write-Host "   • .\start-frontend.ps1  (Frontend)" -ForegroundColor Yellow
Write-Host ""
Write-Host "2. Services testen:" -ForegroundColor White
Write-Host "   • http://localhost:5001/health (AuthService)" -ForegroundColor Yellow
Write-Host "   • http://localhost:5000/health (Gateway)" -ForegroundColor Yellow
Write-Host ""
Write-Host "3. Frontend nutzen:" -ForegroundColor White
Write-Host "   • WPF App sollte automatisch öffnen" -ForegroundColor Yellow

Write-Host "`n💡 Vorteile ohne Docker:" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host "✅ Keine Admin-Rechte erforderlich" -ForegroundColor Green
Write-Host "✅ Schnellerer Start der Services" -ForegroundColor Green
Write-Host "✅ Einfacheres Debugging" -ForegroundColor Green
Write-Host "✅ Direkte .NET Code-Änderungen" -ForegroundColor Green

Write-Host "`n⚠️  Einschränkungen:" -ForegroundColor Yellow
Write-Host "===================" -ForegroundColor Yellow
Write-Host "• Kein PostgreSQL (SQLite stattdessen)" -ForegroundColor Yellow
Write-Host "• Kein RabbitMQ (In-Memory Queue)" -ForegroundColor Yellow
Write-Host "• Keine Redis (In-Memory Cache)" -ForegroundColor Yellow
Write-Host "• Reduzierte Microservice-Features" -ForegroundColor Yellow

Write-Host "`n🎉 Setup abgeschlossen!" -ForegroundColor Green

# Master Setup Launcher für Secure Messenger
# Automatisches Docker Desktop Setup und Projekt-Konfiguration

Write-Host "🚀 Secure Messenger - Master Setup" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Docker Pfad: C:\Program Files\Docker\Docker" -ForegroundColor Gray
Write-Host "Projekt: Secure Messenger (9 Microservices)" -ForegroundColor Gray
Write-Host ""

# Menü für Setup-Optionen
function Show-Menu {
    Write-Host "🎯 Setup Optionen:" -ForegroundColor Yellow
    Write-Host "==================" -ForegroundColor Yellow
    Write-Host "1. 🐳 Docker Desktop Status prüfen" -ForegroundColor White
    Write-Host "2. 🚀 Komplettes Setup (Empfohlen)" -ForegroundColor Green
    Write-Host "3. 🧪 Docker Setup testen" -ForegroundColor White
    Write-Host "4. 🔧 Docker Troubleshooting" -ForegroundColor White
    Write-Host "5. 📦 Frontend bauen" -ForegroundColor White
    Write-Host "6. 🛑 Services stoppen" -ForegroundColor White
    Write-Host "7. 📊 Status anzeigen" -ForegroundColor White
    Write-Host "8. 🧹 Docker bereinigen" -ForegroundColor White
    Write-Host "9. 📖 Dokumentation" -ForegroundColor White
    Write-Host "0. ❌ Beenden" -ForegroundColor Red
    Write-Host ""
}

function Wait-ForKey {
    param([string]$Message = "Drücke eine beliebige Taste um fortzufahren...")
    Write-Host $Message -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    Write-Host ""
}

do {
    Clear-Host
    Write-Host "🚀 Secure Messenger - Master Setup" -ForegroundColor Cyan
    Write-Host "====================================" -ForegroundColor Cyan
    
    Show-Menu
    $choice = Read-Host "Wähle eine Option (0-9)"
    
    switch ($choice) {
        "1" {
            Write-Host "`n🐳 Docker Status prüfen..." -ForegroundColor Cyan
            & ".\check-docker.ps1"
            Wait-ForKey
        }
        
        "2" {
            Write-Host "`n🚀 Komplettes Setup wird gestartet..." -ForegroundColor Green
            Write-Host "📋 Setup Reihenfolge:" -ForegroundColor Yellow
            Write-Host "   1. Docker Status prüfen" -ForegroundColor White
            Write-Host "   2. Docker Services starten" -ForegroundColor White
            Write-Host "   3. Health Checks ausführen" -ForegroundColor White
            Write-Host "   4. Frontend vorbereiten" -ForegroundColor White
            Write-Host ""
            
            $confirm = Read-Host "Komplettes Setup starten? (j/N)"
            if ($confirm -eq "j" -or $confirm -eq "J" -or $confirm -eq "ja") {
                # Schritt 1: Docker prüfen
                Write-Host "`n🔍 Schritt 1/4: Docker prüfen..." -ForegroundColor Yellow
                & ".\check-docker.ps1"
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "❌ Docker Setup fehlgeschlagen!" -ForegroundColor Red
                    Wait-ForKey
                    continue
                }
                
                # Schritt 2: Services starten
                Write-Host "`n🚀 Schritt 2/4: Services starten..." -ForegroundColor Yellow
                & ".\setup-docker.ps1"
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "❌ Service Setup fehlgeschlagen!" -ForegroundColor Red
                    Wait-ForKey
                    continue
                }
                
                # Schritt 3: Tests
                Write-Host "`n🧪 Schritt 3/4: Tests ausführen..." -ForegroundColor Yellow
                & ".\test-docker.ps1"
                
                # Schritt 4: Frontend vorbereiten
                Write-Host "`n📦 Schritt 4/4: Frontend vorbereiten..." -ForegroundColor Yellow
                if (Test-Path ".\build-client.bat") {
                    $buildFrontend = Read-Host "Frontend jetzt bauen? (j/N)"
                    if ($buildFrontend -eq "j" -or $buildFrontend -eq "J") {
                        & ".\build-client.bat"
                    }
                } else {
                    Write-Host "⚠️  build-client.bat nicht gefunden" -ForegroundColor Yellow
                }
                
                Write-Host "`n🎉 Komplettes Setup abgeschlossen!" -ForegroundColor Green
                Write-Host "🌐 Services verfügbar unter:" -ForegroundColor Cyan
                Write-Host "   http://localhost:5000 (API Gateway)" -ForegroundColor White
                Write-Host "   http://localhost:15672 (RabbitMQ Management)" -ForegroundColor White
                Write-Host ""
                Write-Host "📱 Frontend starten:" -ForegroundColor Cyan
                Write-Host "   .\publish\MessengerClient\MessengerClient.exe" -ForegroundColor White
                
                Wait-ForKey
            }
        }
        
        "3" {
            Write-Host "`n🧪 Docker Tests werden ausgeführt..." -ForegroundColor Cyan
            & ".\test-docker.ps1"
            Wait-ForKey
        }
        
        "4" {
            Write-Host "`n🔧 Docker Troubleshooting..." -ForegroundColor Cyan
            & ".\docker-troubleshoot.ps1"
            Wait-ForKey
        }
        
        "5" {
            Write-Host "`n📦 Frontend wird gebaut..." -ForegroundColor Cyan
            if (Test-Path ".\build-client.bat") {
                & ".\build-client.bat"
            } else {
                Write-Host "❌ build-client.bat nicht gefunden" -ForegroundColor Red
            }
            Wait-ForKey
        }
        
        "6" {
            Write-Host "`n🛑 Services werden gestoppt..." -ForegroundColor Cyan
            docker-compose down
            Write-Host "✅ Alle Services gestoppt" -ForegroundColor Green
            Wait-ForKey
        }
        
        "7" {
            Write-Host "`n📊 Service Status:" -ForegroundColor Cyan
            Write-Host "Container Status:" -ForegroundColor Yellow
            docker-compose ps
            
            Write-Host "`nPort Status:" -ForegroundColor Yellow
            $ports = @(5000, 5001, 5002, 5003, 5004, 5005, 5006, 5007, 5008, 5432, 6379, 5672)
            foreach ($port in $ports) {
                $connection = Test-NetConnection -ComputerName "localhost" -Port $port -WarningAction SilentlyContinue
                if ($connection.TcpTestSucceeded) {
                    Write-Host "✅ Port $port" -ForegroundColor Green
                } else {
                    Write-Host "❌ Port $port" -ForegroundColor Red
                }
            }
            Wait-ForKey
        }
        
        "8" {
            Write-Host "`n🧹 Docker Bereinigung..." -ForegroundColor Cyan
            Write-Host "⚠️  Dies entfernt alle ungenutzten Docker Objekte!" -ForegroundColor Yellow
            $confirm = Read-Host "Fortfahren? (j/N)"
            if ($confirm -eq "j" -or $confirm -eq "J") {
                Write-Host "🛑 Stoppe Services..." -ForegroundColor Yellow
                docker-compose down
                
                Write-Host "🧹 Bereinige Container..." -ForegroundColor Yellow
                docker container prune -f
                
                Write-Host "🧹 Bereinige Images..." -ForegroundColor Yellow
                docker image prune -f
                
                Write-Host "🧹 Bereinige Volumes..." -ForegroundColor Yellow
                docker volume prune -f
                
                Write-Host "🧹 Bereinige Netzwerke..." -ForegroundColor Yellow
                docker network prune -f
                
                Write-Host "✅ Docker Bereinigung abgeschlossen" -ForegroundColor Green
            }
            Wait-ForKey
        }
        
        "9" {
            Write-Host "`n📖 Dokumentation:" -ForegroundColor Cyan
            Write-Host "==================" -ForegroundColor Cyan
            
            if (Test-Path "README.md") {
                Write-Host "📄 README.md gefunden" -ForegroundColor Green
            }
            if (Test-Path "docker-compose.yml") {
                Write-Host "🐳 docker-compose.yml gefunden" -ForegroundColor Green
            }
            if (Test-Path ".env") {
                Write-Host "⚙️  .env Datei gefunden" -ForegroundColor Green
            }
            if (Test-Path ".env.example") {
                Write-Host "📝 .env.example gefunden" -ForegroundColor Green
            }
            
            Write-Host "`n🔗 Wichtige URLs:" -ForegroundColor Yellow
            Write-Host "Docker Desktop Docs: https://docs.docker.com/desktop/windows/" -ForegroundColor Blue
            Write-Host "Secure Messenger GitHub: https://github.com/Krialder/Messenger-App" -ForegroundColor Blue
            
            Write-Host "`n📋 Verfügbare Skripte:" -ForegroundColor Yellow
            $scripts = @(
                "check-docker.ps1",
                "setup-docker.ps1", 
                "test-docker.ps1",
                "docker-troubleshoot.ps1",
                "build-client.bat"
            )
            
            foreach ($script in $scripts) {
                if (Test-Path $script) {
                    Write-Host "✅ $script" -ForegroundColor Green
                } else {
                    Write-Host "❌ $script (fehlt)" -ForegroundColor Red
                }
            }
            
            Wait-ForKey
        }
        
        "0" {
            Write-Host "`n👋 Setup wird beendet..." -ForegroundColor Yellow
            break
        }
        
        default {
            Write-Host "`n❌ Ungültige Auswahl: $choice" -ForegroundColor Red
            Wait-ForKey "Drücke eine beliebige Taste um fortzufahren..."
        }
    }
} while ($true)

Write-Host "`n✅ Master Setup beendet!" -ForegroundColor Green
Write-Host "📚 Für Hilfe: .\docker-troubleshoot.ps1" -ForegroundColor Cyan

# Einfaches Setup für Secure Messenger - OHNE Syntax-Fehler
# Funktioniert garantiert in PowerShell

Write-Host "🚀 Secure Messenger - Einfaches Setup" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Funktion für Menü
function Show-MainMenu {
    Write-Host "🎯 Was möchtest du machen?" -ForegroundColor Yellow
    Write-Host "=========================" -ForegroundColor Yellow
    Write-Host "1. Docker prüfen" -ForegroundColor White
    Write-Host "2. Alles starten (Docker)" -ForegroundColor Green
    Write-Host "3. Services testen" -ForegroundColor White
    Write-Host "4. Services stoppen" -ForegroundColor White
    Write-Host "5. Status anzeigen" -ForegroundColor White
    Write-Host "6. Lokale Entwicklung (ohne Docker)" -ForegroundColor Cyan
    Write-Host "7. Beenden" -ForegroundColor Red
    Write-Host ""
}

# Hauptprogramm
$continue = $true

while ($continue) {
    Clear-Host
    Write-Host "🚀 Secure Messenger Setup" -ForegroundColor Cyan
    Write-Host "=========================" -ForegroundColor Cyan
    
    Show-MainMenu
    $choice = Read-Host "Wähle eine Option (1-7)"
    
    switch ($choice) {
        "1" {
            Write-Host "`n🔍 Docker Status wird geprüft..." -ForegroundColor Cyan
            
            # Einfacher Docker Check
            try {
                $dockerVersion = docker --version 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ Docker CLI verfügbar: $dockerVersion" -ForegroundColor Green
                } else {
                    Write-Host "❌ Docker CLI nicht verfügbar" -ForegroundColor Red
                    Write-Host "💡 Starte Docker Desktop manuell" -ForegroundColor Yellow
                }
            }
            catch {
                Write-Host "❌ Docker nicht gefunden" -ForegroundColor Red
            }
            
            try {
                $dockerInfo = docker info --format "{{.ServerVersion}}" 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ Docker Engine läuft" -ForegroundColor Green
                } else {
                    Write-Host "❌ Docker Engine läuft nicht" -ForegroundColor Red
                }
            }
            catch {
                Write-Host "❌ Docker Engine nicht erreichbar" -ForegroundColor Red
            }
            
            Read-Host "`nDrücke Enter um fortzufahren"
        }
        
        "2" {
            Write-Host "`n🚀 Starte alle Services..." -ForegroundColor Green
            Write-Host "⏳ Dies kann 2-5 Minuten dauern..." -ForegroundColor Yellow
            
            # .env Datei prüfen
            if (!(Test-Path ".env")) {
                if (Test-Path ".env.example") {
                    Copy-Item ".env.example" ".env"
                    Write-Host "✅ .env Datei erstellt" -ForegroundColor Green
                } else {
                    Write-Host "❌ .env.example nicht gefunden" -ForegroundColor Red
                    Read-Host "Drücke Enter um fortzufahren"
                    continue
                }
            }
            
            # Docker Compose starten
            Write-Host "🐳 Starte Docker Services..." -ForegroundColor Cyan
            docker-compose down 2>$null
            docker-compose up -d --build
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Services gestartet!" -ForegroundColor Green
                Write-Host "`n🌐 URLs:" -ForegroundColor Cyan
                Write-Host "API Gateway: http://localhost:5000" -ForegroundColor White
                Write-Host "RabbitMQ:    http://localhost:15672" -ForegroundColor White
                Write-Host "`n⏳ Warte 30 Sekunden auf Service-Start..." -ForegroundColor Yellow
                Start-Sleep -Seconds 30
            } else {
                Write-Host "❌ Service-Start fehlgeschlagen" -ForegroundColor Red
            }
            
            Read-Host "`nDrücke Enter um fortzufahren"
        }
        
        "3" {
            Write-Host "`n🧪 Teste Services..." -ForegroundColor Cyan
            
            $services = @(
                @{Name="Gateway"; Url="http://localhost:5000/health"},
                @{Name="AuthService"; Url="http://localhost:5001/health"},
                @{Name="MessageService"; Url="http://localhost:5002/health"},
                @{Name="CryptoService"; Url="http://localhost:5003/health"}
            )
            
            foreach ($service in $services) {
                try {
                    $response = Invoke-RestMethod -Uri $service.Url -Method Get -TimeoutSec 5
                    Write-Host "✅ $($service.Name)" -ForegroundColor Green
                }
                catch {
                    Write-Host "❌ $($service.Name)" -ForegroundColor Red
                }
            }
            
            Read-Host "`nDrücke Enter um fortzufahren"
        }
        
        "4" {
            Write-Host "`n🛑 Stoppe alle Services..." -ForegroundColor Yellow
            docker-compose down
            Write-Host "✅ Services gestoppt" -ForegroundColor Green
            Read-Host "`nDrücke Enter um fortzufahren"
        }
        
        "5" {
            Write-Host "`n📊 Service Status..." -ForegroundColor Cyan
            docker-compose ps
            Read-Host "`nDrücke Enter um fortzufahren"
        }
        
        "6" {
            Write-Host "`n💻 Lokale Entwicklung (ohne Docker)" -ForegroundColor Cyan
            Write-Host "====================================" -ForegroundColor Cyan
            Write-Host "Starte diese Services in separaten Terminals:" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Terminal 1: cd src/Backend/AuthService && dotnet run" -ForegroundColor White
            Write-Host "Terminal 2: cd src/Backend/GatewayService && dotnet run" -ForegroundColor White  
            Write-Host "Terminal 3: cd src/Frontend/MessengerClient && dotnet run" -ForegroundColor White
            Write-Host ""
            Write-Host "💡 Keine Docker/Admin-Rechte erforderlich!" -ForegroundColor Green
            Read-Host "`nDrücke Enter um fortzufahren"
        }
        
        "7" {
            Write-Host "`n👋 Setup beendet!" -ForegroundColor Yellow
            $continue = $false
        }
        
        default {
            Write-Host "`n❌ Ungültige Auswahl: $choice" -ForegroundColor Red
            Read-Host "Drücke Enter um fortzufahren"
        }
    }
}

Write-Host "`n✅ Auf Wiedersehen!" -ForegroundColor Green

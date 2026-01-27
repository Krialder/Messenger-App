# =====================================================
# Secure Messenger - Cleanup Script
# Bereinigt Docker-Ressourcen und stoppt Services
# =====================================================

param(
    [switch]$DeleteData,
    [switch]$Force
)

Write-Host ""
Write-Host "╔══════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Secure Messenger - Cleanup             ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Wechsle ins Projekt-Verzeichnis
cd "I:\Just_for_fun\Messenger"

# Warnung
if ($DeleteData -and -not $Force) {
    Write-Host "⚠️  WARNUNG: -DeleteData löscht alle Datenbanken und Volumes!" -ForegroundColor Red
    Write-Host "             Alle Benutzer, Nachrichten und Daten gehen verloren!" -ForegroundColor Red
    Write-Host ""
    $confirm = Read-Host "Fortfahren? (j/N)"
    
    if ($confirm -ne "j" -and $confirm -ne "J") {
        Write-Host "❌ Abgebrochen" -ForegroundColor Yellow
        exit 0
    }
}

# Stoppe Container
Write-Host "[1/4] Stoppe Container..." -ForegroundColor Yellow
if ($DeleteData) {
    docker-compose down -v
    Write-Host "✅ Container gestoppt und Volumes gelöscht" -ForegroundColor Green
} else {
    docker-compose down
    Write-Host "✅ Container gestoppt (Volumes bleiben erhalten)" -ForegroundColor Green
}

# Bereinige ungenutzte Images
Write-Host "`n[2/4] Bereinige Docker Images..." -ForegroundColor Yellow
docker image prune -f
Write-Host "✅ Ungenutzte Images gelöscht" -ForegroundColor Green

# Bereinige Networks
Write-Host "`n[3/4] Bereinige Networks..." -ForegroundColor Yellow
docker network prune -f
Write-Host "✅ Ungenutzte Networks gelöscht" -ForegroundColor Green

# System-Info
Write-Host "`n[4/4] System-Info..." -ForegroundColor Yellow
docker system df

Write-Host ""
Write-Host "╔══════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         Cleanup abgeschlossen!           ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

if ($DeleteData) {
    Write-Host "⚠️  Alle Daten wurden gelöscht!" -ForegroundColor Yellow
    Write-Host "   Beim nächsten Start werden neue Datenbanken erstellt." -ForegroundColor Yellow
} else {
    Write-Host "ℹ️  Daten bleiben erhalten" -ForegroundColor Cyan
    Write-Host "   Beim nächsten Start werden bestehende Datenbanken verwendet." -ForegroundColor Cyan
}

Write-Host ""
Write-Host "📋 Nächste Schritte:" -ForegroundColor Cyan
Write-Host "  Services starten:  .\quick-start.ps1" -ForegroundColor White
Write-Host "  Oder manuell:      docker-compose up -d" -ForegroundColor White
Write-Host ""

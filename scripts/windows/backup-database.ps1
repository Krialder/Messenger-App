# backup-database.ps1
# Automatisches Datenbank-Backup fuer Windows 11 Server
# Verwendung: powershell -ExecutionPolicy Bypass -File scripts\windows\backup-database.ps1

param(
    [string]$BackupDir = "C:\Server\Messenger\backups",
    [int]$RetentionDays = 30
)

$ErrorActionPreference = "Stop"

# Farben fuer Ausgabe
function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

# Header
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   Secure Messenger - Database Backup    " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Backup-Verzeichnis erstellen
if (-not (Test-Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir -Force | Out-Null
    Write-Success "Backup-Verzeichnis erstellt: $BackupDir"
}

# Timestamp
$DATE = Get-Date -Format "yyyyMMdd_HHmmss"
$sqlFile = Join-Path $BackupDir "postgres_$DATE.sql"
$zipFile = Join-Path $BackupDir "postgres_$DATE.zip"

Write-Info "Erstelle Datenbank-Backup..."

try {
    # PostgreSQL Backup via Docker
    docker exec messenger_postgres pg_dumpall -U messenger_prod | Out-File -Encoding UTF8 $sqlFile
    
    if (-not (Test-Path $sqlFile)) {
        throw "Backup-Datei wurde nicht erstellt"
    }
    
    $sqlSize = (Get-Item $sqlFile).Length / 1MB
    Write-Success "SQL Dump erstellt: $([math]::Round($sqlSize, 2)) MB"
    
    # Komprimieren
    Write-Info "Komprimiere Backup..."
    Compress-Archive -Path $sqlFile -DestinationPath $zipFile -Force
    
    # SQL-Datei loeschen (nur ZIP behalten)
    Remove-Item $sqlFile
    
    $zipSize = (Get-Item $zipFile).Length / 1MB
    $compression = [math]::Round((1 - ($zipSize / $sqlSize)) * 100, 1)
    Write-Success "Backup komprimiert: $([math]::Round($zipSize, 2)) MB (Kompression: $compression%)"
    
} catch {
    Write-Error "Backup fehlgeschlagen: $($_.Exception.Message)"
    
    # Cleanup bei Fehler
    if (Test-Path $sqlFile) {
        Remove-Item $sqlFile
    }
    if (Test-Path $zipFile) {
        Remove-Item $zipFile
    }
    
    exit 1
}

# Alte Backups loeschen
Write-Info "Bereinige alte Backups (aelter als $RetentionDays Tage)..."
$cutoffDate = (Get-Date).AddDays(-$RetentionDays)
$oldBackups = Get-ChildItem -Path $BackupDir -Filter "postgres_*.zip" -ErrorAction SilentlyContinue | 
    Where-Object { $_.LastWriteTime -lt $cutoffDate }

if ($oldBackups) {
    $oldBackups | ForEach-Object {
        Remove-Item $_.FullName
        Write-Info "Geloescht: $($_.Name)"
    }
    Write-Success "$($oldBackups.Count) alte Backup(s) geloescht"
} else {
    Write-Info "Keine alten Backups zum Loeschen"
}

# Zusammenfassung
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "          Backup erfolgreich!             " -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

Write-Info "Backup Details:"
Write-Host "  Datei: $zipFile" -ForegroundColor Gray
Write-Host "  Groesse: $([math]::Round($zipSize, 2)) MB" -ForegroundColor Gray
Write-Host "  Datum: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# Backup-Anzahl anzeigen
$totalBackups = (Get-ChildItem -Path $BackupDir -Filter "postgres_*.zip" -ErrorAction SilentlyContinue).Count
if ($totalBackups -gt 0) {
    $totalSize = (Get-ChildItem -Path $BackupDir -Filter "postgres_*.zip" | Measure-Object -Property Length -Sum).Sum / 1GB
    Write-Info "Gesamt: $totalBackups Backup(s), $([math]::Round($totalSize, 2)) GB"
}

Write-Host ""
Write-Success "Fertig!"

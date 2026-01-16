# generate-secrets.ps1
# Automatische Generierung von Production Secrets fuer Windows 11 Server
# Verwendung: powershell -ExecutionPolicy Bypass -File scripts\windows\generate-secrets.ps1

param(
    [string]$EnvFile = ".env.production",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Farben fuer Ausgabe
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

function Write-Success {
    param([string]$Message)
    Write-ColorOutput Green "[OK] $Message"
}

function Write-Warning {
    param([string]$Message)
    Write-ColorOutput Yellow "[WARN] $Message"
}

function Write-Error {
    param([string]$Message)
    Write-ColorOutput Red "[ERROR] $Message"
}

function Write-Info {
    param([string]$Message)
    Write-ColorOutput Cyan "[INFO] $Message"
}

# Funktion: Sicheres Secret generieren
function Generate-SecureSecret {
    param(
        [int]$Length = 64,
        [ValidateSet("Base64", "Alphanumeric")]
        [string]$Type = "Alphanumeric"
    )
    
    if ($Type -eq "Base64") {
        # Base64-codiertes Random (wie openssl rand -base64)
        $bytes = New-Object byte[] $Length
        [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($bytes)
        return [Convert]::ToBase64String($bytes).Substring(0, $Length)
    } else {
        # Alphanumerisch (Zahlen + Buchstaben)
        -join ((48..57) + (65..90) + (97..122) | Get-Random -Count $Length | ForEach-Object {[char]$_})
    }
}

# Header
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   Secure Messenger - Secret Generator   " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Pruefen ob .env.production existiert
if (Test-Path $EnvFile) {
    if (-not $Force) {
        Write-Warning "$EnvFile existiert bereits!"
        $confirm = Read-Host "Ueberschreiben? (j/N)"
        if ($confirm -ne "j" -and $confirm -ne "J") {
            Write-Info "Abgebrochen. Verwende -Force zum Ueberschreiben."
            exit 0
        }
    }
}

# Pruefen ob Template existiert
$templateFile = "$EnvFile.example"
if (-not (Test-Path $templateFile)) {
    Write-Error "$templateFile nicht gefunden!"
    Write-Info "Erstelle Template zuerst."
    exit 1
}

Write-Info "Generiere Secrets..."
Write-Host ""

# Secrets generieren
$secrets = @{
    "JWT_SECRET" = Generate-SecureSecret -Length 64 -Type Base64
    "TOTP_ENCRYPTION_KEY" = Generate-SecureSecret -Length 64 -Type Base64
    "POSTGRES_PASSWORD" = Generate-SecureSecret -Length 32 -Type Alphanumeric
    "REDIS_PASSWORD" = Generate-SecureSecret -Length 32 -Type Alphanumeric
    "RABBITMQ_PASSWORD" = Generate-SecureSecret -Length 32 -Type Alphanumeric
}

# Secrets anzeigen (erste 20 Zeichen)
Write-Info "Generierte Secrets (Preview):"
foreach ($key in $secrets.Keys) {
    $preview = $secrets[$key].Substring(0, [Math]::Min(20, $secrets[$key].Length))
    Write-Host "  $key = $preview..." -ForegroundColor Gray
}
Write-Host ""

# Template kopieren
Copy-Item $templateFile $EnvFile -Force
Write-Success "$EnvFile erstellt"

# Secrets in Datei ersetzen
foreach ($key in $secrets.Keys) {
    $value = $secrets[$key]
    
    # Regex fuer verschiedene Platzhalter-Patterns
    $patterns = @(
        "$key=CHANGE_THIS.*",
        "$key=.*CHANGE_THIS.*",
        "$key=.*"
    )
    
    $content = Get-Content $EnvFile -Raw
    
    foreach ($pattern in $patterns) {
        if ($content -match $pattern) {
            $content = $content -replace $pattern, "$key=$value"
            break
        }
    }
    
    Set-Content -Path $EnvFile -Value $content -NoNewline
    Write-Success "$key gesetzt"
}

Write-Host ""
Write-Success "Alle Secrets generiert!"
Write-Host ""

# Validierung
Write-Info "Validiere Konfiguration..."
$placeholders = Select-String -Path $EnvFile -Pattern "CHANGE_THIS" -ErrorAction SilentlyContinue

if ($placeholders) {
    Write-Warning "Noch Platzhalter gefunden:"
    $placeholders | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
    Write-Host ""
    Write-Warning "Bitte manuell anpassen!"
} else {
    Write-Success "Keine Platzhalter mehr vorhanden"
}

# LAN-IP ermitteln
Write-Host ""
Write-Info "Ermittle LAN-IP fuer nginx-lan.conf..."
try {
    $lanIP = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue | 
        Where-Object {$_.IPAddress -like "192.168.*" -or $_.IPAddress -like "10.*" -or $_.IPAddress -like "172.16.*"} | 
        Select-Object -First 1 -ExpandProperty IPAddress

    if ($lanIP) {
        Write-Success "LAN-IP gefunden: $lanIP"
        Write-Warning "WICHTIG: Trage diese IP in nginx/nginx-lan.conf ein!"
        Write-Host "  Zeile 41: server_name $lanIP localhost;" -ForegroundColor Yellow
    } else {
        Write-Warning "Keine LAN-IP gefunden. Manuell ermitteln!"
    }
} catch {
    Write-Warning "Konnte LAN-IP nicht ermitteln."
}

# Abschluss
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "        Secrets erfolgreich erstellt!     " -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

Write-Info "Naechste Schritte:"
Write-Host "  1. Backup erstellen: Copy-Item $EnvFile ${EnvFile}.backup" -ForegroundColor Gray
Write-Host "  2. LAN-IP in nginx/nginx-lan.conf anpassen" -ForegroundColor Gray
Write-Host "  3. Docker Services starten:" -ForegroundColor Gray
Write-Host "     docker-compose -f docker-compose.yml -f docker-compose.lan.yml up -d" -ForegroundColor Gray
Write-Host ""

# Sicherheitshinweis
Write-Warning "SICHERHEIT:"
Write-Host "  Niemals $EnvFile in Git committen!" -ForegroundColor Red
Write-Host "  Sichere Kopie ausserhalb des Repos erstellen!" -ForegroundColor Red
Write-Host ""

# Secret-Backup anbieten
$backup = Read-Host "Backup erstellen? (J/n)"
if ($backup -ne "n" -and $backup -ne "N") {
    $backupPath = "${EnvFile}.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item $EnvFile $backupPath
    Write-Success "Backup erstellt: $backupPath"
}

Write-Host ""
Write-Success "Fertig!"

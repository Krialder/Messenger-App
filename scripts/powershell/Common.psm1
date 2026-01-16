# Common.psm1 - Shared Utilities für Secure Messenger

function Write-Header {
    <#
    .SYNOPSIS
    Schreibt einen formatierten Header
    #>
    param([string]$Title)
    
    Write-Host ""
    Write-Host "===========================================" -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host "===========================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step {
    <#
    .SYNOPSIS
    Schreibt einen nummerierten Schritt
    #>
    param(
        [int]$Step,
        [int]$Total,
        [string]$Description
    )
    
    Write-Host "`n[$Step/$Total] $Description" -ForegroundColor Yellow
    Write-Host ("-" * 50) -ForegroundColor Gray
}

function Write-Success {
    <#
    .SYNOPSIS
    Schreibt eine Erfolgsmeldung
    #>
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Error {
    <#
    .SYNOPSIS
    Schreibt eine Fehlermeldung
    #>
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Warning {
    <#
    .SYNOPSIS
    Schreibt eine Warnung
    #>
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Info {
    <#
    .SYNOPSIS
    Schreibt eine Info-Meldung
    #>
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Confirm-Action {
    <#
    .SYNOPSIS
    Fragt den Benutzer um Bestätigung
    #>
    param(
        [string]$Message,
        [switch]$DefaultYes
    )
    
    $prompt = if ($DefaultYes) { "(J/n)" } else { "(j/N)" }
    $response = Read-Host "$Message $prompt"
    
    if ($DefaultYes) {
        return ($response -ne "n" -and $response -ne "N")
    } else {
        return ($response -eq "j" -or $response -eq "J")
    }
}

function Get-ScriptRoot {
    <#
    .SYNOPSIS
    Gibt das Stammverzeichnis der Scripts zurück
    #>
    return Split-Path -Parent $PSScriptRoot
}

Export-ModuleMember -Function @(
    'Write-Header',
    'Write-Step',
    'Write-Success',
    'Write-Error',
    'Write-Warning',
    'Write-Info',
    'Confirm-Action',
    'Get-ScriptRoot'
)

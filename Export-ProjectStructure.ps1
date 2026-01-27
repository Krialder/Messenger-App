<#
.SYNOPSIS
    Exportiert die Projektstruktur des Secure Messenger Workspace
.DESCRIPTION
    Erstellt eine übersichtliche Baumstruktur aller Projekte und Dateien
    - Filtert Build-Artefakte (bin, obj, .vs)
    - Zeigt nur relevante Dateien (.cs, .csproj, .xaml, .json, .md)
    - Exportiert als Text und Markdown
.EXAMPLE
    .\Export-ProjectStructure.ps1
.EXAMPLE
    .\Export-ProjectStructure.ps1 -IncludeAllFiles
.EXAMPLE
    .\Export-ProjectStructure.ps1 -MaxDepth 3
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$OutputPath = ".\PROJECT_STRUCTURE.md",
    
    [Parameter()]
    [int]$MaxDepth = 5,
    
    [Parameter()]
    [switch]$IncludeAllFiles
)

# Farben fuer Console-Output
$Colors = @{
    Header = "Cyan"
    Directory = "Yellow"
    File = "Gray"
    Success = "Green"
    Warning = "DarkYellow"
}

# Ausgeschlossene Verzeichnisse
$ExcludedDirs = @(
    '.vs', '.git', '.obsidian',
    'bin', 'obj', 'packages', 'node_modules',
    'TestResults', '.idea', 'publish'
)

# Relevante Dateierweiterungen
$RelevantExtensions = @(
    '.cs', '.csproj', '.sln', '.xaml',
    '.json', '.yml', '.yaml',
    '.md', '.txt', '.sql',
    '.ps1', '.bat', '.sh',
    '.config', '.env', '.example',
    '.dockerfile', '.dockerignore'
)

function Get-TreeStructure {
    param(
        [string]$Path,
        [int]$Depth = 0,
        [string]$Prefix = ""
    )
    
    if ($Depth -gt $MaxDepth) { return }
    
    $items = Get-ChildItem -Path $Path -Force -ErrorAction SilentlyContinue | Where-Object {
        $_.Name -notin $ExcludedDirs -and
        ((-not $_.Name.StartsWith('.')) -or
        $_.Name -in @('.env', '.env.example', '.editorconfig', '.gitignore', '.gitattributes'))
    }
    
    # Sortieren: Verzeichnisse zuerst, dann Dateien
    $dirs = $items | Where-Object { $_.PSIsContainer } | Sort-Object Name
    $files = $items | Where-Object { -not $_.PSIsContainer }
    
    # Dateien filtern (wenn nicht alle angezeigt werden sollen)
    if (-not $IncludeAllFiles) {
        $files = $files | Where-Object {
            $ext = $_.Extension.ToLower()
            $ext -in $RelevantExtensions -or $_.Name -match '\.(env|example|gitignore|gitattributes)$'
        }
    }
    
    $files = $files | Sort-Object Name
    $allItems = @($dirs) + @($files)
    
    $output = @()
    
    for ($i = 0; $i -lt $allItems.Count; $i++) {
        $item = $allItems[$i]
        $isLast = ($i -eq $allItems.Count - 1)
        
        # ASCII-kompatible Tree-Zeichen
        $connector = if ($isLast) { "+-- " } else { "|-- " }
        $newPrefix = if ($isLast) { "$Prefix    " } else { "$Prefix|   " }
        
        if ($item.PSIsContainer) {
            # Verzeichnis
            $output += "$Prefix$connector[DIR] $($item.Name)/"
            $output += Get-TreeStructure -Path $item.FullName -Depth ($Depth + 1) -Prefix $newPrefix
        }
        else {
            # Datei mit Marker
            $marker = switch ($item.Extension.ToLower()) {
                '.cs' { '[CS]' }
                '.csproj' { '[PROJ]' }
                '.sln' { '[SLN]' }
                '.xaml' { '[XAML]' }
                '.json' { '[JSON]' }
                '.yml' { '[YML]' }
                '.yaml' { '[YML]' }
                '.md' { '[MD]' }
                '.ps1' { '[PS1]' }
                '.bat' { '[BAT]' }
                '.sh' { '[SH]' }
                '.sql' { '[SQL]' }
                '.env' { '[ENV]' }
                '.example' { '[EX]' }
                default { '[FILE]' }
            }
            $output += "$Prefix$connector$marker $($item.Name)"
        }
    }
    
    return $output
}

# Header generieren
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$header = @"
# Secure Messenger - Projektstruktur

**Workspace:** I:\Just_for_fun\Messenger\
**Generiert:** $timestamp
**Version:** 9.2.2
**Branch:** master

---

## Solution-Uebersicht

**13 Projekte:**

### Backend Services (9)
- AuthService - Authentication, JWT, MFA (Port 5001)
- MessageService - Encrypted Messaging (Port 5002)
- UserService - User Profile Management (Port 5003)
- NotificationService - Push Notifications (Port 5004)
- CryptoService - Cryptographic Operations (Port 5005)
- KeyManagementService - Key Rotation (Port 5006)
- AuditLogService - DSGVO Audit Logs (Port 5007)
- FileTransferService - Secure File Sharing (Port 5008)
- GatewayService - API Gateway (Ocelot, Port 7001)

### Shared Libraries (2)
- MessengerContracts - DTOs & Interfaces
- MessengerCommon - Extensions & Helpers

### Frontend (1)
- MessengerClient - WPF Desktop App (MVVM, Material Design)

### Tests (1)
- MessengerTests - Unit & Integration Tests (173/178 passing)

---

## Projektstruktur

``````
Messenger/
"@

# Struktur generieren
Write-Host "`n[*] Analysiere Projektstruktur..." -ForegroundColor $Colors.Header
Write-Host "    Workspace: I:\Just_for_fun\Messenger\" -ForegroundColor $Colors.File
Write-Host "    Max Depth: $MaxDepth" -ForegroundColor $Colors.File
if ($IncludeAllFiles) {
    Write-Host "    Modus: Alle Dateien" -ForegroundColor $Colors.Warning
} else {
    Write-Host "    Modus: Nur relevante Dateien (.cs, .csproj, .xaml, etc.)" -ForegroundColor $Colors.File
}
Write-Host ""

$structure = Get-TreeStructure -Path "." -Depth 0

# Markdown-Datei erstellen
$footer = @"
``````

---

## Statistiken

"@

# Statistiken berechnen
Write-Host "[*] Berechne Statistiken..." -ForegroundColor $Colors.Header

$stats = @{
    TotalFiles = 0
    CSharpFiles = 0
    XamlFiles = 0
    JsonFiles = 0
    MarkdownFiles = 0
    Scripts = 0
}

Get-ChildItem -Recurse -File -ErrorAction SilentlyContinue | Where-Object {
    $_.DirectoryName -notmatch '\\(bin|obj|\.vs|\.git|node_modules|packages)\\' 
} | ForEach-Object {
    $stats.TotalFiles++
    switch ($_.Extension.ToLower()) {
        '.cs' { $stats.CSharpFiles++ }
        '.xaml' { $stats.XamlFiles++ }
        '.json' { $stats.JsonFiles++ }
        '.md' { $stats.MarkdownFiles++ }
        { $_ -in @('.ps1', '.bat', '.sh') } { $stats.Scripts++ }
    }
}

$statsText = @"
| Kategorie | Anzahl |
|-----------|--------|
| **Projekte** | 13 |
| **Dateien gesamt** | $($stats.TotalFiles) |
| **C# Dateien (.cs)** | $($stats.CSharpFiles) |
| **XAML Dateien** | $($stats.XamlFiles) |
| **JSON Dateien** | $($stats.JsonFiles) |
| **Markdown Dateien** | $($stats.MarkdownFiles) |
| **Scripts (PS1/BAT/SH)** | $($stats.Scripts) |

---

## Legende

| Marker | Bedeutung |
|--------|-----------|
| [DIR] | Verzeichnis |
| [CS] | C# Datei (.cs) |
| [PROJ] | Projekt-Datei (.csproj) |
| [SLN] | Solution-Datei (.sln) |
| [XAML] | XAML-Datei |
| [JSON] | JSON-Konfiguration |
| [YML] | Docker/YAML-Datei |
| [MD] | Markdown-Dokumentation |
| [PS1] | PowerShell-Script |
| [BAT] | Batch-Script |
| [SH] | Shell-Script (.sh) |
| [SQL] | SQL-Datei |
| [ENV] | Environment-Datei (.env) |
| [EX] | Example/Template-Datei |
| [FILE] | Sonstige Datei |

---

**Generiert mit:** Export-ProjectStructure.ps1
**Repository:** https://github.com/Krialder/Messenger-App
**Dokumentation:** WORKSPACE_OVERVIEW.md
"@

$markdown = $header + "`n" + ($structure -join "`n") + "`n" + $footer + $statsText

# Speichern
$markdown | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "[+] Struktur exportiert nach: $OutputPath" -ForegroundColor $Colors.Success

# Optional: Auch als reine Textdatei
$textPath = $OutputPath -replace '\.md$', '.txt'
$structure | Out-File -FilePath $textPath -Encoding UTF8
Write-Host "[+] Textversion exportiert nach: $textPath" -ForegroundColor $Colors.Success

# Statistiken anzeigen
Write-Host "`n[*] Statistiken:" -ForegroundColor $Colors.Header
Write-Host "    Dateien gesamt: $($stats.TotalFiles)" -ForegroundColor $Colors.File
Write-Host "    C# Dateien: $($stats.CSharpFiles)" -ForegroundColor $Colors.File
Write-Host "    XAML Dateien: $($stats.XamlFiles)" -ForegroundColor $Colors.File
Write-Host "    JSON Dateien: $($stats.JsonFiles)" -ForegroundColor $Colors.File
Write-Host "    Markdown Dateien: $($stats.MarkdownFiles)" -ForegroundColor $Colors.File
Write-Host "    Scripts: $($stats.Scripts)" -ForegroundColor $Colors.File
Write-Host "    Projekte: 13" -ForegroundColor $Colors.File

Write-Host "`n[+] Export abgeschlossen!" -ForegroundColor $Colors.Success
Write-Host "[*] Druecke eine Taste zum Beenden..." -ForegroundColor $Colors.File

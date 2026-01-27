# 🗄️ Datenbank-Migrationen - Anleitung

## ✅ **Automatisch (Empfohlen)**

Das `deploy-complete.ps1` Script wendet alle Migrationen automatisch an.

```powershell
.\deploy-complete.ps1
```

---

## 🔧 **Manuell**

Falls du Migrationen manuell anwenden möchtest:

### **Schritt 1: EF Core Tools installieren**

```powershell
# Prüfe ob bereits installiert
dotnet tool list -g

# Falls nicht installiert:
dotnet tool install --global dotnet-ef
```

### **Schritt 2: Migrationen anwenden**

```powershell
# AuthService
cd src\Backend\AuthService
dotnet ef database update
cd ..\..\..

# MessageService
cd src\Backend\MessageService
dotnet ef database update
cd ..\..\..

# UserService
cd src\Backend\UserService
dotnet ef database update
cd ..\..\..

# KeyManagementService
cd src\Backend\KeyManagementService
dotnet ef database update
cd ..\..\..

# FileTransferService
cd src\Backend\FileTransferService
dotnet ef database update
cd ..\..\..

# AuditLogService
cd src\Backend\AuditLogService
dotnet ef database update
cd ..\..\..
```

---

## 📊 **Datenbank-Struktur**

Nach erfolgreichen Migrationen werden folgende Datenbanken erstellt:

| Datenbank | Service | Tabellen |
|-----------|---------|----------|
| **messenger_auth** | AuthService | Users, RefreshTokens, MfaSettings, RecoveryCodes |
| **messenger_messages** | MessageService | Conversations, ConversationMembers, Messages |
| **messenger_users** | UserService | UserProfiles, Contacts |
| **messenger_keys** | KeyManagementService | EncryptionKeys |
| **messenger_files** | FileTransferService | FileMetadata |
| **messenger_audit** | AuditLogService | AuditLogs (JSONB) |

---

## ✅ **Validierung**

### **Option 1: Via Docker**

```powershell
# Verbinde zu PostgreSQL
docker exec -it messenger_postgres psql -U messenger_admin -d messenger_auth

# Liste alle Tabellen
\dt

# Zeige Users-Tabelle
\d users

# Beenden
\q
```

### **Option 2: Via Health Checks**

```powershell
# Prüfe ob Services mit Datenbank verbunden sind
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health

# Erwartete Ausgabe: "Healthy"
```

---

## 🐛 **Troubleshooting**

### **Fehler: "Unable to create an object of type 'DbContext'"**

**Lösung:**
```powershell
# Stelle sicher, dass du im richtigen Verzeichnis bist
cd src\Backend\AuthService

# Baue das Projekt
dotnet build

# Versuche Migration erneut
dotnet ef database update
```

---

### **Fehler: "A connection was successfully established..."**

**Ursache:** PostgreSQL Container läuft nicht

**Lösung:**
```powershell
# Prüfe Docker Container
docker ps -a

# Starte PostgreSQL
docker-compose up -d postgres

# Warte 10 Sekunden
Start-Sleep -Seconds 10

# Versuche Migration erneut
dotnet ef database update
```

---

### **Fehler: "No migrations were found"**

**Ursache:** Migration-Dateien fehlen

**Lösung:**
```powershell
# Prüfe ob Migrations-Ordner existiert
Test-Path "Data\Migrations"

# Falls nicht, erstelle neue Migration
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

### **Fehler: "Duplicate key value violates unique constraint"**

**Ursache:** Datenbank existiert bereits mit Daten

**Lösung A - Reset (⚠️ Löscht alle Daten):**
```powershell
# Stoppe Services
docker-compose down

# Lösche Volumes
docker-compose down -v

# Starte neu
docker-compose up -d

# Warte 60 Sekunden
Start-Sleep -Seconds 60

# Wende Migrationen an
dotnet ef database update
```

**Lösung B - Ignorieren:**
```powershell
# Wenn Fehler "already exists" ist, ist das normal
# Datenbank ist bereits aktuell
```

---

## 📋 **Migration Cheat Sheet**

```powershell
# Neue Migration erstellen
dotnet ef migrations add <MigrationName>

# Migration anwenden
dotnet ef database update

# Letzte Migration rückgängig machen
dotnet ef database update <VorherigerMigrationName>

# Migration entfernen (nur wenn noch nicht applied)
dotnet ef migrations remove

# SQL-Script generieren (ohne Ausführung)
dotnet ef migrations script

# Datenbank löschen
dotnet ef database drop

# Connection String anzeigen
dotnet ef dbcontext info
```

---

## 🎯 **Schnelltest**

```powershell
# Test: Registriere User über API
$user = @{
    username = "testuser"
    email = "test@example.com"
    password = "SecurePass123!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/auth/register" `
    -Method POST `
    -ContentType "application/json" `
    -Body $user

# Erwartete Ausgabe: User-Objekt mit userId
```

---

## ✅ **Erfolgskriterien**

Nach erfolgreichen Migrationen solltest du sehen:

- ✅ `dotnet ef database update` gibt keine Fehler
- ✅ `curl http://localhost:5001/health` gibt "Healthy"
- ✅ User-Registrierung über API funktioniert
- ✅ `docker exec messenger_postgres psql -U messenger_admin -d messenger_auth -c "\dt"` zeigt Tabellen

---

**Version:** 1.0  
**Letzte Aktualisierung:** 2026-01-23

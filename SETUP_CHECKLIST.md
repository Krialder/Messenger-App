# ✅ Setup-Checkliste - Secure Messenger

## 📋 Vor dem Start

### **1. Software installieren**

- [ ] **Docker Desktop** installiert
  - Download: https://www.docker.com/products/docker-desktop/
  - Version: 24.x oder höher
  - Status prüfen: `docker --version`

- [ ] **.NET 9.0 SDK** installiert
  - Download: https://dotnet.microsoft.com/download/dotnet/9.0
  - Status prüfen: `dotnet --version`

- [ ] **PowerShell** (Windows) oder **Bash** (Linux/Mac)
  - Windows: Bereits vorinstalliert
  - Status prüfen: `$PSVersionTable.PSVersion`

### **2. Repository klonen**

```powershell
# Klone Repository
git clone https://github.com/Krialder/Messenger-App.git
cd Messenger

# Prüfe ob alle Dateien vorhanden sind
Test-Path "docker-compose.yml"  # Sollte True sein
Test-Path ".env"                # Sollte True sein
Test-Path "Messenger.sln"       # Sollte True sein
```

### **3. Environment-Variablen prüfen**

```powershell
# Öffne .env Datei
notepad .env

# Prüfe folgende Variablen:
# ✅ POSTGRES_PASSWORD (sollte NICHT "CHANGE_THIS" sein)
# ✅ REDIS_PASSWORD
# ✅ JWT_SECRET (min. 64 Zeichen)
# ✅ TOTP_ENCRYPTION_KEY (min. 32 Zeichen)
```

**⚠️ WICHTIG für Production:**
```powershell
# Generiere sichere Secrets
openssl rand -base64 64

# Ersetze in .env:
# JWT_SECRET=<generierter-wert>
# TOTP_ENCRYPTION_KEY=<generierter-wert>
```

---

## 🚀 Quick Start (Automatisch)

### **Option 1: Quick Start Script (2-3 Minuten)**

```powershell
.\quick-start.ps1
```

**Was es macht:**
- ✅ Docker Desktop starten
- ✅ Docker Services starten
- ✅ Health Checks durchführen
- ✅ URLs anzeigen

**Perfekt für:** Schnelles Testen, Entwicklung

---

### **Option 2: Vollständiges Deployment (5-7 Minuten)**

```powershell
.\deploy-complete.ps1
```

**Was es macht:**
- ✅ Docker Desktop starten
- ✅ Environment validieren
- ✅ Docker Images bauen
- ✅ Services starten
- ✅ **Datenbank-Migrationen anwenden**
- ✅ Health Checks durchführen
- ✅ API Test (User registrieren)
- ✅ Detaillierte Zusammenfassung

**Perfekt für:** Production-Setup, erste Installation

---

## 🔧 Manuelle Installation

Falls du alle Schritte manuell ausführen möchtest:

### **Schritt 1: Docker Desktop starten**

```powershell
# Öffne Docker Desktop (manuell)
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"

# Warte 45 Sekunden
Start-Sleep -Seconds 45

# Prüfe Status
docker --version
docker info
```

### **Schritt 2: Services starten**

```powershell
# Stoppe alte Container
docker-compose down

# Starte Services
docker-compose up -d

# Warte 60 Sekunden
Start-Sleep -Seconds 60
```

### **Schritt 3: Container-Status prüfen**

```powershell
# Zeige alle Container
docker-compose ps

# Erwartete Ausgabe: Alle Container "Up (healthy)"
```

### **Schritt 4: Datenbank-Migrationen**

```powershell
# Installiere EF Core Tools
dotnet tool install --global dotnet-ef

# AuthService
cd src\Backend\AuthService
dotnet ef database update
cd ..\..\..

# MessageService
cd src\Backend\MessageService
dotnet ef database update
cd ..\..\..

# Weitere Services (siehe docs/DATABASE_MIGRATIONS.md)
```

### **Schritt 5: Health Checks**

```powershell
# Teste Services
curl http://localhost:5001/health  # AuthService
curl http://localhost:5002/health  # MessageService
curl http://localhost:7001/health  # Gateway

# Erwartete Ausgabe: "Healthy"
```

### **Schritt 6: API Test**

```powershell
# Registriere Test-User
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

## ✅ Validierung

### **1. Docker Container**

```powershell
docker-compose ps
```

**Erwartete Ausgabe:**
```
NAMES                               STATUS
messenger_postgres                  Up (healthy)
messenger_redis                     Up (healthy)
messenger_rabbitmq                  Up (healthy)
messenger_auth_service              Up (healthy)
messenger_message_service           Up (healthy)
messenger_user_service              Up (healthy)
messenger_gateway                   Up (healthy)
... (weitere Services)
```

### **2. Services erreichbar**

| Service | URL | Erwarteter Status |
|---------|-----|-------------------|
| AuthService | http://localhost:5001/health | Healthy |
| MessageService | http://localhost:5002/health | Healthy |
| UserService | http://localhost:5003/health | Healthy |
| Gateway | http://localhost:7001/health | Healthy |
| Swagger | http://localhost:5001/swagger | UI lädt |
| RabbitMQ | http://localhost:15672 | Login-Seite |

### **3. Datenbank**

```powershell
# Verbinde zu PostgreSQL
docker exec -it messenger_postgres psql -U messenger_admin -d messenger_auth

# Liste Tabellen
\dt

# Erwartete Ausgabe:
#  public | users
#  public | refresh_tokens
#  public | mfa_settings
#  public | recovery_codes
```

### **4. API funktioniert**

```powershell
# Test Registration
Invoke-RestMethod -Uri "http://localhost:5001/api/auth/register" -Method POST -Body $userJson

# Erwartete Ausgabe: User-Objekt (kein Fehler)
```

---

## 🌐 Nach dem Setup

### **URLs öffnen**

```
Swagger UI:    http://localhost:5001/swagger
Gateway:       http://localhost:7001
RabbitMQ UI:   http://localhost:15672
```

### **RabbitMQ Login**
- **User:** messenger
- **Password:** (siehe .env - RABBITMQ_PASSWORD)

### **Frontend starten**

```powershell
cd src\Frontend\MessengerClient
dotnet run
```

---

## 📋 Häufige Befehle

```powershell
# Services starten
docker-compose up -d

# Services stoppen
docker-compose down

# Logs anzeigen
docker-compose logs -f auth-service

# Container-Status
docker-compose ps

# Alle Services neustarten
docker-compose restart

# Einzelnen Service neustarten
docker-compose restart auth-service

# Datenbank löschen (⚠️ Löscht alle Daten!)
docker-compose down -v
```

---

## 🐛 Troubleshooting

### **Problem: Docker startet nicht**

```powershell
# Prüfe Docker-Prozess
Get-Process "Docker Desktop"

# Manuell starten
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"

# Warte 60 Sekunden
Start-Sleep -Seconds 60
```

### **Problem: Services nicht erreichbar**

```powershell
# Prüfe Container-Status
docker-compose ps

# Prüfe Logs
docker-compose logs auth-service

# Neustart
docker-compose restart auth-service
```

### **Problem: Migration schlägt fehl**

```powershell
# Prüfe PostgreSQL
docker-compose ps postgres

# Neustart PostgreSQL
docker-compose restart postgres

# Warte 10 Sekunden
Start-Sleep -Seconds 10

# Migration erneut
cd src\Backend\AuthService
dotnet ef database update
```

### **Problem: Port bereits belegt**

```powershell
# Prüfe welcher Prozess Port 5001 nutzt
netstat -ano | findstr "5001"

# Stoppe den Prozess oder ändere Port in docker-compose.yml
```

---

## ✅ Abschluss-Checkliste

- [ ] Docker Desktop läuft
- [ ] `docker-compose ps` zeigt alle Container als "healthy"
- [ ] `curl http://localhost:5001/health` gibt "Healthy"
- [ ] `curl http://localhost:5001/swagger` zeigt Swagger UI
- [ ] User-Registrierung über API funktioniert
- [ ] RabbitMQ UI erreichbar (http://localhost:15672)
- [ ] Frontend startet ohne Fehler (`dotnet run`)

**Wenn alle Punkte ✅ sind: SETUP ERFOLGREICH! 🎉**

---

## 📞 Hilfe

- **Dokumentation:** [docs/README.md](docs/README.md)
- **Datenbank:** [docs/DATABASE_MIGRATIONS.md](docs/DATABASE_MIGRATIONS.md)
- **Issues:** https://github.com/Krialder/Messenger-App/issues

---

**Version:** 1.0  
**Letzte Aktualisierung:** 2026-01-23  
**Status:** ✅ Production Ready

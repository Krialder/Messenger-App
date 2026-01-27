# 📜 PowerShell Scripts - Übersicht

## ✅ **Verfügbare Scripts**

| Script | Zweck | Dauer | Empfohlen für |
|--------|-------|-------|---------------|
| **quick-start.ps1** | Schneller Start ohne Migrationen | 2-3 min | Entwicklung, Testing |
| **deploy-complete.ps1** | Vollständiges Deployment mit Migrationen | 5-7 min | Erste Installation, Production |
| **cleanup.ps1** | Services stoppen und aufräumen | 1 min | Nach Entwicklung, Reset |

---

## 🚀 **1. quick-start.ps1**

### **Verwendung**

```powershell
.\quick-start.ps1
```

### **Was es macht**

1. ✅ Prüft Docker Desktop
2. ✅ Startet Docker (falls nicht aktiv)
3. ✅ Startet alle Docker Services
4. ✅ Wartet 60 Sekunden
5. ✅ Führt Health Checks durch
6. ✅ Zeigt URLs an

### **Ausgabe**

```
🚀 Secure Messenger - Quick Start
====================================

[1/5] Prüfe Docker...
✅ Docker OK

[2/5] Wechsle ins Projekt...
✅ Verzeichnis OK

[3/5] Starte Services...
✅ Services gestartet
⏳ Warte 60 Sekunden...

[4/5] Prüfe Services...
  ✅ AuthService
  ✅ Gateway

[5/5] Fertig!

🌐 URLs:
  Swagger:  http://localhost:5001/swagger
  Gateway:  http://localhost:7001

✅ Ready to use! 🚀
```

### **Wann verwenden?**

- ✅ Schnelles Testen
- ✅ Entwicklung
- ✅ Services sind bereits einmal deployed
- ✅ Datenbank existiert bereits

### **Wann NICHT verwenden?**

- ❌ Erste Installation
- ❌ Datenbank-Schema fehlt
- ❌ Nach `cleanup.ps1 -DeleteData`

---

## 🏗️ **2. deploy-complete.ps1**

### **Verwendung**

```powershell
# Standard (mit Migrationen)
.\deploy-complete.ps1

# Ohne Migrationen
.\deploy-complete.ps1 -SkipMigrations

# Mit Rebuild aller Images
.\deploy-complete.ps1 -RebuildImages

# Verbose-Ausgabe
.\deploy-complete.ps1 -Verbose

# Kombiniert
.\deploy-complete.ps1 -RebuildImages -Verbose
```

### **Was es macht**

1. ✅ Prüft Docker Desktop (startet automatisch falls nötig)
2. ✅ Validiert Projekt-Verzeichnis und Dateien
3. ✅ Prüft .env Konfiguration
4. ✅ Stoppt alte Container
5. ✅ Baut Docker Images (optional rebuild)
6. ✅ Startet alle Services
7. ✅ Wartet 60 Sekunden
8. ✅ Prüft Container-Status
9. ✅ **Wendet Datenbank-Migrationen an** (6 Services)
10. ✅ Führt Health Checks durch (9 Services)
11. ✅ Testet API (User-Registrierung)
12. ✅ Zeigt detaillierte Zusammenfassung

### **Ausgabe**

```
╔══════════════════════════════════════════════════╗
║   Secure Messenger - Automatisches Deployment   ║
║              Version 2.0 (2026-01-23)           ║
╚══════════════════════════════════════════════════╝

[1/10] Prüfe Docker Desktop...
✅ Docker CLI: Docker version 24.0.7
✅ Docker Engine läuft
✅ Docker Desktop vollständig verfügbar

[2/10] Prüfe Projekt-Verzeichnis...
✅ Verzeichnis: I:\Just_for_fun\Messenger
✅ Alle erforderlichen Dateien vorhanden

[3/10] Prüfe Environment-Konfiguration...
✅ .env Datei vorhanden
✅ Alle Environment-Variablen vorhanden

[4/10] Stoppe alte Container...
✅ Alte Container gestoppt

[5/10] Baue Docker Images...
✅ Docker Images erfolgreich gebaut

[6/10] Starte Docker Services...
✅ Services gestartet
✅ Services initialisiert

[7/10] Prüfe Container-Status...
Container Status:
=================
NAMES                    STATUS         PORTS
messenger_postgres       Up (healthy)   0.0.0.0:5432->5432/tcp
messenger_auth_service   Up (healthy)   0.0.0.0:5001->8080/tcp
...

[8/10] Wende Datenbank-Migrationen an...
  → AuthService...
    ✅ AuthService Migration OK
  → MessageService...
    ✅ MessageService Migration OK
  ...

[9/10] Führe Health Checks durch...
Service Health:
===============
  ✅ AuthService (Port 5001)
  ✅ MessageService (Port 5002)
  ...

ℹ️  Gesundheitsstatus: 9/9 Services (100%)
✅ Health Checks bestanden

[10/10] Teste API (Registrierung)...
✅ API Test erfolgreich
ℹ️  User ID: 123e4567-e89b-12d3-a456-426614174000

╔══════════════════════════════════════════════════╗
║           Deployment Zusammenfassung            ║
╚══════════════════════════════════════════════════╝

📊 Deployment-Status:
  Erfolgreiche Schritte: 10/10 (100%)

🌐 Service URLs:
  Swagger UI:       http://localhost:5001/swagger
  Auth Service:     http://localhost:5001
  Message Service:  http://localhost:5002
  Gateway:          http://localhost:7001
  RabbitMQ UI:      http://localhost:15672

✅ Deployment erfolgreich! Alle Systeme betriebsbereit! 🚀
```

### **Wann verwenden?**

- ✅ **Erste Installation**
- ✅ Production Deployment
- ✅ Nach Major Updates
- ✅ Nach `cleanup.ps1 -DeleteData`
- ✅ Vollständige Validierung benötigt

### **Parameter**

| Parameter | Beschreibung | Beispiel |
|-----------|--------------|----------|
| `-SkipMigrations` | Überspringt Datenbank-Migrationen | `.\deploy-complete.ps1 -SkipMigrations` |
| `-RebuildImages` | Baut alle Images neu (ohne Cache) | `.\deploy-complete.ps1 -RebuildImages` |
| `-Verbose` | Zeigt detaillierte Logs | `.\deploy-complete.ps1 -Verbose` |

---

## 🧹 **3. cleanup.ps1**

### **Verwendung**

```powershell
# Standard (behält Daten)
.\cleanup.ps1

# Löscht ALLE Daten (⚠️ GEFÄHRLICH!)
.\cleanup.ps1 -DeleteData

# Ohne Bestätigung
.\cleanup.ps1 -DeleteData -Force
```

### **Was es macht**

1. ✅ Stoppt alle Docker Container
2. ✅ Optional: Löscht alle Volumes (Datenbanken)
3. ✅ Bereinigt ungenutzte Images
4. ✅ Bereinigt ungenutzte Networks
5. ✅ Zeigt System-Info

### **Ausgabe (Standard)**

```
╔══════════════════════════════════════════╗
║   Secure Messenger - Cleanup             ║
╚══════════════════════════════════════════╝

[1/4] Stoppe Container...
✅ Container gestoppt (Volumes bleiben erhalten)

[2/4] Bereinige Docker Images...
✅ Ungenutzte Images gelöscht

[3/4] Bereinige Networks...
✅ Ungenutzte Networks gelöscht

[4/4] System-Info...
TYPE           TOTAL     ACTIVE    SIZE
Images         12        9         2.3GB
Containers     9         0         0B
Volumes        4         4         1.2GB

╔══════════════════════════════════════════╗
║         Cleanup abgeschlossen!           ║
╚══════════════════════════════════════════╝

ℹ️  Daten bleiben erhalten
   Beim nächsten Start werden bestehende Datenbanken verwendet.
```

### **Ausgabe (mit -DeleteData)**

```
⚠️  WARNUNG: -DeleteData löscht alle Datenbanken und Volumes!
             Alle Benutzer, Nachrichten und Daten gehen verloren!

Fortfahren? (j/N): j

[1/4] Stoppe Container...
✅ Container gestoppt und Volumes gelöscht

...

⚠️  Alle Daten wurden gelöscht!
   Beim nächsten Start werden neue Datenbanken erstellt.
```

### **Wann verwenden?**

| Szenario | Befehl | Effekt |
|----------|--------|--------|
| **Nach Entwicklung** | `.\cleanup.ps1` | Stoppt Services, behält Daten |
| **Kompletter Reset** | `.\cleanup.ps1 -DeleteData` | Löscht ALLES (⚠️) |
| **Disk Space** | `.\cleanup.ps1` | Bereinigt ungenutzte Images |
| **Vor Updates** | `.\cleanup.ps1` | Sauberer Zustand |

### **⚠️ WARNUNG: -DeleteData**

```powershell
# Löscht:
# - Alle PostgreSQL Datenbanken
# - Alle Redis Caches
# - Alle RabbitMQ Queues
# - Alle Benutzer, Nachrichten, Dateien

# Verwendet nur wenn:
# ✅ Du einen kompletten Neustart willst
# ✅ Du Testdaten löschen willst
# ✅ Du Disk Space freigeben willst

# NICHT verwenden wenn:
# ❌ Production-Daten vorhanden sind
# ❌ Backup fehlt
```

---

## 📋 **Typische Workflows**

### **Workflow 1: Erste Installation**

```powershell
# 1. Docker Desktop starten (manuell)

# 2. Vollständiges Deployment
.\deploy-complete.ps1

# 3. Frontend starten
cd src\Frontend\MessengerClient
dotnet run
```

---

### **Workflow 2: Tägliche Entwicklung**

```powershell
# Morgens: Services starten
.\quick-start.ps1

# Entwickeln...

# Abends: Services stoppen
.\cleanup.ps1
```

---

### **Workflow 3: Reset nach Tests**

```powershell
# Stoppe und lösche Testdaten
.\cleanup.ps1 -DeleteData

# Neues Deployment
.\deploy-complete.ps1

# Neue Testdaten erstellen
```

---

### **Workflow 4: Production Deployment**

```powershell
# 1. Secrets in .env anpassen
notepad .env

# 2. Vollständiges Deployment mit Rebuild
.\deploy-complete.ps1 -RebuildImages -Verbose

# 3. Logs überwachen
docker-compose logs -f

# 4. Health Checks
curl http://localhost:5001/health
```

---

## 🎯 **Schnellreferenz**

| Ziel | Befehl |
|------|--------|
| **Schnell starten** | `.\quick-start.ps1` |
| **Erste Installation** | `.\deploy-complete.ps1` |
| **Alles neu bauen** | `.\deploy-complete.ps1 -RebuildImages` |
| **Services stoppen** | `.\cleanup.ps1` |
| **Alles löschen** | `.\cleanup.ps1 -DeleteData` |
| **Logs anzeigen** | `docker-compose logs -f` |
| **Container prüfen** | `docker-compose ps` |

---

## 📞 Hilfe

- **Setup-Anleitung:** [SETUP_CHECKLIST.md](SETUP_CHECKLIST.md)
- **Datenbank:** [docs/DATABASE_MIGRATIONS.md](docs/DATABASE_MIGRATIONS.md)
- **Dokumentation:** [docs/README.md](docs/README.md)

---

**Version:** 1.0  
**Letzte Aktualisierung:** 2026-01-23  
**Status:** ✅ Production Ready

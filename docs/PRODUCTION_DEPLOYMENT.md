# 🚀 Production Deployment Guide - Secure Messenger

**Version**: 1.0  
**Status**: Production Ready  
**Last Updated**: 2025-01-16

---

## 📋 Voraussetzungen

### Server Requirements

| Ressource | Minimum | Empfohlen |
|-----------|---------|-----------|
| **CPU** | 2 Cores | 4+ Cores |
| **RAM** | 8 GB | 16+ GB |
| **Storage** | 50 GB SSD | 100+ GB SSD |
| **OS** | Ubuntu 22.04 | Ubuntu 22.04 LTS |
| **Network** | 100 Mbit/s | 1 Gbit/s |

### Software Requirements

- Docker 24.x oder neuer
- Docker Compose 2.x
- Domain mit DNS Zugriff
- SSL Zertifikat (Let's Encrypt empfohlen)

### Ports

| Port | Protokoll | Zweck | Firewall |
|------|-----------|-------|----------|
| 80 | TCP | HTTP → HTTPS Redirect | ✅ Offen |
| 443 | TCP | HTTPS (API + WebSocket) | ✅ Offen |
| 22 | TCP | SSH (Management) | ✅ Offen (IP-restricted) |

---

## 🔧 Schritt 1: Server Vorbereitung

### 1.1 System Update

```bash
# Als root oder mit sudo
sudo apt update && sudo apt upgrade -y
sudo reboot  # Falls Kernel-Updates
```

### 1.2 Docker Installation

```bash
# Docker installieren
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Docker Compose installieren
sudo apt install docker-compose-plugin -y

# User zu docker Gruppe hinzufügen
sudo usermod -aG docker $USER
newgrp docker

# Testen
docker --version
docker compose version
```

### 1.3 Firewall Konfiguration

```bash
# UFW Firewall einrichten
sudo ufw allow 22/tcp   # SSH
sudo ufw allow 80/tcp   # HTTP
sudo ufw allow 443/tcp  # HTTPS
sudo ufw enable

# Status prüfen
sudo ufw status
```

### 1.4 Fail2Ban (Optional aber empfohlen)

```bash
# Schutz gegen Brute-Force Angriffe
sudo apt install fail2ban -y
sudo systemctl enable fail2ban
sudo systemctl start fail2ban
```

---

## 📦 Schritt 2: Projekt Setup

### 2.1 Repository klonen

```bash
# Als normaler User (nicht root!)
cd /opt
sudo mkdir messenger && sudo chown $USER:$USER messenger
cd messenger

# Repository klonen
git clone https://github.com/Krialder/Messenger-App.git .

# Branch wechseln (falls nötig)
git checkout master
```

### 2.2 Production Environment erstellen

```bash
# .env.production aus Template erstellen
cp .env.production.example .env.production

# Jetzt BEARBEITEN! (siehe nächster Schritt)
nano .env.production
```

---

## 🔐 Schritt 3: Secrets Generieren

### Option A: Automatisches Script (Coming Soon)

```bash
./scripts/generate-production-secrets.sh
```

### Option B: Manuell

```bash
# JWT Secret (64 chars)
echo "JWT_SECRET=$(openssl rand -base64 64 | tr -d '\n')" >> .env.production

# TOTP Encryption Key (64 chars)
echo "TOTP_ENCRYPTION_KEY=$(openssl rand -base64 64 | tr -d '\n')" >> .env.production

# PostgreSQL Password (32 chars)
echo "POSTGRES_PASSWORD=$(openssl rand -base64 32 | tr -d '\n')" >> .env.production

# Redis Password (32 chars)
echo "REDIS_PASSWORD=$(openssl rand -base64 32 | tr -d '\n')" >> .env.production

# RabbitMQ Password (32 chars)
echo "RABBITMQ_PASSWORD=$(openssl rand -base64 32 | tr -d '\n')" >> .env.production
```

### Secrets validieren

```bash
# Prüfe, ob noch "CHANGE_THIS" vorkommt
cat .env.production | grep "CHANGE_THIS"
# Erwartete Ausgabe: NICHTS! (alle Secrets ersetzt)

# Datei-Permissions setzen
chmod 600 .env.production
```

---

## 🔒 Schritt 4: SSL Zertifikate

### 4.1 DNS konfigurieren

**Vor dem SSL-Setup:**

```
Domain: messenger.yourdomain.com
DNS A-Record: YOUR_SERVER_IP
```

Warte 5-10 Minuten, bis DNS propagiert ist:

```bash
# DNS testen
nslookup messenger.yourdomain.com
# Sollte deine Server-IP zeigen
```

### 4.2 Let's Encrypt Zertifikat

```bash
# Certbot installieren
sudo apt install certbot -y

# Zertifikat generieren (Standalone Mode)
sudo certbot certonly --standalone \
  -d messenger.yourdomain.com \
  --email admin@yourdomain.com \
  --agree-tos \
  --no-eff-email

# Zertifikate kopieren
sudo cp /etc/letsencrypt/live/messenger.yourdomain.com/fullchain.pem ./ssl/
sudo cp /etc/letsencrypt/live/messenger.yourdomain.com/privkey.pem ./ssl/

# Permissions
sudo chmod 644 ./ssl/fullchain.pem
sudo chmod 600 ./ssl/privkey.pem
sudo chown $USER:$USER ./ssl/*.pem
```

### 4.3 Auto-Renewal einrichten

```bash
# Cronjob erstellen
(crontab -l 2>/dev/null; echo "0 3 * * * certbot renew --quiet --post-hook 'cd /opt/messenger && docker compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx'") | crontab -

# Testen
sudo certbot renew --dry-run
```

---

## 🌐 Schritt 5: Nginx Konfiguration

### Domain anpassen

```bash
# In nginx.conf die Domain ersetzen
sed -i 's/messenger.yourdomain.com/YOUR_ACTUAL_DOMAIN/g' nginx/nginx.conf

# Validierung
grep "server_name" nginx/nginx.conf
# Sollte deine Domain zeigen
```

### Nginx Verzeichnisse erstellen

```bash
# Log-Verzeichnis
mkdir -p nginx/logs
```

---

## 🚀 Schritt 6: Deployment

### 6.1 Docker Images bauen

```bash
# Production Build
docker compose -f docker-compose.yml -f docker-compose.prod.yml build --parallel

# Dauer: ~5-15 Minuten (je nach Server)
```

### 6.2 Services starten

```bash
# Detached Mode (im Hintergrund)
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Logs verfolgen
docker compose logs -f

# Warte ~60 Sekunden auf vollständigen Start
```

### 6.3 Health Checks

```bash
# Container Status
docker compose ps

# Erwartete Ausgabe: Alle "healthy" oder "running"

# API Health Check
curl -k https://messenger.yourdomain.com/health

# Erwartete Antwort:
# {"status":"Healthy"}
```

---

## ✅ Schritt 7: Verifikation

### 7.1 HTTPS Test

```bash
# SSL Labs Test (Browser)
# https://www.ssllabs.com/ssltest/analyze.html?d=messenger.yourdomain.com

# cURL Test
curl -I https://messenger.yourdomain.com/health

# Erwartete Header:
# HTTP/2 200
# strict-transport-security: max-age=31536000
```

### 7.2 API Endpoints testen

```bash
# User Registration
curl -X POST https://messenger.yourdomain.com/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "SecurePass123!"
  }'

# Erwartete Antwort: HTTP 201 Created
```

### 7.3 WebSocket Test

```bash
# SignalR Connection (requires wscat)
npm install -g wscat
wscat -c wss://messenger.yourdomain.com/hubs/notifications

# Sollte erfolgreich verbinden
```

---

## 📊 Schritt 8: Monitoring Setup

### 8.1 Docker Logs

```bash
# Alle Logs
docker compose logs -f

# Spezifischer Service
docker compose logs -f gateway-service

# Letzte 100 Zeilen
docker compose logs --tail=100 auth-service
```

### 8.2 Resource Monitoring

```bash
# Container Stats (Live)
docker stats

# Disk Usage
docker system df

# Network Status
docker network ls
```

### 8.3 Health Monitoring (Optional)

**Uptime Kuma** (empfohlen):

```bash
# Als separater Container
docker run -d \
  --name uptime-kuma \
  -p 3001:3001 \
  -v uptime-kuma:/app/data \
  louislam/uptime-kuma:1

# Zugriff: http://YOUR_SERVER_IP:3001
```

---

## 🔄 Schritt 9: Backup & Recovery

### 9.1 Datenbank Backup

```bash
# Backup Script erstellen
cat > /opt/messenger/backup-db.sh << 'EOF'
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/opt/messenger/backups"
mkdir -p $BACKUP_DIR

# PostgreSQL Backup
docker exec messenger_postgres pg_dumpall -U messenger_prod | gzip > $BACKUP_DIR/postgres_$DATE.sql.gz

# Alte Backups löschen (älter als 30 Tage)
find $BACKUP_DIR -name "postgres_*.sql.gz" -mtime +30 -delete

echo "Backup completed: postgres_$DATE.sql.gz"
EOF

chmod +x /opt/messenger/backup-db.sh
```

### 9.2 Automatisches Backup

```bash
# Cronjob (täglich um 2 Uhr)
(crontab -l 2>/dev/null; echo "0 2 * * * /opt/messenger/backup-db.sh") | crontab -
```

### 9.3 Restore

```bash
# Backup wiederherstellen
gunzip -c /opt/messenger/backups/postgres_YYYYMMDD_HHMMSS.sql.gz | \
  docker exec -i messenger_postgres psql -U messenger_prod
```

---

## 🔧 Schritt 10: Wartung

### Updates durchführen

```bash
cd /opt/messenger

# 1. Code aktualisieren
git pull origin master

# 2. Rebuild (falls nötig)
docker compose -f docker-compose.yml -f docker-compose.prod.yml build

# 3. Rolling Update (kein Downtime)
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# 4. Alte Images aufräumen
docker image prune -f
```

### Service Restart

```bash
# Einzelner Service
docker compose restart auth-service

# Alle Services
docker compose -f docker-compose.yml -f docker-compose.prod.yml restart

# Nur Nginx (z.B. nach Config-Änderung)
docker compose restart nginx
```

---

## 🆘 Troubleshooting

### Problem: Nginx startet nicht

```bash
# Config testen
docker compose exec nginx nginx -t

# Logs prüfen
docker compose logs nginx

# Häufige Ursachen:
# - SSL Zertifikate fehlen (ssl/*.pem)
# - Domain in nginx.conf falsch
# - Port 80/443 bereits belegt
```

### Problem: Services nicht erreichbar

```bash
# Port Check
sudo netstat -tulpn | grep -E '(80|443)'

# Container Status
docker compose ps

# Gateway Logs
docker compose logs gateway-service

# Network Test
docker compose exec nginx curl http://gateway-service:8080/health
```

### Problem: Datenbank Verbindung fehlgeschlagen

```bash
# PostgreSQL Logs
docker compose logs postgres

# Verbindung testen
docker compose exec postgres psql -U messenger_prod -d messenger_production

# Password prüfen
cat .env.production | grep POSTGRES_PASSWORD
```

---

## 📚 Nächste Schritte

- [ ] Monitoring Setup (Prometheus, Grafana)
- [ ] Log Aggregation (ELK Stack)
- [ ] Backup-Verifizierung (Restore-Test)
- [ ] Load Testing
- [ ] Disaster Recovery Plan
- [ ] Security Audit

---

## 📞 Support

**Issues**: https://github.com/Krialder/Messenger-App/issues  
**Documentation**: [README.md](../README.md)  
**Security**: SECURITY.md

---

**Status**: ✅ Production Deployment Complete

**Version**: 1.0  
**Created**: 2025-01-16

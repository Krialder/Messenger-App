# Deployment Guide

> **Status**: Planungsphase - Deployment-Konzept, noch nicht getestet

## 1. Übersicht

Dieses Dokument beschreibt, wie das Secure Messenger System deployed wird.

**Deployment-Optionen**:
- **Development**: Lokaler Docker Compose
- **Staging**: Docker Compose auf VM
- **Production**: Docker Compose + Reverse Proxy (Nginx/Traefik)

## 2. Voraussetzungen

### Software

- Docker 24.0+
- Docker Compose 2.20+
- Git

### Hardware (Minimum)

**Development**:
- 8 GB RAM
- 4 CPU Cores
- 50 GB Disk

**Production** (für ~1000 User):
- 16 GB RAM
- 8 CPU Cores
- 200 GB SSD

## 3. Development Setup

### 3.1 Repository klonen

```bash
git clone https://github.com/Krialder/Messenger.git
cd Messenger
```

### 3.2 Environment Variables

`.env` Datei erstellen:

```env
# Database
POSTGRES_USER=messenger
POSTGRES_PASSWORD=change_this_in_production
POSTGRES_DB=messenger

# Redis
REDIS_PASSWORD=change_this_too

# RabbitMQ
RABBITMQ_DEFAULT_USER=messenger
RABBITMQ_DEFAULT_PASS=change_this_as_well

# Application
JWT_SECRET=your-super-secret-jwt-key-min-32-chars
JWT_ISSUER=secure-messenger
JWT_AUDIENCE=secure-messenger-client
JWT_EXPIRY_MINUTES=15

# SMTP (für E-Mail-Versand)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your-email@gmail.com
SMTP_PASSWORD=your-app-password

# Environment
ASPNETCORE_ENVIRONMENT=Development
```

### 3.3 Docker Compose starten

`docker-compose.yml`:

```yaml
version: '3.8'

services:
  # PostgreSQL
  postgres:
    image: postgres:16-alpine
    container_name: messenger-postgres
    environment:
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: ${POSTGRES_DB}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init.sql
    ports:
      - "5432:5432"
    networks:
      - messenger-network
    restart: unless-stopped

  # Redis
  redis:
    image: redis:7-alpine
    container_name: messenger-redis
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis_data:/data
    ports:
      - "6379:6379"
    networks:
      - messenger-network
    restart: unless-stopped

  # RabbitMQ
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: messenger-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_DEFAULT_USER}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_DEFAULT_PASS}
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"  # Management UI
    networks:
      - messenger-network
    restart: unless-stopped

  # Auth Service
  auth-service:
    build:
      context: ./src/Services/AuthService
      dockerfile: Dockerfile
    container_name: messenger-auth-service
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=${RABBITMQ_DEFAULT_USER}
      - RabbitMQ__Password=${RABBITMQ_DEFAULT_PASS}
      - Jwt__Secret=${JWT_SECRET}
      - Jwt__Issuer=${JWT_ISSUER}
      - Jwt__Audience=${JWT_AUDIENCE}
      - Jwt__ExpiryMinutes=${JWT_EXPIRY_MINUTES}
    depends_on:
      - postgres
      - redis
      - rabbitmq
    ports:
      - "5001:80"
    networks:
      - messenger-network
    restart: unless-stopped

  # Message Service
  message-service:
    build:
      context: ./src/Services/MessageService
      dockerfile: Dockerfile
    container_name: messenger-message-service
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=${RABBITMQ_DEFAULT_USER}
      - RabbitMQ__Password=${RABBITMQ_DEFAULT_PASS}
    depends_on:
      - postgres
      - redis
      - rabbitmq
    ports:
      - "5002:80"
    networks:
      - messenger-network
    restart: unless-stopped

  # Key Management Service
  key-service:
    build:
      context: ./src/Services/KeyService
      dockerfile: Dockerfile
    container_name: messenger-key-service
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD}
    depends_on:
      - postgres
      - redis
    ports:
      - "5003:80"
    networks:
      - messenger-network
    restart: unless-stopped

volumes:
  postgres_data:
  redis_data:
  rabbitmq_data:

networks:
  messenger-network:
    driver: bridge
```

**Starten**:
```bash
docker-compose up -d
```

**Logs**:
```bash
docker-compose logs -f
```

**Stoppen**:
```bash
docker-compose down
```

### 3.4 Database Migrations

```bash
# Migrations anwenden
docker-compose exec auth-service dotnet ef database update

# Oder direkt via Script
docker-compose exec postgres psql -U messenger -d messenger -f /scripts/migrations/001_initial.sql
```

### 3.5 Überprüfung

**Services**:
- Auth Service: http://localhost:5001/health
- Message Service: http://localhost:5002/health
- Key Service: http://localhost:5003/health

**Management UIs**:
- RabbitMQ: http://localhost:15672 (user/pass aus `.env`)
- PostgreSQL: `psql -h localhost -U messenger -d messenger`

## 4. Production Deployment

### 4.1 Reverse Proxy (Nginx)

`nginx.conf`:

```nginx
upstream auth_service {
    server auth-service:80;
}

upstream message_service {
    server message-service:80;
}

upstream key_service {
    server key-service:80;
}

server {
    listen 80;
    server_name api.secure-messenger.com;

    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.secure-messenger.com;

    # SSL Certificates
    ssl_certificate /etc/nginx/ssl/fullchain.pem;
    ssl_certificate_key /etc/nginx/ssl/privkey.pem;

    # SSL Configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    # Security Headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header X-Frame-Options "DENY" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # Rate Limiting
    limit_req_zone $binary_remote_addr zone=login:10m rate=5r/m;
    limit_req_zone $binary_remote_addr zone=api:10m rate=100r/m;

    # Auth Service
    location /api/auth/ {
        limit_req zone=login burst=10 nodelay;
        proxy_pass http://auth_service/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Message Service
    location /api/messages/ {
        limit_req zone=api burst=20 nodelay;
        proxy_pass http://message_service/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Key Service
    location /api/keys/ {
        limit_req zone=api burst=20 nodelay;
        proxy_pass http://key_service/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # SignalR WebSocket
    location /hubs/ {
        proxy_pass http://message_service/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 86400;
    }
}
```

**Nginx zu Docker Compose hinzufügen**:

```yaml
  nginx:
    image: nginx:alpine
    container_name: messenger-nginx
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
    ports:
      - "80:80"
      - "443:443"
    depends_on:
      - auth-service
      - message-service
      - key-service
    networks:
      - messenger-network
    restart: unless-stopped
```

### 4.2 SSL Certificates (Let's Encrypt)

```bash
# Certbot installieren
apt-get install certbot

# Zertifikat erstellen
certbot certonly --standalone -d api.secure-messenger.com

# Zertifikate kopieren
cp /etc/letsencrypt/live/api.secure-messenger.com/fullchain.pem ./nginx/ssl/
cp /etc/letsencrypt/live/api.secure-messenger.com/privkey.pem ./nginx/ssl/

# Auto-Renewal (crontab)
0 0 * * 0 certbot renew --quiet && docker-compose restart nginx
```

### 4.3 Environment Variables (Production)

`.env.production`:

```env
# NIEMALS in Git committen!

# Database
POSTGRES_USER=messenger_prod
POSTGRES_PASSWORD=<strong-random-password-64-chars>
POSTGRES_DB=messenger_production

# Redis
REDIS_PASSWORD=<strong-random-password-64-chars>

# RabbitMQ
RABBITMQ_DEFAULT_USER=messenger_prod
RABBITMQ_DEFAULT_PASS=<strong-random-password-64-chars>

# JWT
JWT_SECRET=<strong-random-secret-min-64-chars>
JWT_ISSUER=secure-messenger
JWT_AUDIENCE=secure-messenger-client
JWT_EXPIRY_MINUTES=15

# SMTP
SMTP_HOST=smtp.sendgrid.net
SMTP_PORT=587
SMTP_USER=apikey
SMTP_PASSWORD=<sendgrid-api-key>

# Environment
ASPNETCORE_ENVIRONMENT=Production
```

### 4.4 Deployment

```bash
# .env.production verwenden
cp .env.production .env

# Pull latest code
git pull origin master

# Rebuild images
docker-compose build --no-cache

# Start services
docker-compose up -d

# Check logs
docker-compose logs -f

# Verify
curl https://api.secure-messenger.com/health
```

## 5. Backup & Recovery

### 5.1 PostgreSQL Backup

**Automated Daily Backup**:

`backup.sh`:
```bash
#!/bin/bash
BACKUP_DIR="/backups/postgres"
DATE=$(date +%Y%m%d_%H%M%S)

docker-compose exec -T postgres pg_dump -U messenger messenger > "$BACKUP_DIR/backup_$DATE.sql"

# Alte Backups löschen (älter als 30 Tage)
find $BACKUP_DIR -type f -mtime +30 -delete

# Optional: Upload zu S3/Cloud Storage
# aws s3 cp "$BACKUP_DIR/backup_$DATE.sql" s3://messenger-backups/
```

**Crontab**:
```bash
0 2 * * * /path/to/backup.sh
```

### 5.2 Recovery

```bash
# Restore from backup
docker-compose exec -T postgres psql -U messenger messenger < /backups/postgres/backup_20250106_020000.sql
```

### 5.3 Redis Persistence

**RDB Snapshots** (automatisch aktiviert):
- Alle 15 Minuten wenn min. 1 Key geändert
- Alle 5 Minuten wenn min. 10 Keys geändert
- Alle 60 Sekunden wenn min. 1000 Keys geändert

**AOF** (Append-Only File) aktivieren:

`redis.conf`:
```conf
appendonly yes
appendfsync everysec
```

## 6. Monitoring

### 6.1 Health Checks

**Docker Compose Health Checks**:

```yaml
services:
  auth-service:
    # ... existing config
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### 6.2 Logging

**Serilog + Seq**:

`docker-compose.yml`:
```yaml
  seq:
    image: datalust/seq:latest
    container_name: messenger-seq
    environment:
      ACCEPT_EULA: Y
    volumes:
      - seq_data:/data
    ports:
      - "5341:80"
    networks:
      - messenger-network
    restart: unless-stopped
```

**Service-Konfiguration**:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:5341"
        }
      }
    ]
  }
}
```

### 6.3 Metrics (Optional - Prometheus)

`docker-compose.yml`:
```yaml
  prometheus:
    image: prom/prometheus:latest
    container_name: messenger-prometheus
    volumes:
      - ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    ports:
      - "9090:9090"
    networks:
      - messenger-network
    restart: unless-stopped

  grafana:
    image: grafana/grafana:latest
    container_name: messenger-grafana
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana_data:/var/lib/grafana
    ports:
      - "3000:3000"
    depends_on:
      - prometheus
    networks:
      - messenger-network
    restart: unless-stopped
```

## 7. CI/CD (GitHub Actions)

`.github/workflows/deploy.yml`:

```yaml
name: Deploy to Production

on:
  push:
    branches:
      - master

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Run tests
        run: dotnet test --configuration Release

      - name: Build Docker images
        run: docker-compose build

      - name: Deploy to server
        uses: appleboy/ssh-action@v0.1.5
        with:
          host: ${{ secrets.SERVER_HOST }}
          username: ${{ secrets.SERVER_USER }}
          key: ${{ secrets.SSH_PRIVATE_KEY }}
          script: |
            cd /opt/messenger
            git pull origin master
            docker-compose down
            docker-compose up -d --build
            docker-compose logs -f --tail=100
```

## 8. Security Checklist

**Vor Production**:
- [ ] Alle Passwörter in `.env` geändert (min. 32 Zeichen)
- [ ] SSL/TLS konfiguriert (Let's Encrypt)
- [ ] Firewall konfiguriert (nur Ports 80, 443, 22 offen)
- [ ] SSH Key-basierte Auth (Password-Auth deaktiviert)
- [ ] Rate Limiting aktiviert (Nginx)
- [ ] Security Headers gesetzt (HSTS, CSP, X-Frame-Options)
- [ ] Database Backups eingerichtet
- [ ] Monitoring aktiviert (Seq/Prometheus)
- [ ] Log-Rotation konfiguriert
- [ ] Docker Images regelmäßig updaten

## 9. Troubleshooting

### Service startet nicht

```bash
# Logs checken
docker-compose logs auth-service

# Container-Status
docker-compose ps

# In Container reingehen
docker-compose exec auth-service bash
```

### Database Connection Error

```bash
# PostgreSQL läuft?
docker-compose ps postgres

# Connection testen
docker-compose exec postgres psql -U messenger -d messenger -c "SELECT 1;"
```

### High Memory Usage

```bash
# Memory-Nutzung checken
docker stats

# Speicher-Limits setzen (docker-compose.yml):
services:
  auth-service:
    deploy:
      resources:
        limits:
          memory: 512M
```

---

**Dokument-Version**: 1.0  
**Letzte Aktualisierung**: 2025-01-06  
**Status**: Planungsphase - Deployment-Konzept

# 🚀 Deployment Guide - Secure Messenger

**Version**: 9.0  
**Status**: Production Ready ✅  
**Last Updated**: 2025-01-10

---

## 📋 Overview

This guide covers deployment strategies for Secure Messenger Application.

For complete project structure, see [PROJECT_STRUCTURE.md](docs/guides/PROJECT_STRUCTURE.md).

---

## 🎯 Deployment Options

### Option 1: Docker Compose (Recommended) ✅

**Advantages**:
- Simple setup
- Automatic orchestration
- Isolated environment
- Built-in health checks

**Steps**:

```bash
# 1. Clone repository
git clone https://github.com/Krialder/Messenger-App
cd Messenger-App

# 2. Configure environment
cp .env.example .env
# Edit .env and replace all placeholder secrets!

# 3. Start backend services
docker-compose up -d

# 4. Verify services
docker-compose ps
docker-compose logs -f

# 5. Health check
curl http://localhost:5000/health
```

**Ports**:
- API Gateway: `5000` (http://localhost:5000)
- PostgreSQL: `5432`
- RabbitMQ: `5672` (AMQP), `15672` (Management UI)
- Redis: `6379`

---

### Option 2: Standalone Services ✅

**Backend**:

```bash
# 1. Setup PostgreSQL (create databases)
# messenger_auth, messenger_messages, messenger_keys, messenger_users

# 2. Setup RabbitMQ
# Install with management plugin

# 3. Run each service
cd src/Backend/AuthService
dotnet run --configuration Release

# Repeat for all 9 services
```

**Frontend**:

```bash
# Windows
.\build-client.bat

# Linux/macOS
chmod +x build-client.sh && ./build-client.sh

# Output: publish/MessengerClient/MessengerClient.exe
```

---

## 🔧 Configuration

### Environment Variables

See `.env.example` for complete template.

**Critical Settings**:
```env
# JWT Authentication (MUST CHANGE)
JWT_SECRET=CHANGE_THIS_TO_SECURE_RANDOM_64_CHARS

# Database (MUST CHANGE)
POSTGRES_PASSWORD=CHANGE_THIS_SECURE_PASSWORD

# RabbitMQ (MUST CHANGE)
RABBITMQ_PASSWORD=CHANGE_THIS_SECURE_PASSWORD

# Redis (MUST CHANGE)
REDIS_PASSWORD=CHANGE_THIS_SECURE_PASSWORD
```

**Generate secure secrets**:
```bash
# PowerShell (Windows)
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})

# Bash (Linux/macOS)
openssl rand -base64 64 | tr -d '\n'
```

---

## 🔒 Security Checklist

### Pre-Production

- [ ] Replace all passwords in `.env`
- [ ] Generate strong JWT secret (≥ 32 chars)
- [ ] Enable HTTPS (reverse proxy with SSL/TLS)
- [ ] Configure firewall rules
- [ ] Setup database backups
- [ ] Enable log monitoring
- [ ] Configure rate limiting
- [ ] Update CORS origins

---

## 📊 Monitoring & Logging

### Health Checks

```bash
# Gateway
curl http://localhost:5000/health

# All services
for port in 5001 5002 5003 5004 5005 5006 5007 5008; do
  curl http://localhost:$port/health
done
```

**Expected Response**:
```json
{
  "status": "Healthy",
  "duration": "00:00:00.0123456"
}
```

### Logs

```bash
# Docker Compose
docker-compose logs -f

# Specific service
docker-compose logs -f auth_service

# Last 100 lines
docker-compose logs --tail=100
```

---

## 🚢 CI/CD Pipeline

### GitHub Actions

**Backend CI** (`.github/workflows/backend-ci.yml`):
- Automated testing (151 tests)
- Docker image builds
- Code coverage reports

**Frontend CI** (`.github/workflows/frontend-ci.yml`):
- WPF build
- Standalone publish
- Artifact upload

---

## 📦 Release Process

### 1. Semantic Versioning

```bash
git tag v9.0.0
git push origin v9.0.0
```

### 2. GitHub Release

```bash
gh release create v9.0.0 \
  --title "Secure Messenger v9.0 - Production Ready" \
  --notes "See CHANGELOG.md" \
  publish/MessengerClient-win-x64.zip
```

---

## 🌐 Production Example (AWS)

```bash
# 1. EC2 Instance (Ubuntu 22.04)
sudo apt update
sudo apt install -y docker.io docker-compose

# 2. Clone & Configure
git clone https://github.com/Krialder/Messenger-App
cd Messenger-App
cp .env.example .env
# Edit .env with production secrets

# 3. Deploy
docker-compose up -d

# 4. Setup Nginx + SSL
sudo apt install -y nginx certbot python3-certbot-nginx
sudo certbot --nginx -d messenger.yourdomain.com
```

---

## 🧪 Testing Deployment

### Smoke Tests

```bash
# 1. Backend health
curl http://localhost:5000/health

# 2. Register user
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@example.com","password":"Test1234!"}'

# 3. Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test1234!"}'

# 4. Frontend
# Launch MessengerClient.exe → Register → Login → Send Message
```

---

## 📚 Troubleshooting

### Common Issues

**Database Connection Failed**:
```bash
docker-compose logs postgres
docker exec -it messenger_postgres psql -U messenger_admin
```

**RabbitMQ Connection Failed**:
```bash
docker-compose logs rabbitmq
# Management UI: http://localhost:15672
```

**Service Not Starting**:
```bash
docker-compose logs <service-name>
docker-compose restart <service-name>
```

---

## 🎯 Performance Tuning

### Database Indexes

```sql
CREATE INDEX idx_messages_sender ON messages(sender_id);
CREATE INDEX idx_messages_recipient ON messages(recipient_id);
CREATE INDEX idx_messages_timestamp ON messages(timestamp);
```

### Docker Resource Limits

```yaml
services:
  auth_service:
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 512M
```

---

## ✅ Deployment Checklist

### Pre-Deployment
- [ ] All tests passing (151/151)
- [ ] Code reviewed
- [ ] Documentation updated
- [ ] Secrets configured
- [ ] Backup strategy defined

### Deployment
- [ ] Docker Compose up
- [ ] Health checks passing
- [ ] Smoke tests successful
- [ ] Monitoring configured
- [ ] Logs centralized

### Post-Deployment
- [ ] Performance baseline
- [ ] Alerts configured
- [ ] Team notified
- [ ] Rollback plan tested

---

## 📖 Additional Resources

- **Project Structure**: [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)
- **Architecture**: [../02_ARCHITECTURE.md](../02_ARCHITECTURE.md)
- **Security**: [../../SECURITY.md](../../SECURITY.md)
- **API Reference**: [../09_API_REFERENCE.md](../09_API_REFERENCE.md)

---

**Status**: 🟢 **PRODUCTION READY**

**Next Steps**:
1. Deploy to staging
2. Run smoke tests
3. Deploy to production
4. Monitor & iterate

---

**Version**: 9.0  
**Last Updated**: 2025-01-10

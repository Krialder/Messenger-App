#!/bin/bash

# ===================================
# Production Deployment Script
# Secure Messenger
# ===================================

set -e  # Exit on error

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Functions
print_header() {
    echo -e "${CYAN}"
    echo "╔══════════════════════════════════════════╗"
    echo "║   Secure Messenger - Production Deploy  ║"
    echo "╚══════════════════════════════════════════╝"
    echo -e "${NC}"
}

print_step() {
    echo -e "${YELLOW}[$1/$2] $3${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Main Script
print_header

# Step 1: Prerequisites Check
print_step 1 7 "Checking prerequisites..."

# Check if running as root
if [ "$EUID" -eq 0 ]; then
    print_error "Please do not run as root. Run as normal user with sudo privileges."
    exit 1
fi

# Check Docker
if ! command -v docker &> /dev/null; then
    print_error "Docker is not installed. Please install Docker first."
    exit 1
fi
print_success "Docker installed"

# Check Docker Compose
if ! command -v docker compose &> /dev/null; then
    print_error "Docker Compose is not installed. Please install Docker Compose plugin."
    exit 1
fi
print_success "Docker Compose installed"

# Check if .env.production exists
if [ ! -f ".env.production" ]; then
    print_error ".env.production not found!"
    echo ""
    echo "Please create .env.production:"
    echo "  1. cp .env.production.example .env.production"
    echo "  2. Edit .env.production and set all secrets"
    echo "  3. Run this script again"
    exit 1
fi
print_success ".env.production found"

# Step 2: Secrets Validation
print_step 2 7 "Validating secrets..."

# Check for CHANGE_THIS in .env.production
if grep -q "CHANGE_THIS" .env.production; then
    print_error "Found CHANGE_THIS in .env.production!"
    echo ""
    echo "Please replace all placeholder secrets in .env.production:"
    echo "  - JWT_SECRET"
    echo "  - TOTP_ENCRYPTION_KEY"
    echo "  - POSTGRES_PASSWORD"
    echo "  - REDIS_PASSWORD"
    echo "  - RABBITMQ_PASSWORD"
    exit 1
fi
print_success "All secrets configured"

# Step 3: SSL Certificates Check
print_step 3 7 "Checking SSL certificates..."

if [ ! -f "ssl/fullchain.pem" ] || [ ! -f "ssl/privkey.pem" ]; then
    print_warning "SSL certificates not found in ssl/"
    echo ""
    read -p "Do you want to continue without SSL? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo ""
        echo "Please obtain SSL certificates first:"
        echo "  1. Install certbot: sudo apt install certbot"
        echo "  2. Generate certificate: sudo certbot certonly --standalone -d messenger.yourdomain.com"
        echo "  3. Copy certificates: sudo cp /etc/letsencrypt/live/*/fullchain.pem ./ssl/"
        echo "  4. Copy private key: sudo cp /etc/letsencrypt/live/*/privkey.pem ./ssl/"
        echo "  5. Run this script again"
        exit 1
    fi
else
    print_success "SSL certificates found"
fi

# Step 4: Build Images
print_step 4 7 "Building Docker images..."

docker compose -f docker-compose.yml -f docker-compose.prod.yml build --parallel || {
    print_error "Build failed"
    exit 1
}
print_success "Images built successfully"

# Step 5: Stop Old Containers
print_step 5 7 "Stopping old containers..."

docker compose -f docker-compose.yml -f docker-compose.prod.yml down || true
print_success "Old containers stopped"

# Step 6: Start Services
print_step 6 7 "Starting production services..."

docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d || {
    print_error "Failed to start services"
    echo ""
    echo "Showing logs:"
    docker compose -f docker-compose.yml -f docker-compose.prod.yml logs --tail=50
    exit 1
}
print_success "Services started"

# Wait for services to be healthy
echo ""
echo "Waiting for services to be healthy (max 120 seconds)..."
sleep 10

TIMEOUT=120
ELAPSED=0
INTERVAL=5

while [ $ELAPSED -lt $TIMEOUT ]; do
    # Check if all containers are healthy or running
    UNHEALTHY=$(docker compose ps | grep -E "(unhealthy|starting)" | wc -l)
    
    if [ "$UNHEALTHY" -eq 0 ]; then
        print_success "All services are healthy!"
        break
    fi
    
    echo "Waiting... ($ELAPSED/$TIMEOUT seconds)"
    sleep $INTERVAL
    ELAPSED=$((ELAPSED + INTERVAL))
done

if [ $ELAPSED -ge $TIMEOUT ]; then
    print_warning "Some services may not be healthy yet"
    docker compose ps
fi

# Step 7: Health Check
print_step 7 7 "Running health checks..."

# Get domain from .env.production
DOMAIN=$(grep "^DOMAIN=" .env.production | cut -d'=' -f2)

if [ -z "$DOMAIN" ]; then
    DOMAIN="localhost"
    print_warning "DOMAIN not set in .env.production, using localhost"
fi

# Test health endpoint
if [ -f "ssl/fullchain.pem" ]; then
    PROTOCOL="https"
else
    PROTOCOL="http"
fi

echo ""
echo "Testing health endpoint: $PROTOCOL://$DOMAIN/health"

if command -v curl &> /dev/null; then
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -k "$PROTOCOL://$DOMAIN/health" || echo "000")
    
    if [ "$HTTP_CODE" = "200" ]; then
        print_success "Health check passed (HTTP $HTTP_CODE)"
    else
        print_warning "Health check returned HTTP $HTTP_CODE"
        echo "This may be normal if services are still initializing"
    fi
else
    print_warning "curl not installed, skipping HTTP health check"
fi

# Final Summary
echo ""
echo -e "${CYAN}╔══════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║         Deployment Complete!             ║${NC}"
echo -e "${CYAN}╚══════════════════════════════════════════╝${NC}"
echo ""
echo "Container Status:"
docker compose -f docker-compose.yml -f docker-compose.prod.yml ps
echo ""
echo -e "${GREEN}Service URLs:${NC}"
if [ -f "ssl/fullchain.pem" ]; then
    echo "  Gateway: https://$DOMAIN"
    echo "  Health:  https://$DOMAIN/health"
else
    echo "  Gateway: http://$DOMAIN"
    echo "  Health:  http://$DOMAIN/health"
fi
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "  1. Verify health: curl -k $PROTOCOL://$DOMAIN/health"
echo "  2. View logs:     docker compose logs -f"
echo "  3. Monitor:       docker compose ps"
echo "  4. Test API:      curl -k $PROTOCOL://$DOMAIN/api/auth/register"
echo ""
echo -e "${YELLOW}Management Commands:${NC}"
echo "  Restart:  docker compose -f docker-compose.yml -f docker-compose.prod.yml restart"
echo "  Stop:     docker compose -f docker-compose.yml -f docker-compose.prod.yml down"
echo "  Logs:     docker compose -f docker-compose.yml -f docker-compose.prod.yml logs -f"
echo ""
print_success "Deployment successful! 🚀"

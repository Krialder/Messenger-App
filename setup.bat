@echo off
setlocal EnableDelayedExpansion

:: =====================================================
:: Secure Messenger - Docker Setup
:: =====================================================

title Secure Messenger - Docker Setup
chcp 65001 >nul

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Secure Messenger - Docker Setup       ║
echo ╚══════════════════════════════════════════╝
echo.

:: Check if docker is running
echo [1/4] Checking Docker...
docker --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Docker is not installed or not in PATH
    echo Please install Docker Desktop from https://www.docker.com/products/docker-desktop/
    pause
    exit /b 1
)

docker info >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ⚠️  Docker is not running
    echo Please start Docker Desktop and try again
    pause
    exit /b 1
)

echo ✅ Docker is running
echo.

:: Check for .env file
echo [2/4] Checking environment...
if not exist ".env" (
    if exist ".env.example" (
        copy ".env.example" ".env" >nul
        echo ✅ Created .env from .env.example
        echo 🔐 IMPORTANT: Edit .env and change passwords before production!
    ) else (
        echo ❌ .env.example not found!
        pause
        exit /b 1
    )
) else (
    echo ✅ .env file exists
)
echo.

:: Check for docker-compose.yml
if not exist "docker-compose.yml" (
    echo ❌ docker-compose.yml not found!
    pause
    exit /b 1
)
echo ✅ docker-compose.yml found
echo.

:: Stop existing containers
echo [3/4] Stopping existing containers...
docker-compose down >nul 2>&1
echo ✅ Old containers stopped
echo.

:: Start services
echo [4/4] Starting Docker services...
set /p rebuild="Rebuild images? (y/N): "
if /i "%rebuild%"=="y" (
    echo Building images...
    docker-compose build --parallel
    if %ERRORLEVEL% NEQ 0 (
        echo ❌ Build failed!
        pause
        exit /b 1
    )
)

echo Starting containers...
docker-compose up -d
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Failed to start containers!
    echo.
    echo Showing logs:
    docker-compose logs --tail=50
    pause
    exit /b 1
)

echo ✅ Services started
echo.
echo Waiting 30 seconds for services to initialize...
timeout /t 30 /nobreak >nul
echo.

:: Show status
echo ╔══════════════════════════════════════════╗
echo ║            Setup Complete!               ║
echo ╚══════════════════════════════════════════╝
echo.
echo Running containers:
docker-compose ps
echo.
echo Service URLs:
echo   Gateway:    http://localhost:7001
echo   Auth:       http://localhost:5001
echo   Messages:   http://localhost:5002
echo   Users:      http://localhost:5003
echo   RabbitMQ:   http://localhost:15672
echo.
echo Next steps:
echo   1. Check status:  status.bat
echo   2. View logs:     docker-compose logs -f [service-name]
echo   3. Run tests:     test.bat
echo   4. Stop all:      cleanup.bat
echo.
pause

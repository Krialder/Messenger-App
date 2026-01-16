@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

:: ===================================
:: Secure Messenger - Setup
:: ===================================

title Secure Messenger - Setup

:: Wechsle ins Projekt-Root (2 Ebenen hoch von scripts/batch/)
set "SCRIPT_DIR=%~dp0"
for %%i in ("%SCRIPT_DIR%..\..") do set "PROJECT_ROOT=%%~fi"

cd /d "%PROJECT_ROOT%"

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Secure Messenger - Docker Setup       ║
echo ╚══════════════════════════════════════════╝
echo.
echo Projekt: %PROJECT_ROOT%
echo.

:: Check if docker is running
echo [1/4] Checking Docker...
docker --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Docker is not installed or not in PATH
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
        echo 🔐 IMPORTANT: Edit .env and change passwords!
    ) else (
        echo ❌ .env.example not found!
        pause
        exit /b 1
    )
) else (
    echo ✅ .env file exists
)
echo.

:: Stop existing containers
echo [3/4] Stopping old containers...
docker-compose down >nul 2>&1
echo ✅ Old containers stopped
echo.

:: Start services
echo [4/4] Starting services...
set /p rebuild="Rebuild images? (y/N): "
if /i "%rebuild%"=="y" (
    echo Building images...
    docker-compose build --parallel
)

echo Starting containers...
docker-compose up -d
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Failed to start!
    pause
    exit /b 1
)

echo ✅ Services started
echo.
echo ✅ Setup erfolgreich abgeschlossen!
echo.
pause
exit /b 0

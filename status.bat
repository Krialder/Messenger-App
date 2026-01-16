@echo off
setlocal EnableDelayedExpansion

:: =====================================================
:: Secure Messenger - Status Check
:: =====================================================

title Secure Messenger - Status
chcp 65001 >nul

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Secure Messenger - Status Check       ║
echo ╚══════════════════════════════════════════╝
echo.

:: Check Docker
docker --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Docker is not available
    pause
    exit /b 1
)

:: Container Status
echo [1/3] Container Status:
echo ═══════════════════════════════════════════
docker-compose ps
echo.

:: Port Status
echo [2/3] Port Status:
echo ═══════════════════════════════════════════
set ports=7001 5001 5002 5003 5432 6379 5672 15672

for %%p in (%ports%) do (
    netstat -an | findstr ":%%p " | findstr "LISTENING" >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        echo   ✅ Port %%p - OPEN
    ) else (
        echo   ❌ Port %%p - CLOSED
    )
)
echo.

:: Docker Resources
echo [3/3] Docker Resources:
echo ═══════════════════════════════════════════
docker system df
echo.

echo ═══════════════════════════════════════════
echo Available commands:
echo   docker-compose logs [service]  - View logs
echo   docker-compose restart         - Restart all
echo   docker-compose stop            - Stop all
echo.
pause

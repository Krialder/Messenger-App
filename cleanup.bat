@echo off
setlocal EnableDelayedExpansion

:: =====================================================
:: Secure Messenger - Cleanup
:: =====================================================

title Secure Messenger - Cleanup
chcp 65001 >nul

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Secure Messenger - Cleanup             ║
echo ╚══════════════════════════════════════════╝
echo.
echo ⚠️  WARNING: This will stop and remove containers
echo.
set /p confirm="Continue? (y/N): "
if /i not "%confirm%"=="y" (
    echo Cancelled.
    pause
    exit /b 0
)
echo.

:: Stop containers
echo [1/4] Stopping containers...
docker-compose down
if %ERRORLEVEL% EQU 0 (
    echo ✅ Containers stopped
) else (
    echo ⚠️  No containers to stop
)
echo.

:: Clean containers
echo [2/4] Removing unused containers...
docker container prune -f
echo ✅ Containers cleaned
echo.

:: Clean images
echo [3/4] Removing unused images...
docker image prune -f
echo ✅ Images cleaned
echo.

:: Clean networks
echo [4/4] Removing unused networks...
docker network prune -f
echo ✅ Networks cleaned
echo.

:: Optional: Remove volumes
set /p volumes="Also remove volumes (data will be lost)? (y/N): "
if /i "%volumes%"=="y" (
    echo Removing volumes...
    docker volume prune -f
    echo ✅ Volumes removed
)
echo.

echo ╔══════════════════════════════════════════╗
echo ║         Cleanup Complete!                ║
echo ╚══════════════════════════════════════════╝
echo.
echo To restart services, run: setup.bat
echo.
pause

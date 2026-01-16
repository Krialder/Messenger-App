@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

:: ===================================
:: Secure Messenger - Cleanup
:: ===================================

title Secure Messenger - Cleanup

set "SCRIPT_DIR=%~dp0"
for %%i in ("%SCRIPT_DIR%..\..") do set "PROJECT_ROOT=%%~fi"

cd /d "%PROJECT_ROOT%"

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

echo Stopping containers...
docker-compose down
echo.

echo Cleaning up...
docker container prune -f
docker image prune -f
docker network prune -f
echo.

echo ✅ Cleanup complete!
echo.
pause
exit /b 0

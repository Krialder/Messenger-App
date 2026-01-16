@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

:: ===================================
:: Secure Messenger - Status
:: ===================================

title Secure Messenger - Status

set "SCRIPT_DIR=%~dp0"
for %%i in ("%SCRIPT_DIR%..\..") do set "PROJECT_ROOT=%%~fi"

cd /d "%PROJECT_ROOT%"

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Secure Messenger - Status Check       ║
echo ╚══════════════════════════════════════════╝
echo.

docker-compose ps
echo.

echo Port Status:
set ports=7001 5001 5002 5003 5432 6379 5672

for %%p in (%ports%) do (
    netstat -an | findstr ":%%p " | findstr "LISTENING" >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        echo   ✅ Port %%p
    ) else (
        echo   ❌ Port %%p
    )
)
echo.
pause
exit /b 0

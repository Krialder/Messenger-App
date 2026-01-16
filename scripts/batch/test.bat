@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

:: ===================================
:: Secure Messenger - Tests
:: ===================================

title Secure Messenger - Tests

set "SCRIPT_DIR=%~dp0"
for %%i in ("%SCRIPT_DIR%..\..") do set "PROJECT_ROOT=%%~fi"

cd /d "%PROJECT_ROOT%"

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Secure Messenger - Test Runner        ║
echo ╚══════════════════════════════════════════╝
echo.

:: Docker Tests
echo [1/2] Docker Status:
docker-compose ps
echo.

:: Unit Tests
echo [2/2] Unit Tests:
if exist "tests\MessengerTests\MessengerTests.csproj" (
    dotnet test "tests\MessengerTests\MessengerTests.csproj" --verbosity minimal
) else (
    echo ⚠️  Test project not found
)
echo.
pause
exit /b 0

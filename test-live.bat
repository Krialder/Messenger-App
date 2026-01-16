@echo off
setlocal EnableDelayedExpansion

:: =====================================================
:: Live Integration Test Launcher
:: =====================================================

title Secure Messenger - Live Integration Test
chcp 65001 >nul

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Live Integration Test - E2E            ║
echo ╚══════════════════════════════════════════╝
echo.

echo This test will:
echo   1. Register a new test user
echo   2. Login with the test user
echo   3. Fetch user profile
echo   4. Update user profile
echo   5. Refresh access token
echo   6. Test invalid login
echo   7. Logout
echo.
echo Press any key to start...
pause >nul

:: Run PowerShell script
powershell -ExecutionPolicy Bypass -File "scripts\windows\Live-Integration-Test.ps1" -Verbose

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ All integration tests passed!
) else (
    echo.
    echo ❌ Some integration tests failed!
)

echo.
pause

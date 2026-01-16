@echo off
setlocal EnableDelayedExpansion

:: =====================================================
:: Production Service Tests Launcher
:: =====================================================

title Secure Messenger - Production Tests
chcp 65001 >nul

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Production Service Tests               ║
echo ╚══════════════════════════════════════════╝
echo.

:: Parse arguments
set "VERBOSE="
if /i "%~1"=="-Verbose" set "VERBOSE=-Verbose"
if /i "%~1"=="-v" set "VERBOSE=-Verbose"

:: Run PowerShell script
powershell -ExecutionPolicy Bypass -File "scripts\windows\Test-Production.ps1" %VERBOSE%

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ All tests passed!
) else (
    echo.
    echo ❌ Some tests failed!
)

echo.
pause

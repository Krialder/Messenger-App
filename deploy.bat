@echo off
setlocal EnableDelayedExpansion

:: =====================================================
:: Secure Messenger - Production Deployment Launcher
:: =====================================================

title Secure Messenger - Production Deployment
chcp 65001 >nul

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Secure Messenger - Production Deploy  ║
echo ╚══════════════════════════════════════════╝
echo.

:: Check for PowerShell
where powershell >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ PowerShell nicht gefunden!
    echo Bitte installiere PowerShell 7.x oder höher.
    pause
    exit /b 1
)

:: Parse arguments
set "SKIP_SSL="
set "SKIP_HEALTH="
set "FORCE="

:parse_args
if "%~1"=="" goto end_parse
if /i "%~1"=="-SkipSSL" set "SKIP_SSL=-SkipSSL"
if /i "%~1"=="-SkipHealthCheck" set "SKIP_HEALTH=-SkipHealthCheck"
if /i "%~1"=="-Force" set "FORCE=-Force"
shift
goto parse_args
:end_parse

:: Run PowerShell script
echo Running deployment script...
echo.

powershell -ExecutionPolicy Bypass -File "scripts\windows\Deploy-Production.ps1" %SKIP_SSL% %SKIP_HEALTH% %FORCE%

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ Deployment fehlgeschlagen!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ✅ Deployment abgeschlossen!
echo.
pause

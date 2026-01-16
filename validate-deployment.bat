@echo off
setlocal EnableDelayedExpansion

:: =====================================================
:: Quick Deployment Validation
:: =====================================================

echo.
echo ================================================
echo   Windows Production Deployment - Validation
echo ================================================
echo.

:: Check PowerShell Scripts
echo [1/5] Checking PowerShell Scripts...
if exist "scripts\windows\Deploy-Production.ps1" (
    echo   [OK] Deploy-Production.ps1
) else (
    echo   [FAIL] Deploy-Production.ps1 NOT FOUND
    goto :error
)

if exist "scripts\windows\generate-secrets.ps1" (
    echo   [OK] generate-secrets.ps1
) else (
    echo   [FAIL] generate-secrets.ps1 NOT FOUND
    goto :error
)

if exist "scripts\windows\backup-database.ps1" (
    echo   [OK] backup-database.ps1
) else (
    echo   [FAIL] backup-database.ps1 NOT FOUND
    goto :error
)

:: Check Docker Compose Files
echo.
echo [2/5] Checking Docker Compose Files...
if exist "docker-compose.yml" (
    echo   [OK] docker-compose.yml
) else (
    echo   [FAIL] docker-compose.yml NOT FOUND
    goto :error
)

if exist "docker-compose.lan.yml" (
    echo   [OK] docker-compose.lan.yml
) else (
    echo   [FAIL] docker-compose.lan.yml NOT FOUND
    goto :error
)

:: Check Nginx Configs
echo.
echo [3/5] Checking Nginx Configs...
if exist "nginx\nginx-lan.conf" (
    echo   [OK] nginx\nginx-lan.conf
) else (
    echo   [FAIL] nginx\nginx-lan.conf NOT FOUND
    goto :error
)

:: Check Launchers
echo.
echo [4/5] Checking Launchers...
if exist "deploy.bat" (
    echo   [OK] deploy.bat
) else (
    echo   [FAIL] deploy.bat NOT FOUND
    goto :error
)

:: Check Documentation
echo.
echo [5/5] Checking Documentation...
if exist "docs\WINDOWS_DEPLOYMENT.md" (
    echo   [OK] WINDOWS_DEPLOYMENT.md
) else (
    echo   [FAIL] WINDOWS_DEPLOYMENT.md NOT FOUND
    goto :error
)

if exist "WINDOWS_DEPLOYMENT_SUMMARY.md" (
    echo   [OK] WINDOWS_DEPLOYMENT_SUMMARY.md
) else (
    echo   [FAIL] WINDOWS_DEPLOYMENT_SUMMARY.md NOT FOUND
    goto :error
)

:: All checks passed
echo.
echo ================================================
echo   ALL FILES PRESENT - READY TO DEPLOY!
echo ================================================
echo.
echo Next Steps:
echo   1. Run: deploy.bat -SkipSSL
echo   2. OR: powershell -ExecutionPolicy Bypass -File scripts\windows\Deploy-Production.ps1 -SkipSSL
echo.
pause
exit /b 0

:error
echo.
echo ================================================
echo   VALIDATION FAILED - Missing Files
echo ================================================
echo.
echo Please ensure all files are present before deploying.
pause
exit /b 1

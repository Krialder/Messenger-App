@echo off
setlocal EnableDelayedExpansion

:: =====================================================
:: Secure Messenger - Test Runner
:: =====================================================

title Secure Messenger - Tests
chcp 65001 >nul

echo.
echo ╔══════════════════════════════════════════╗
echo ║   Secure Messenger - Test Runner        ║
echo ╚══════════════════════════════════════════╝
echo.

:: Check Docker
echo [1/3] Docker Tests:
echo ═══════════════════════════════════════════
docker --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Docker not available
    goto :unit_tests
) else (
    echo ✅ Docker CLI available
)

docker info >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Docker Engine not running
    goto :unit_tests
) else (
    echo ✅ Docker Engine running
)

:: Check if containers are running
docker-compose ps -q >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ✅ Docker Compose project exists
    
    :: Count running containers
    for /f %%i in ('docker-compose ps -q ^| find /c /v ""') do set count=%%i
    echo ✅ Running containers: !count!
) else (
    echo ⚠️  No containers running
)
echo.

:: Test connectivity
echo [2/3] Service Connectivity:
echo ═══════════════════════════════════════════
call :check_port 7001 Gateway
call :check_port 5001 Auth
call :check_port 5002 Message
call :check_port 5003 User
echo.

:unit_tests
:: Unit Tests
echo [3/3] Unit Tests:
echo ═══════════════════════════════════════════
if exist "tests\MessengerTests\MessengerTests.csproj" (
    echo Running .NET tests...
    dotnet test "tests\MessengerTests\MessengerTests.csproj" --verbosity minimal
    if %ERRORLEVEL% EQU 0 (
        echo ✅ Unit tests passed
    ) else (
        echo ❌ Some unit tests failed
    )
) else (
    echo ⚠️  Test project not found
)
echo.

echo ═══════════════════════════════════════════
echo Test run complete
echo.
pause
exit /b

:check_port
setlocal
set port=%1
set name=%2
netstat -an | findstr ":%port% " | findstr "LISTENING" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo   ✅ %name% Service ^(Port %port%^)
) else (
    echo   ❌ %name% Service ^(Port %port%^)
)
endlocal
exit /b

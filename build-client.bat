@echo off
REM ============================================
REM Messenger WPF Client - Standalone Build Script
REM ============================================

echo.
echo ============================================
echo  Messenger Client - Standalone Build
echo ============================================
echo.

REM Configuration
set "PROJECT_PATH=src\Frontend\MessengerClient\MessengerClient.csproj"
set "OUTPUT_DIR=publish\MessengerClient"
set "RUNTIME=win-x64"
set "CONFIGURATION=Release"

REM Clean previous build
echo [1/4] Cleaning previous build...
if exist "%OUTPUT_DIR%" (
    rmdir /s /q "%OUTPUT_DIR%"
)

REM Restore NuGet packages
echo.
echo [2/4] Restoring NuGet packages...
dotnet restore "%PROJECT_PATH%"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: NuGet restore failed!
    pause
    exit /b 1
)

REM Build project
echo.
echo [3/4] Building project...
dotnet build "%PROJECT_PATH%" -c %CONFIGURATION% --no-restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

REM Publish standalone
echo.
echo [4/4] Publishing standalone application...
dotnet publish "%PROJECT_PATH%" ^
    -c %CONFIGURATION% ^
    -r %RUNTIME% ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o "%OUTPUT_DIR%"

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Publish failed!
    pause
    exit /b 1
)

echo.
echo ============================================
echo  Build Successful!
echo ============================================
echo.
echo Output directory: %OUTPUT_DIR%
echo Executable: %OUTPUT_DIR%\MessengerClient.exe
echo.
echo You can now distribute the entire "%OUTPUT_DIR%" folder.
echo.

REM Optional: Open output folder
echo Opening output folder...
explorer "%OUTPUT_DIR%"

pause

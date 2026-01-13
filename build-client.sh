#!/bin/bash

# ============================================
# Messenger WPF Client - Standalone Build Script (Linux/macOS)
# ============================================

echo ""
echo "============================================"
echo " Messenger Client - Standalone Build"
echo "============================================"
echo ""

# Configuration
PROJECT_PATH="src/Frontend/MessengerClient/MessengerClient.csproj"
OUTPUT_DIR="publish/MessengerClient"
RUNTIME="win-x64"
CONFIGURATION="Release"

# Clean previous build
echo "[1/4] Cleaning previous build..."
if [ -d "$OUTPUT_DIR" ]; then
    rm -rf "$OUTPUT_DIR"
fi

# Restore NuGet packages
echo ""
echo "[2/4] Restoring NuGet packages..."
dotnet restore "$PROJECT_PATH"
if [ $? -ne 0 ]; then
    echo "ERROR: NuGet restore failed!"
    exit 1
fi

# Build project
echo ""
echo "[3/4] Building project..."
dotnet build "$PROJECT_PATH" -c $CONFIGURATION --no-restore
if [ $? -ne 0 ]; then
    echo "ERROR: Build failed!"
    exit 1
fi

# Publish standalone
echo ""
echo "[4/4] Publishing standalone application..."
dotnet publish "$PROJECT_PATH" \
    -c $CONFIGURATION \
    -r $RUNTIME \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "$OUTPUT_DIR"

if [ $? -ne 0 ]; then
    echo "ERROR: Publish failed!"
    exit 1
fi

echo ""
echo "============================================"
echo " Build Successful!"
echo "============================================"
echo ""
echo "Output directory: $OUTPUT_DIR"
echo "Executable: $OUTPUT_DIR/MessengerClient.exe"
echo ""
echo "You can now distribute the entire '$OUTPUT_DIR' folder."
echo ""

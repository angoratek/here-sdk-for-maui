#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

echo "=== Cleaning all build outputs ==="

# Clean .NET projects
dotnet clean src/HereSdk.Explore.Android.Binding -c Release 2>/dev/null || true
dotnet clean src/HereSdk.Explore.iOS.Binding -c Release 2>/dev/null || true
dotnet clean src/HereSdk.Explore.Maui -c Release 2>/dev/null || true
dotnet clean src/HereSdk.Explore.Maui.RefApp -c Release 2>/dev/null || true
dotnet clean tests/HereSdk.Explore.Maui.Tests -c Release 2>/dev/null || true
dotnet clean tests/HereSdk.Explore.Maui.DeviceTests -c Release 2>/dev/null || true

# Remove bin/obj directories
find . -type d -name bin -exec rm -rf {} + 2>/dev/null || true
find . -type d -name obj -exec rm -rf {} + 2>/dev/null || true

# Remove iOS NativeBridge build outputs
rm -rf src/HereSdk.Explore.iOS.NativeBridge/build/

# Remove Sharpie output
rm -rf src/HereSdk.Explore.iOS.Binding/SharpieOutput/

# Remove NuGet packages
rm -rf artifacts/

echo "=== Clean Complete ==="
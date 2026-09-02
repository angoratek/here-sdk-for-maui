#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

echo "=== Building Android Binding ==="
dotnet build src/HereSdk.Explore.Android.Binding -c Release

echo "=== Building iOS NativeBridge ==="
if [ -d "src/HereSdk.Explore.iOS.Binding/Libs/HereSdkExploreNativeBridge.xcframework" ]; then
    echo "NativeBridge xcframework already present, skipping. Run scripts/build-ios-native.sh to rebuild."
else
    echo "NativeBridge xcframework not found — building it (requires Xcode + HERE SDK cache, see scripts/download-sdk.sh)."
    ./scripts/build-ios-native.sh
fi

echo "=== Building iOS Binding ==="
dotnet build src/HereSdk.Explore.iOS.Binding -c Release

echo "=== Building MAUI Library (Android) ==="
dotnet build src/HereSdk.Explore.Maui -f net10.0-android -c Release

echo "=== Building MAUI Library (iOS) ==="
dotnet build src/HereSdk.Explore.Maui -f net10.0-ios -c Release

echo "=== Building Ref App (Android) ==="
dotnet build src/HereSdk.Explore.Maui.RefApp -f net10.0-android -c Release

echo "=== Running Unit Tests ==="
dotnet test tests/HereSdk.Explore.Maui.Tests -c Release

echo "=== Build Complete ==="

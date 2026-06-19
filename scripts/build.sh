#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

echo "=== Building Android Binding ==="
dotnet build src/HereSdk.Explore.Android.Binding -c Release

echo "=== Building iOS NativeBridge ==="
if [ -d "src/HereSdk.Explore.iOS.NativeBridge/build" ]; then
    echo "iOS NativeBridge already built, skipping. Run scripts/build-ios-native.sh to rebuild."
else
    echo "iOS NativeBridge not built yet. Run scripts/build-ios-native.sh first."
fi

echo "=== Building iOS Binding ==="
if [ -d "src/HereSdk.Explore.iOS.Binding/Libs/HereSdkExploreNativeBridge.xcframework" ]; then
    dotnet build src/HereSdk.Explore.iOS.Binding -c Release
else
    echo "Skipping iOS binding — xcframework not found. Run scripts/build-ios-native.sh first."
fi

echo "=== Building MAUI Library (Android) ==="
dotnet build src/HereSdk.Explore.Maui -f net10.0-android -c Release

echo "=== Building MAUI Library (iOS) ==="
if [ -d "src/HereSdk.Explore.iOS.Binding/Libs/HereSdkExploreNativeBridge.xcframework" ]; then
    dotnet build src/HereSdk.Explore.Maui -f net10.0-ios -c Release
else
    echo "Skipping iOS MAUI build — iOS binding not available."
fi

echo "=== Building Ref App (Android) ==="
dotnet build src/HereSdk.Explore.Maui.RefApp -f net10.0-android -c Release

echo "=== Running Unit Tests ==="
dotnet test tests/HereSdk.Explore.Maui.Tests -c Release

echo "=== Build Complete ==="

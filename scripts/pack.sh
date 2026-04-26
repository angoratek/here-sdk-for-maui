#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

VERSION="${1:-4.25.5.0-alpha1}"

echo "=== Packing Android Binding ==="
dotnet pack src/HereSdk.Explore.Android.Binding -c Release -p:PackageVersion="$VERSION"

echo "=== Packing iOS Binding ==="
if [ -d "src/HereSdk.Explore.iOS.Binding/Libs/HereSdkExploreNativeBridge.xcframework" ]; then
    dotnet pack src/HereSdk.Explore.iOS.Binding -c Release -p:PackageVersion="$VERSION"
else
    echo "Skipping iOS binding pack — xcframework not found."
fi

echo "=== Packing MAUI Library ==="
dotnet pack src/HereSdk.Explore.Maui -c Release -p:PackageVersion="$VERSION"

echo "=== Collecting packages ==="
mkdir -p artifacts
find . -name "*.nupkg" -exec mv {} artifacts/ \;
ls -la artifacts/

echo "=== Pack Complete ==="
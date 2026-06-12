#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

# Read version from Version.props
VERSION=$(sed -n 's/.*<PackageVersion>\([^<]*\)<\/PackageVersion>.*/\1/p' "$ROOT_DIR/Version.props")
SUFFIX=""

while [[ $# -gt 0 ]]; do
    case "$1" in
        --suffix)
            SUFFIX="$2"
            shift 2
            ;;
        *)
            VERSION="$1"
            shift
            ;;
    esac
done

if [ -n "$SUFFIX" ]; then
    VERSION="${VERSION}-${SUFFIX}"
fi

echo "=== Packing version $VERSION ==="

# Clean artifacts directory
rm -rf "$ROOT_DIR/artifacts"
mkdir -p "$ROOT_DIR/artifacts"

echo "=== Packing Android Binding ==="
dotnet pack src/HereSdk.Explore.Android.Binding -c Release -p:PackageVersion="$VERSION" --output "$ROOT_DIR/artifacts"

echo "=== Packing iOS Binding ==="
if [ -d "src/HereSdk.Explore.iOS.Binding/Libs/HereSdkExploreNativeBridge.xcframework" ]; then
    dotnet pack src/HereSdk.Explore.iOS.Binding -c Release -p:PackageVersion="$VERSION" --output "$ROOT_DIR/artifacts"
else
    echo "Skipping iOS binding pack — xcframework not found."
fi

echo "=== Packing MAUI Library ==="
dotnet pack src/HereSdk.Explore.Maui -c Release -p:PackageVersion="$VERSION" --output "$ROOT_DIR/artifacts"

echo "=== Packages ==="
ls -la "$ROOT_DIR/artifacts/"

echo "=== Pack Complete ==="

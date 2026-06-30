#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

echo "=== Extracting AAR (if needed) ==="
SDK_VERSION="4.25.5.0"
SDK_BUILD="274356"
AAR_FILENAME="heresdk-explore-android-${SDK_VERSION}.${SDK_BUILD}.zip"
AAR_CACHE="${HERE_SDK_CACHE:-$HOME/.cache/heredl}/$AAR_FILENAME"
AAR_SOURCE="tmp/$AAR_FILENAME"
AAR_DEST="src/HereSdk.Explore.Android.Binding/Jars/heresdk-explore-android-${SDK_VERSION}.${SDK_BUILD}.aar"

# Prefer the SDK cache over the in-repo tmp/ archive.
if [ -f "$AAR_CACHE" ]; then
    AAR_SOURCE="$AAR_CACHE"
    echo "Using cached AAR: $AAR_CACHE"
fi

if [ ! -f "$AAR_DEST" ]; then
    if [ -f "$AAR_SOURCE" ]; then
        echo "Extracting AAR from SDK archive..."
        # The AAR is inside the zip — extract it
        unzip -o "$AAR_SOURCE" -d tmp/aar-extract/
        AAR_FILE=$(find tmp/aar-extract -name "*.aar" | head -1)
        if [ -n "$AAR_FILE" ]; then
            cp "$AAR_FILE" "$AAR_DEST"
            echo "AAR copied to $AAR_DEST"
        else
            echo "ERROR: No .aar file found in the archive"
            exit 1
        fi
    else
        echo "ERROR: SDK archive not found in cache or tmp/"
        echo "  Cache: $AAR_CACHE"
        echo "  tmp/:  $AAR_SOURCE"
        echo "Run ./scripts/download-sdk.sh or place the archive in tmp/."
        exit 1
    fi
else
    echo "AAR already in place: $AAR_DEST"
fi

echo "=== Building Android Binding ==="
dotnet build src/HereSdk.Explore.Android.Binding -c Release

echo "=== Android Binding Build Complete ==="
#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

echo "=== Extracting AAR (if needed) ==="
AAR_SOURCE="tmp/heresdk-explore-android-4.25.5.0.274356.zip"
AAR_DEST="src/HereSdk.Explore.Android.Binding/Jars/heresdk-explore-android-4.25.5.0.274356.aar"

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
        echo "ERROR: SDK archive not found at $AAR_SOURCE"
        echo "Download the HERE Explore SDK for Android and place it in tmp/"
        exit 1
    fi
else
    echo "AAR already in place: $AAR_DEST"
fi

echo "=== Building Android Binding ==="
dotnet build src/HereSdk.Explore.Android.Binding -c Release

echo "=== Android Binding Build Complete ==="
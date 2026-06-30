#!/bin/bash
# Download the HERE SDK Explore v4.25.5.0 archives into a local cache.
#
# The build scripts (build-android.sh, build-ios-native.sh) look in:
#   1. $HERE_SDK_CACHE (default: ~/.cache/heredl)  — preferred
#   2. ./tmp/                                       — legacy fallback
#
# This script populates the cache so subsequent builds don't need the archives
# in the repo working tree. Archives are ~190MB (Android) + ~227MB (iOS).

set -euo pipefail

CACHE_DIR="${HERE_SDK_CACHE:-$HOME/.cache/heredl}"
SDK_VERSION="4.25.5.0"
SDK_BUILD="274356"
BASE_URL="https://account.here.com/documents/107583/1106271"

ANDROID_ZIP="heresdk-explore-android-${SDK_VERSION}.${SDK_BUILD}.zip"
IOS_ZIP="heresdk-explore-ios-${SDK_VERSION}.${SDK_BUILD}.zip"

mkdir -p "$CACHE_DIR"
echo "Cache: $CACHE_DIR"

download() {
    local filename="$1"
    local dest="$CACHE_DIR/$filename"

    if [ -f "$dest" ]; then
        echo "✓ $filename already in cache"
        return 0
    fi

    echo "↓ Downloading $filename …"
    echo "  (requires HERE account; if curl returns 403, the URL has changed)"
    echo "  Manual download: $BASE_URL/$filename"

    if curl -fL -o "$dest" "$BASE_URL/$filename" 2>/dev/null; then
        echo "✓ Saved to $dest"
    else
        rm -f "$dest"
        echo "✗ Download failed. Place the archive manually at:"
        echo "    $dest"
        return 1
    fi
}

# Continue on individual download failure — the user may have one archive but not the other.
download "$ANDROID_ZIP" || true
download "$IOS_ZIP"     || true

echo ""
echo "Cache contents:"
ls -lh "$CACHE_DIR/" || true

echo ""
echo "Next steps:"
echo "  - Build Android: ./scripts/build-android.sh"
echo "  - Build iOS:     ./scripts/build-ios-native.sh"
echo ""
echo "To override cache location:"
echo "  export HERE_SDK_CACHE=/path/to/cache"

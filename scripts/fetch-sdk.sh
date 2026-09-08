#!/bin/bash
# Download the HERE SDK Explore archives into the local cache in CI.
#
# The SDK archives are proprietary and gitignored, so CI fetches them from
# release assets of the private dist repo (angoratek/here-sdk-dist) using a
# fine-grained PAT with read access to that repo, passed as $HERE_SDK_DIST_TOKEN.
#
# Populates the same cache that scripts/download-sdk.sh and build-android.sh /
# build-ios-native.sh consume:
#   $HERE_SDK_CACHE (default: ~/.cache/heredl)
#
# Usage: scripts/fetch-sdk.sh [android|ios]   (default: both)

set -euo pipefail

CACHE_DIR="${HERE_SDK_CACHE:-$HOME/.cache/heredl}"
SDK_VERSION="4.25.5.0"
SDK_BUILD="274356"
DIST_REPO="${HERE_SDK_DIST_REPO:-angoratek/here-sdk-dist}"
TAG="sdk-${SDK_VERSION}"
API_URL="https://api.github.com/repos/$DIST_REPO/releases/tags/$TAG"

ANDROID_ZIP="heresdk-explore-android-${SDK_VERSION}.${SDK_BUILD}.zip"
IOS_ZIP="heresdk-explore-ios-${SDK_VERSION}.${SDK_BUILD}.zip"

if [ -z "${HERE_SDK_DIST_TOKEN:-}" ]; then
    echo "ERROR: HERE_SDK_DIST_TOKEN is not set."
    echo "  Configure a fine-grained PAT with Contents:read on $DIST_REPO"
    echo "  as a repository (and Dependabot) secret, then re-run."
    exit 1
fi

mkdir -p "$CACHE_DIR"

# Resolve asset id by name via the release API, then download through the
# octet-stream endpoint (robust for private repos — no redirect auth quirks).
fetch() {
    local filename="$1"
    local dest="$CACHE_DIR/$filename"

    if [ -f "$dest" ]; then
        echo "✓ $filename already in cache"
        return 0
    fi

    echo "↓ Downloading $filename from $DIST_REPO@$TAG …"
    local asset_id
    asset_id=$(curl -fsSL -H "Authorization: Bearer $HERE_SDK_DIST_TOKEN" \
        "$API_URL" | python3 -c "
import json, sys
release = json.load(sys.stdin)
for a in release.get('assets', []):
    if a['name'] == '$filename':
        print(a['id']); break
else:
    sys.exit('$filename not found in release $TAG of $DIST_REPO')
")
    curl -fSL --retry 3 \
        -H "Authorization: Bearer $HERE_SDK_DIST_TOKEN" \
        -H "Accept: application/octet-stream" \
        -o "$dest" "https://api.github.com/repos/$DIST_REPO/releases/assets/$asset_id"
    echo "✓ Saved to $dest"
}

WHAT="${1:-both}"
case "$WHAT" in
    android) fetch "$ANDROID_ZIP" ;;
    ios)     fetch "$IOS_ZIP" ;;
    both)    fetch "$ANDROID_ZIP"; fetch "$IOS_ZIP" ;;
    *) echo "Usage: $0 [android|ios]"; exit 1 ;;
esac

echo "Cache contents:"
ls -lh "$CACHE_DIR/" || true
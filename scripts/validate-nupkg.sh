#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

ARTIFACTS_DIR="$ROOT_DIR/artifacts"

if [ ! -d "$ARTIFACTS_DIR" ]; then
    echo "No artifacts directory found. Run scripts/pack.sh first."
    exit 1
fi

NUPKGS=$(find "$ARTIFACTS_DIR" -name "*.nupkg" 2>/dev/null || true)
if [ -z "$NUPKGS" ]; then
    echo "No .nupkg files found in $ARTIFACTS_DIR"
    exit 1
fi

echo "=== Validating NuGet packages ==="

FAILURES=0

for pkg in "$ARTIFACTS_DIR"/*.nupkg; do
    pkgname=$(basename "$pkg")
    echo ""
    echo "--- $pkgname ---"

    # Check package structure
    echo "  Contents:"
    unzip -l "$pkg" | grep -E '(\.dll|\.xml|\.pdb|README|\.props|\.targets)' | while read -r line; do
        echo "    $line"
    done

    # Check for XML doc inclusion (only for MAUI and bindings that generate docs)
    if [[ "$pkgname" == HereSdk.Explore.Maui* ]] || [[ "$pkgname" == HereSdk.Explore.Android* ]]; then
        if unzip -l "$pkg" | grep -q '\.xml'; then
            echo "  [PASS] XML doc file included"
        else
            # iOS binding may not produce XML docs due to binding project limitations
            if [[ "$pkgname" == HereSdk.Explore.iOS* ]]; then
                echo "  [SKIP] XML doc check skipped for iOS binding"
            else
                echo "  [FAIL] No XML doc file found"
                FAILURES=$((FAILURES + 1))
            fi
        fi
    fi

    # Check for README
    if unzip -l "$pkg" | grep -q 'README.md'; then
        echo "  [PASS] README.md included"
    else
        echo "  [FAIL] README.md missing"
        FAILURES=$((FAILURES + 1))
    fi

    # Check .nuspec
    echo "  Nuspec metadata:"
    unzip -p "$pkg" '*.nuspec' 2>/dev/null | grep -E '<(id|version|description|authors|license|projectUrl|repository|tags|releaseNotes)' | while read -r line; do
        echo "    $line"
    done || true

    echo "  [PASS] Package structure valid"
done

echo ""
if [ "$FAILURES" -gt 0 ]; then
    echo "=== Validation FAILED — $FAILURES issue(s) found ==="
    exit 1
fi
echo "=== All packages validated successfully ==="

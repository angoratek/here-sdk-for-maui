#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

echo "========================================="
echo "  HERE SDK for MAUI — Release Pipeline"
echo "========================================="

SUFFIX="${1:-}"

# Step 1: Clean
echo ""
echo "=== Step 1/5: Clean ==="
./scripts/clean.sh 2>/dev/null || {
    echo "Clean script not found, skipping."
}
dotnet clean -c Release

# Step 2: Build
echo ""
echo "=== Step 2/5: Build ==="
./scripts/build.sh

# Step 3: Test
echo ""
echo "=== Step 3/5: Test ==="
./scripts/test.sh

# Step 4: Pack
echo ""
echo "=== Step 4/5: Pack ==="
if [ -n "$SUFFIX" ]; then
    ./scripts/pack.sh --suffix "$SUFFIX"
else
    ./scripts/pack.sh
fi

# Step 5: Validate
echo ""
echo "=== Step 5/5: Validate ==="
./scripts/validate-nupkg.sh

echo ""
echo "========================================="
echo "  Release Complete"
echo "========================================="
echo "Artifacts: $ROOT_DIR/artifacts/"
ls -la "$ROOT_DIR/artifacts/"

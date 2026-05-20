#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

echo "=== Generating API documentation ==="

# Install docfx if not present
if ! command -v docfx &> /dev/null; then
    echo "Installing docfx..."
    dotnet tool restore 2>/dev/null || dotnet tool install --global docfx
fi

# Build metadata from XML doc comments
echo "--- Building metadata ---"
docfx metadata docfx.json --force

# Build static HTML site
echo "--- Building site ---"
docfx build docfx.json

echo ""
echo "=== Documentation generated ==="
echo "Output: $ROOT_DIR/artifacts/docs/_site/"
echo "Open artifacts/docs/_site/index.html to browse."

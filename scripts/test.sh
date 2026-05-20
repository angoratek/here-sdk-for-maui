#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

FAILURES=0

echo "=== Running Unit Tests ==="
dotnet test tests/HereSdk.Explore.Maui.Tests -c Release --collect:"XPlat Code Coverage" || FAILURES=$((FAILURES + 1))

echo "=== Running UI Tests ==="
dotnet test tests/HereSdk.Explore.Maui.RefApp.UITests -c Release || FAILURES=$((FAILURES + 1))

echo "=== Running Android Device Tests ==="
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-android -c Release || {
    echo "Android device tests failed (may need emulator)."
    FAILURES=$((FAILURES + 1))
}

echo "=== Running iOS Device Tests ==="
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-ios -c Release || {
    echo "iOS device tests failed (may need simulator)."
    FAILURES=$((FAILURES + 1))
}

echo "=== Tests Complete ==="
if [ "$FAILURES" -gt 0 ]; then
    echo "$FAILURES test suite(s) failed."
    exit 1
fi
echo "All test suites passed."

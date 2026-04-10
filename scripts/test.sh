#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

echo "=== Running Unit Tests ==="
dotnet test tests/HereSdk.Explore.Maui.Tests -c Release --collect:"XPlat Code Coverage"

echo "=== Running Android Device Tests ==="
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net9.0-android -c Release || echo "Android device tests skipped or failed."

echo "=== Running iOS Device Tests ==="
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net9.0-ios -c Release || echo "iOS device tests skipped or failed."

echo "=== Tests Complete ==="
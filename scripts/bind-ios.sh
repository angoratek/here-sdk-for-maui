#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

BINDING_DIR="src/HereSdk.Explore.iOS.Binding"
XCFRAMEWORK="$BINDING_DIR/Libs/HereSdkExploreNativeBridge.xcframework"

echo "=== Running Objective-Sharpie for iOS Bindings ==="

if [ ! -d "$XCFRAMEWORK" ]; then
    echo "ERROR: xcframework not found at $XCFRAMEWORK"
    echo "Run scripts/build-ios-native.sh first."
    exit 1
fi

if ! command -v sharpie &> /dev/null; then
    echo "ERROR: Objective-Sharpie not found."
    echo "Install it from: https://aka.ms/objective-sharpie"
    exit 1
fi

# Find headers in the xcframework
HEADERS_DIR="$XCFRAMEWORK/ios-arm64/HereSdkExploreNativeBridge.framework/Headers"
if [ ! -d "$HEADERS_DIR" ]; then
    echo "ERROR: Headers directory not found at $HEADERS_DIR"
    exit 1
fi

# Get SDK version
IOS_SDK_VERSION=$(xcrun --sdk iphoneos --show-sdk-version 2>/dev/null || echo "17.0")

SHARPIE_OUTPUT="$BINDING_DIR/SharpieOutput"
mkdir -p "$SHARPIE_OUTPUT"

echo "Running Sharpie against SDK iphoneos$IOS_SDK_VERSION..."
sharpie bind \
    --sdk="iphoneos$IOS_SDK_VERSION" \
    --output="$SHARPIE_OUTPUT" \
    --namespace="Here.Explore.iOS" \
    --scope="$HEADERS_DIR" \
    "$HEADERS_DIR/HereSdkExploreNativeBridge-Swift.h" \
    -arch arm64

echo ""
echo "Sharpie output written to: $SHARPIE_OUTPUT"
echo ""
echo "IMPORTANT: You must now manually:"
echo "  1. Copy relevant parts from SharpieOutput/ApiDefinition.cs to ApiDefinition.cs"
echo "  2. Copy enums/structs to StructsAndEnums.cs"
echo "  3. Remove all [Verify] attributes and resolve each one"
echo "  4. Fix type mismatches (NSInteger → nint, CGFloat → nfloat, etc.)"
echo "  5. Add [Async] attributes for completion-handler methods"
echo ""
echo "Then run: dotnet build src/HereSdk.Explore.iOS.Binding -c Release"
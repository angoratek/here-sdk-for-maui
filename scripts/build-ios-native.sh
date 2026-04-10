#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$ROOT_DIR"

NATIVE_BRIDGE_DIR="src/HereSdk.Explore.iOS.NativeBridge"

echo "=== Building iOS NativeBridge xcframework ==="
echo "This requires Xcode and the HERE SDK xcframework."

# Check for Xcode
if ! command -v xcodebuild &> /dev/null; then
    echo "ERROR: xcodebuild not found. Install Xcode to build the iOS NativeBridge."
    exit 1
fi

# Check for HERE SDK
HERE_SDK_XCFRAMEWORK="tmp/ios-inspect/heresdk-explore-ios-4.25.5.0.274356/heresdk/frameworks/heresdk.xcframework"
if [ ! -d "$HERE_SDK_XCFRAMEWORK" ]; then
    echo "ERROR: HERE SDK xcframework not found at $HERE_SDK_XCFRAMEWORK"
    echo "Extract the iOS SDK first."
    exit 1
fi

cd "$NATIVE_BRIDGE_DIR"

# Build for device (arm64)
echo "Building for iOS device (arm64)..."
xcodebuild archive \
    -scheme HereSdkExploreNativeBridge \
    -sdk iphoneos \
    -configuration Release \
    -archivePath "./build/ios.xcarchive" \
    SKIP_INSTALL=NO \
    BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
    clean build 2>&1 || {
        echo "NOTE: The Xcode project needs to be created first."
        echo "Run 'scripts/setup-ios-xcode-project.sh' to generate it."
        exit 1
    }

# Build for simulator (x86_64 + arm64)
echo "Building for iOS simulator..."
xcodebuild archive \
    -scheme HereSdkExploreNativeBridge \
    -sdk iphonesimulator \
    -configuration Release \
    -archivePath "./build/ios-simulator.xcarchive" \
    SKIP_INSTALL=NO \
    BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
    clean build

# Create xcframework
echo "Creating xcframework..."
FRAMEWORK_NAME="HereSdkExploreNativeBridge"
xcodebuild -create-xcframework \
    -framework "./build/ios.xcarchive/Products/Library/Frameworks/$FRAMEWORK_NAME.framework" \
    -framework "./build/ios-simulator.xcarchive/Products/Library/Frameworks/$FRAMEWORK_NAME.framework" \
    -output "./build/$FRAMEWORK_NAME.xcframework"

# Copy to iOS Binding project
echo "Copying xcframework to iOS Binding project..."
cp -R "./build/$FRAMEWORK_NAME.xcframework" "../HereSdk.Explore.iOS.Binding/Libs/"

echo "=== iOS NativeBridge Build Complete ==="
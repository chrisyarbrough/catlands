#!/bin/bash

APP_NAME="CatLands Sprite Editor"
BUNDLE_NAME="$APP_NAME.app"
OUTPUT_DIR=$1
BUNDLE_DIR="$OUTPUT_DIR../Bundled/$BUNDLE_NAME"

echo "Bundling to $BUNDLE_DIR"

mkdir -p "$BUNDLE_DIR/Contents/MacOS"
mkdir -p "$BUNDLE_DIR/Contents/Resources"

rm -R "$OUTPUT_DIR/runtimes/linux-x64"*
rm -R "$OUTPUT_DIR/runtimes/win-arm64"*
rm -R "$OUTPUT_DIR/runtimes/win-x64"*
rm -R "$OUTPUT_DIR/runtimes/win-x86"*

cp -R "$OUTPUT_DIR/"* "$BUNDLE_DIR/Contents/MacOS/"

cp "PlatformSpecific/macOS/Info.plist" """$BUNDLE_DIR/Contents/Info.plist"""
cp "PlatformSpecific/macOS/Icon.icns" """$BUNDLE_DIR/Contents/Resources/Icon.icns"""

#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

# echo commands
set -x

export UNO_UITEST_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery.UITests
export UNO_UITEST_ANDROID_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery
export UNO_UITEST_BINARY=$BUILD_SOURCESDIRECTORY/Uno.Gallery.UITests/bin/Release/net47/Uno.Gallery.UITests.dll
export UITEST_TEST_TIMEOUT=60m

# Prefer the signed APK from build artifacts (Windows job) when available,
# otherwise fall back to the unsigned APK published locally by the UITest job (macOS agent).
APK_FROM_ARTIFACT="$(ls "$UNO_UITEST_ANDROIDAPK_BASEPATH"/*.apk 2>/dev/null | head -n 1 || true)"
APK_FROM_LOCAL="$(ls $BUILD_SOURCESDIRECTORY/Uno.Gallery/bin/Release/net10.0-android/android-x64/publish/*.apk 2>/dev/null | head -n 1 || true)"

if [ -f "$APK_FROM_ARTIFACT" ]; then
  export UNO_UITEST_ANDROIDAPK_PATH="$APK_FROM_ARTIFACT"
elif [ -f "$APK_FROM_LOCAL" ]; then
  export UNO_UITEST_ANDROIDAPK_PATH="$APK_FROM_LOCAL"
else
  echo "ERROR: APK not found (neither artifact nor local publish)."
  exit 1
fi

echo "Using APK: $UNO_UITEST_ANDROIDAPK_PATH"

# .NET 9 UITest workaround (maui#31072): ensure assemblies.blob exists inside the APK
# UITest sometimes refuses to run if no assemblies store is present.
# Related issue: https://github.com/dotnet/maui/issues/31072
command -v zip >/dev/null || { echo "ERROR: 'zip' not found on PATH"; exit 1; }
tmpdir="$(mktemp -d)"
touch "$tmpdir/assemblies.blob"
( cd "$tmpdir" && zip -q "$UNO_UITEST_ANDROIDAPK_PATH" assemblies.blob )
rm -rf "$tmpdir"

source "$BUILD_SOURCESDIRECTORY/build/scripts/android-sdk-emu.inc.sh"

cp $UNO_UITEST_ANDROIDAPK_PATH $UNO_UITEST_SCREENSHOT_PATH

cd $UNO_UITEST_PROJECT

dotnet test \
	-c Release \
	-l:"console;verbosity=normal" \
	--logger "nunit;LogFileName=$BUILD_SOURCESDIRECTORY/build/TestResult.xml" \
	--blame-hang-timeout $UITEST_TEST_TIMEOUT \
	-v m \
	|| true

## Dump the emulator's system log
$ANDROID_HOME/platform-tools/adb shell logcat -d > $BUILD_ARTIFACTSTAGINGDIRECTORY/android-device-log.txt

$ANDROID_HOME/platform-tools/adb exec-out screencap -p > $BUILD_ARTIFACTSTAGINGDIRECTORY/android-end-run.png

# if we created the emulator instance, kill it
if [ -n "${ANDROID_SERIAL:-}" ]; then
	echo "Shutting down emulator $ANDROID_SERIAL"
	"$ANDROID_HOME/platform-tools/adb" emu kill
fi

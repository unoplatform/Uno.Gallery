#!/usr/bin/env bash

set -euxo pipefail
IFS=$'\n\t'

# `adb install` requires signed APK files, so only look for them
APK_FROM_ARTIFACT="$(ls "$UNO_TEST_ANDROIDAPK_BASEPATH"/*-Signed.apk 2>/dev/null | head -n 1 || true)"
APK_FROM_LOCAL="$(ls "$BUILD_SOURCESDIRECTORY/Uno.Gallery/bin/Release/net10.0-android/android-x64/publish"/*-Signed.apk 2>/dev/null | head -n 1 || true)"

if [ -f "$APK_FROM_ARTIFACT" ]; then
	export UNO_TEST_ANDROIDAPK_PATH="$APK_FROM_ARTIFACT"
elif [ -f "$APK_FROM_LOCAL" ]; then
	export UNO_TEST_ANDROIDAPK_PATH="$APK_FROM_LOCAL"
else
	echo "ERROR: APK not found (neither artifact nor local publish)."
	exit 1
fi

echo "Using APK: $UNO_TEST_ANDROIDAPK_PATH"

# Need API-34 not API-28 to work around: https://github.com/unoplatform/uno/pull/22571
export ANDROID_SIMULATOR_APILEVEL=34

source "$BUILD_SOURCESDIRECTORY/build/scripts/android-sdk-emu.inc.sh"

# Retry adb install — the emulator may be transiently unresponsive right after boot.
adb_install_ok=false
for i in 1 2 3; do
	if "$ANDROID_HOME/platform-tools/adb" install -r "$UNO_TEST_ANDROIDAPK_PATH"; then
		adb_install_ok=true
		break
	fi
	if [ "$i" -lt 3 ]; then
		echo "adb install failed (attempt $i/3), retrying in ${i}s..."
		sleep "$i"
	fi
done
if [ "$adb_install_ok" = false ]; then
	echo "ERROR: adb install failed after 3 attempts."
	exit 1
fi

package_name=$("$LATEST_CMDLINE_TOOLS_PATH/bin/apkanalyzer" manifest application-id "$UNO_TEST_ANDROIDAPK_PATH")

INSTRUMENTATION_OUTPUT=$("$ANDROID_HOME/platform-tools/adb" shell am instrument -w $package_name/my.MainInstrumentation || true)
echo "Instrumentation output:"
echo "$INSTRUMENTATION_OUTPUT"

## Dump the emulator's system log
"$ANDROID_HOME/platform-tools/adb" shell logcat -d > "$BUILD_ARTIFACTSTAGINGDIRECTORY/android-device-log.txt"

# if we created the emulator instance, kill it
if [ -n "${ANDROID_SERIAL:-}" ]; then
	echo "Shutting down emulator $ANDROID_SERIAL"
	"$ANDROID_HOME/platform-tools/adb" emu kill
fi

if echo "$INSTRUMENTATION_OUTPUT" | grep -q "INSTRUMENTATION_CODE: -1"; then
	echo "SUCCESS: Instrumentation completed successfully (INSTRUMENTATION_CODE: -1)"
else
	echo "ERROR: Expected 'INSTRUMENTATION_CODE: -1' not found in instrumentation output."
	exit 1
fi

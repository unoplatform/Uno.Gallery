#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

# Runs the runtime tests compiled into the Android head, on an emulator (CI) or on a connected device (ANDROID_SERIAL set).
# Ported from uno.extensions (build/test-scripts/android-emulator-runtime-tests.sh):
# - `adb shell am start -e` passes the UITEST_RUNTIME_* settings as intent extras, which MainActivity turns into environment variables.
# - MobileRuntimeTestsAutostart runs the tests and writes the results under the app's external files dir; this script waits for that file and pulls it.
#
# Connected-device mode, e.g. from Git Bash on Windows (MSYS_NO_PATHCONV keeps the device paths from being rewritten):
#   MSYS_NO_PATHCONV=1 ANDROID_SERIAL=<serial> ANDROID_HOME=<sdk with platform-tools and build-tools> \
#   BUILD_SOURCESDIRECTORY=$PWD RUNTIME_TESTS_APP_PATH=Uno.Gallery/bin/Release/net10.0-android build/scripts/android-runtime-tests.sh

: "${BUILD_SOURCESDIRECTORY:?}"
# The Release build output of the Android head (the `android-mobile` artifact of the Android stage)
: "${RUNTIME_TESTS_APP_PATH:?}"

# The JIT build of the store package; the artifact also holds the NativeAOT one (.naot), which isn't tested.
# Canary builds add .canary to the app ID (build/templates/canary-updater.yml), which renames the APK.
APK_NAMES=("com.nventive.uno.ui.demo-Signed.apk" "com.nventive.uno.ui.demo.canary-Signed.apk")
AVD_NAME="gallery-runtime-tests"
AVD_IMAGE="system-images;android-34;default;x86_64"
AVD_DEVICE="pixel_6"
EMULATOR_BOOT_TIMEOUT=600
RUN_TIMEOUT_SECONDS=1200
TEST_FILTER="${TEST_FILTER:-}"
USE_CONNECTED_DEVICE=0
if [[ -n "${ANDROID_SERIAL:-}" ]]; then
	USE_CONNECTED_DEVICE=1
fi

BUILD_ROOT="$BUILD_SOURCESDIRECTORY/build"
RESULTS_DIR="$BUILD_ROOT/runtime-tests/android"
RESULTS_PATH="$RESULTS_DIR/android-runtime-tests-results.xml"
EMULATOR_LOG="$RESULTS_DIR/emulator.log"
LOGCAT_LOG="$RESULTS_DIR/logcat.log"

mkdir -p "$RESULTS_DIR"
rm -f "$RESULTS_PATH"

apk_path=""
for apk_name in "${APK_NAMES[@]}"; do
	if [[ -f "$RUNTIME_TESTS_APP_PATH/$apk_name" ]]; then
		apk_path="$RUNTIME_TESTS_APP_PATH/$apk_name"
		break
	fi
done
if [[ -z "$apk_path" ]]; then
	echo "ERROR: ${APK_NAMES[0]} (or its .canary variant) not found under $RUNTIME_TESTS_APP_PATH" >&2
	find "$RUNTIME_TESTS_APP_PATH" -maxdepth 6 -type f -name '*.apk' >&2 || true
	exit 1
fi
echo "Using APK: $apk_path"

if [[ "$USE_CONNECTED_DEVICE" == "1" ]]; then
	: "${ANDROID_HOME:?ANDROID_HOME must point to an SDK with platform-tools and build-tools in connected-device mode}"
	export PATH="$ANDROID_HOME/platform-tools:$PATH"
	echo "Using the connected device $ANDROID_SERIAL"
else
	# Use an SDK of our own: the one on the hosted image has no cmdline-tools/latest.
	# ANDROID_AVD_HOME is pinned because avdmanager and the emulator otherwise disagree on where the AVDs live.
	export ANDROID_HOME="$BUILD_ROOT/android-sdk"
	export ANDROID_SDK_ROOT="$ANDROID_HOME"
	export ANDROID_AVD_HOME="$BUILD_ROOT/android-avd"
	mkdir -p "$ANDROID_AVD_HOME"

	if [[ ! -x "$ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager" ]]; then
		mkdir -p "$ANDROID_HOME/cmdline-tools"
		cmdline_tools_zip="commandlinetools-linux-11076708_latest.zip"
		curl -fsSL "https://dl.google.com/android/repository/$cmdline_tools_zip" -o "/tmp/$cmdline_tools_zip"
		unzip -q "/tmp/$cmdline_tools_zip" -d "$ANDROID_HOME/cmdline-tools"
		rm -f "/tmp/$cmdline_tools_zip"
		mv "$ANDROID_HOME/cmdline-tools/cmdline-tools" "$ANDROID_HOME/cmdline-tools/latest"
	fi

	export PATH="$ANDROID_HOME/platform-tools:$ANDROID_HOME/emulator:$ANDROID_HOME/cmdline-tools/latest/bin:$PATH"

	yes | sdkmanager --sdk_root="$ANDROID_HOME" --licenses >/dev/null 2>&1 || true
	echo "Installing the Android SDK packages..."
	sdkmanager --sdk_root="$ANDROID_HOME" --install "$AVD_IMAGE" "platform-tools" "emulator" "build-tools;34.0.0" | tr '\r' '\n' | uniq

	echo "no" | avdmanager create avd -n "$AVD_NAME" -k "$AVD_IMAGE" --device "$AVD_DEVICE" --force
	if ! emulator -list-avds | grep -qx "$AVD_NAME"; then
		echo "ERROR: The emulator can't see the $AVD_NAME AVD (ANDROID_AVD_HOME=$ANDROID_AVD_HOME)." >&2
		exit 1
	fi
fi

# Read the package and launcher activity from the APK, so the script follows the csproj
aapt="$(find "$ANDROID_HOME/build-tools" -maxdepth 2 \( -name 'aapt' -o -name 'aapt.exe' \) | sort -V | tail -n 1)"
if [[ -z "$aapt" ]]; then
	echo "ERROR: No aapt under $ANDROID_HOME/build-tools" >&2
	exit 1
fi
badging="$("$aapt" dump badging "$apk_path" | tr -d '\r')"
package_name="$(printf '%s\n' "$badging" | awk -F"'" '/^package: / {print $2; exit}')"
launcher_activity="$(printf '%s\n' "$badging" | awk -F"'" '/launchable-activity/ {print $2; exit}')"
if [[ -z "$package_name" || -z "$launcher_activity" ]]; then
	echo "ERROR: Couldn't read the package name or the launcher activity from $apk_path" >&2
	exit 1
fi
echo "Package: $package_name, activity: $launcher_activity"
device_result_file="/storage/emulated/0/Android/data/$package_name/files/runtime-tests-results.xml"

adb start-server

EMULATOR_PID=""
LOGCAT_PID=""
cleanup() {
	if [[ -n "$LOGCAT_PID" ]]; then
		kill "$LOGCAT_PID" 2>/dev/null || true
	fi
	if [[ -n "$EMULATOR_PID" ]]; then
		adb -s emulator-5554 emu kill 2>/dev/null || true
		wait "$EMULATOR_PID" 2>/dev/null || true
	fi
}
trap cleanup EXIT

if [[ "$USE_CONNECTED_DEVICE" == "0" ]]; then
	emulator -avd "$AVD_NAME" -no-window -no-audio -no-boot-anim -no-snapshot -wipe-data -no-metrics \
		-gpu swiftshader_indirect -accel on -port 5554 >"$EMULATOR_LOG" 2>&1 &
	EMULATOR_PID=$!

	# Bounded, and notices a dead emulator: `adb wait-for-device` would block until the job times out.
	emulator_failed() {
		echo "ERROR: $1" >&2
		tail -n 200 "$EMULATOR_LOG" >&2 || true
		exit 1
	}

	echo "Waiting for the emulator to boot (timeout ${EMULATOR_BOOT_TIMEOUT}s)..."
	SECONDS=0
	until [[ "$(adb shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" == "1" ]]; do
		kill -0 "$EMULATOR_PID" 2>/dev/null || emulator_failed "The emulator exited after ${SECONDS}s without booting."
		[[ $SECONDS -lt $EMULATOR_BOOT_TIMEOUT ]] || emulator_failed "The emulator didn't boot within ${EMULATOR_BOOT_TIMEOUT}s."
		sleep 2
	done
	echo "The emulator booted in ${SECONDS}s."

	adb shell settings put global window_animation_scale 0 || true
	adb shell settings put global transition_animation_scale 0 || true
	adb shell settings put global animator_duration_scale 0 || true
fi

adb logcat -c || true
adb logcat >"$LOGCAT_LOG" 2>&1 &
LOGCAT_PID=$!

adb install -r "$apk_path"
# Left over from an earlier run on a connected device
adb shell rm -f "$device_result_file" || true

am_args=(-S -a android.intent.action.MAIN -c android.intent.category.LAUNCHER -e UITEST_RUNTIME_AUTOSTART_RESULT_FILE "$device_result_file")
# Only when set: `adb shell` drops an empty argument, and `am` would then take the next flag as the extra's value.
# Base64, so the filter survives `am start -e` quoting.
if [[ -n "$TEST_FILTER" ]]; then
	am_args+=(-e UITEST_RUNTIME_TESTS_FILTER "$(printf '%s' "$TEST_FILTER" | base64 -w 0)")
fi

echo "Starting $package_name..."
adb shell am start "${am_args[@]}" -n "$package_name/$launcher_activity"

result_exists() {
	adb shell "[ -f \"$device_result_file\" ] && echo found || true" 2>/dev/null | tr -d '\r' | grep -q '^found$'
}

app_died=0
SECONDS=0
echo "Waiting up to ${RUN_TIMEOUT_SECONDS}s for $device_result_file..."
while [[ $SECONDS -lt $RUN_TIMEOUT_SECONDS ]] && ! result_exists; do
	sleep 5
	if ! adb shell pidof "$package_name" >/dev/null 2>&1 && ! result_exists; then
		app_died=1
		break
	fi
done

if ! result_exists; then
	if [[ "$app_died" == "1" ]]; then
		echo "ERROR: The app exited after ${SECONDS}s without writing $device_result_file." >&2
	else
		echo "ERROR: $device_result_file didn't appear within ${RUN_TIMEOUT_SECONDS}s." >&2
	fi
	echo "--- logcat, app and crash lines ---" >&2
	grep -E "$package_name|MobileRuntimeTestsAutostart|monodroid|AndroidRuntime|FATAL" "$LOGCAT_LOG" 2>/dev/null | tail -n 100 >&2 || true
	exit 1
fi

# Give the app a moment to finish writing the file
sleep 2
adb pull "$device_result_file" "$RESULTS_PATH"

python3 "$BUILD_SOURCESDIRECTORY/build/scripts/validate-runtime-test-results.py" "$RESULTS_PATH"

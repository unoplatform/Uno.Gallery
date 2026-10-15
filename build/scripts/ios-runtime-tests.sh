#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

# Runs the runtime tests compiled into the iOS head on a simulator.
# Ported from uno.extensions (build/test-scripts/ios-simulator-runtime-tests.sh):
# - `simctl launch` passes SIMCTL_CHILD_* variables to the app, so the UITEST_RUNTIME_* settings arrive as environment variables.
# - MobileRuntimeTestsAutostart runs the tests and writes the results into the app's data container, which the host can read directly.

: "${BUILD_SOURCESDIRECTORY:?}"
# The Release build output of the iOS head for the simulator (-p:RuntimeIdentifier=iossimulator-*)
: "${RUNTIME_TESTS_APP_PATH:?}"
# Must exist on the agent and match the iOS SDK of the selected Xcode (set together in azure-pipelines.yml)
: "${SIMULATOR_DEVICE:?}"
: "${SIMULATOR_RUNTIME_PREFIX:?}"

RUN_TIMEOUT_SECONDS=1200
TEST_FILTER="${TEST_FILTER:-}"

RESULTS_DIR="$BUILD_SOURCESDIRECTORY/build/runtime-tests/ios"
RESULTS_PATH="$RESULTS_DIR/ios-runtime-tests-results.xml"

mkdir -p "$RESULTS_DIR"
rm -f "$RESULTS_PATH"

app_bundle="$(find "$RUNTIME_TESTS_APP_PATH" -type d -name '*.app' -path '*iossimulator*' -print | head -n 1 || true)"
if [[ -z "$app_bundle" ]]; then
	echo "ERROR: No simulator .app bundle found under $RUNTIME_TESTS_APP_PATH" >&2
	exit 1
fi
echo "Using app bundle: $app_bundle"

bundle_id="$(/usr/libexec/PlistBuddy -c 'Print :CFBundleIdentifier' "$app_bundle/Info.plist")"
echo "Bundle identifier: $bundle_id"

# Not `set -e`-fatal: when nothing matches, the diagnostic below should still print
device_udid="$(xcrun simctl list devices --json | python3 -c "
import json, sys
for runtime, devices in json.load(sys.stdin)['devices'].items():
    if runtime.startswith('$SIMULATOR_RUNTIME_PREFIX'):
        for device in devices:
            if device.get('isAvailable') is not False and device.get('name') == '$SIMULATOR_DEVICE':
                print(device['udid'])
                sys.exit(0)
sys.exit(1)
" || true)"
if [[ -z "$device_udid" ]]; then
	echo "ERROR: No available '$SIMULATOR_DEVICE' simulator with a $SIMULATOR_RUNTIME_PREFIX runtime." >&2
	xcrun simctl list devices available >&2
	exit 1
fi
echo "Simulator: $device_udid"

cleanup() {
	# Console.WriteLine output from the app ends up in os_log
	xcrun simctl spawn "$device_udid" log show --last 30m --style compact \
		--predicate 'processImagePath CONTAINS "Uno.Gallery"' >"$RESULTS_DIR/app.log" 2>&1 || true
	xcrun simctl terminate "$device_udid" "$bundle_id" 2>/dev/null || true
	xcrun simctl shutdown "$device_udid" 2>/dev/null || true
}
trap cleanup EXIT

# Not `simctl bootstatus -b`, which also waits for data migrations and takes 5+ minutes on a fresh agent
xcrun simctl boot "$device_udid" || true
SECONDS=0
until xcrun simctl list devices | grep "$device_udid" | grep -q "(Booted)"; do
	if [[ $SECONDS -ge 180 ]]; then
		echo "ERROR: The simulator didn't boot within 180s." >&2
		exit 1
	fi
	sleep 2
done
echo "The simulator booted in ${SECONDS}s."

xcrun simctl install "$device_udid" "$app_bundle"

app_container="$(xcrun simctl get_app_container "$device_udid" "$bundle_id" data)"
result_file="$app_container/Documents/runtime-tests-results.xml"
mkdir -p "$app_container/Documents"
rm -f "$result_file"

export SIMCTL_CHILD_UITEST_RUNTIME_AUTOSTART_RESULT_FILE="$result_file"
export SIMCTL_CHILD_UITEST_RUNTIME_TESTS_FILTER="$(printf '%s' "$TEST_FILTER" | base64)"

echo "Starting $bundle_id..."
xcrun simctl launch --terminate-running-process "$device_udid" "$bundle_id"

echo "Waiting up to ${RUN_TIMEOUT_SECONDS}s for $result_file..."
SECONDS=0
while [[ ! -f "$result_file" && $SECONDS -lt $RUN_TIMEOUT_SECONDS ]]; do
	sleep 5
done

if [[ ! -f "$result_file" ]]; then
	echo "ERROR: $result_file didn't appear within ${RUN_TIMEOUT_SECONDS}s." >&2
	exit 1
fi

# Give the app a moment to finish writing the file
sleep 2
cp "$result_file" "$RESULTS_PATH"

python3 "$BUILD_SOURCESDIRECTORY/build/scripts/validate-runtime-test-results.py" "$RESULTS_PATH"

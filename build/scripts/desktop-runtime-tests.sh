#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

# Runs the runtime tests compiled into the desktop head (Uno.Gallery/RuntimeTests) under a virtual X server.
# The engine starts the tests when UNO_RUNTIME_TESTS_RUN_TESTS is set, writes the NUnit results, then exits the app.

: "${BUILD_SOURCESDIRECTORY:?}"
# The Release build output of the desktop head (the `desktop` artifact of the Skia_Desktop stage)
: "${RUNTIME_TESTS_APP_PATH:?}"

RESULTS_DIR="$BUILD_SOURCESDIRECTORY/build/runtime-tests/desktop"
# `true` runs every test. A filter string works too, but not a JSON config: the engine parses it with reflection-based
# System.Text.Json and silently falls back to using it as a filter when that fails.
export UNO_RUNTIME_TESTS_RUN_TESTS="${UNO_RUNTIME_TESTS_RUN_TESTS:-true}"
export UNO_RUNTIME_TESTS_OUTPUT_PATH="$RESULTS_DIR/desktop-runtime-tests-results.xml"

mkdir -p "$RESULTS_DIR"
rm -f "$UNO_RUNTIME_TESTS_OUTPUT_PATH"

if [[ ! -f "$RUNTIME_TESTS_APP_PATH/Uno.Gallery.dll" ]]; then
	echo "ERROR: No Uno.Gallery.dll under $RUNTIME_TESTS_APP_PATH" >&2
	exit 1
fi

sudo apt-get update -qq
sudo apt-get install -y -qq xvfb fluxbox

cd "$RUNTIME_TESTS_APP_PATH"

# The app's exit code only reflects engine errors, not test failures: those are reported through the results file.
# With a window manager (fluxbox), like Uno.Toolkit's lane, so window activation and focus behave as on a desktop
xvfb-run --auto-servernum --server-args='-screen 0 1280x1024x24' bash -c "{ fluxbox & } ; dotnet Uno.Gallery.dll" 2>&1 | tee "$RESULTS_DIR/app.log" || echo "The app exited with code $?"

if [[ ! -f "$UNO_RUNTIME_TESTS_OUTPUT_PATH" ]]; then
	echo "ERROR: The runtime tests did not produce results at $UNO_RUNTIME_TESTS_OUTPUT_PATH" >&2
	exit 1
fi

python3 "$BUILD_SOURCESDIRECTORY/build/scripts/validate-runtime-test-results.py" "$UNO_RUNTIME_TESTS_OUTPUT_PATH"

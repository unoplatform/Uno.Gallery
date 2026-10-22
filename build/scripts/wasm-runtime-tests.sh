#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

# Runs the runtime tests compiled into the WebAssembly head in headless Chromium, using the engine's runner tool:
# it serves the app, starts the tests (it injects UNO_RUNTIME_TESTS_RUN_TESTS=true, or the --filter value),
# and writes the NUnit results the app posts back.

: "${BUILD_SOURCESDIRECTORY:?}"
# The Release publish output of the WebAssembly head (the `wwwroot` in the WASM-Skia artifact of the WebAssembly stage)
: "${RUNTIME_TESTS_APP_PATH:?}"

RESULTS_DIR="$BUILD_SOURCESDIRECTORY/build/runtime-tests/wasm"
RESULTS_PATH="$RESULTS_DIR/wasm-runtime-tests-results.xml"
TOOL_PATH="$BUILD_SOURCESDIRECTORY/build/.dotnet-tools"
# Keep in sync with the Uno.UI.RuntimeTests.Engine version in Directory.Packages.props
RUNNER_VERSION="3.0.0-dev.2"

mkdir -p "$RESULTS_DIR" "$TOOL_PATH"
rm -f "$RESULTS_PATH"

if [[ ! -f "$RUNTIME_TESTS_APP_PATH/index.html" ]]; then
	echo "ERROR: No index.html under $RUNTIME_TESTS_APP_PATH" >&2
	exit 1
fi

dotnet tool install --tool-path "$TOOL_PATH" Uno.UI.RuntimeTests.Engine.Wasm.Runner --version "$RUNNER_VERSION"
export PLAYWRIGHT_BROWSERS_PATH="$BUILD_SOURCESDIRECTORY/build/playwright-browsers"
npx --yes playwright install --with-deps chromium

# No UNO_RUNTIME_TESTS_RUN_TESTS query parameter: the runner already sets it, to `true` or to the --filter value.
filter_args=()
if [[ -n "${TEST_FILTER:-}" ]]; then
	filter_args=(--filter "$TEST_FILTER")
fi

# The runner exits with 1 when a test fails; the results file is checked below either way.
"$TOOL_PATH/uno-runtimetests-wasm" \
	--app-path "$RUNTIME_TESTS_APP_PATH" \
	--output "$RESULTS_PATH" \
	--timeout 1200 \
	"${filter_args[@]}" \
	--browser-log-level verbose \
	2>&1 | tee "$RESULTS_DIR/runner.log" || echo "The runner exited with code $?"

if [[ ! -f "$RESULTS_PATH" ]]; then
	echo "ERROR: The runtime tests did not produce results at $RESULTS_PATH" >&2
	exit 1
fi

python3 "$BUILD_SOURCESDIRECTORY/build/scripts/validate-runtime-test-results.py" "$RESULTS_PATH"

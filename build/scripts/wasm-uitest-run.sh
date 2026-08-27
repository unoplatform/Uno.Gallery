#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

export UNO_UITEST_TARGETURI=http://localhost:5000
export UNO_UITEST_DRIVERPATH_CHROME=$BUILD_SOURCESDIRECTORY/build/node_modules/chromedriver/lib/chromedriver
export UNO_UITEST_CHROME_BINARY_PATH=~/.cache/puppeteer/chrome/linux-127.0.6533.72/chrome-linux64/chrome
export UNO_UITEST_SCREENSHOT_PATH=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/wasm
export UNO_UITEST_PLATFORM=Browser
# export UNO_UITEST_CHROME_CONTAINER_MODE=true
export UNO_UITEST_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery.UITests
export UNO_UITEST_LOGFILE=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/wasm/nunit-log.txt
export UNO_UITEST_WASM_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.csproj
export UITEST_TEST_TIMEOUT=60m

if [[ -n "${UNO_UITEST_TEST_TIER:-}" ]]; then
	TEST_TIER="$UNO_UITEST_TEST_TIER"
elif [[ "${BUILD_REASON:-}" == "PullRequest" ]]; then
	TEST_TIER="Smoke"
else
	TEST_TIER="Interaction"
fi

if [[ "$TEST_TIER" != "Smoke" && "$TEST_TIER" != "Interaction" && "$TEST_TIER" != "ExtensionsPatterns" && "$TEST_TIER" != "All" ]]; then
	echo "UNO_UITEST_TEST_TIER must be Smoke, Interaction, ExtensionsPatterns, or All (got '$TEST_TIER')." >&2
	exit 2
fi

cd $BUILD_SOURCESDIRECTORY/build
mkdir -p tools

npm i chromedriver@127.0.0
npm i puppeteer@22.14.0

# install dotnet serve / Remove as needed
dotnet tool uninstall dotnet-serve -g || true
dotnet tool uninstall dotnet-serve --tool-path $BUILD_SOURCESDIRECTORY/build/tools || true
dotnet tool install dotnet-serve --version 1.10.140 --tool-path $BUILD_SOURCESDIRECTORY/build/tools || true
export PATH="$PATH:$BUILD_SOURCESDIRECTORY/build/tools"

mkdir -p $UNO_UITEST_SCREENSHOT_PATH

# Start the server
dotnet-serve -p 5000 -d "$UNO_UITEST_WASM_OUTPUT_PATH" &
SERVER_PID=$!
cleanup() {
	if kill -0 "$SERVER_PID" 2>/dev/null; then
		kill "$SERVER_PID" 2>/dev/null || true
		wait "$SERVER_PID" 2>/dev/null || true
	fi
}
trap cleanup EXIT

cd $UNO_UITEST_PROJECT

TEST_ARGS=(
	-c Release
	-l "console;verbosity=normal"
	--logger "nunit;LogFileName=$BUILD_SOURCESDIRECTORY/build/TestResult.xml"
	--blame-hang-timeout "$UITEST_TEST_TIMEOUT"
	-v m
)

if [[ "${ENABLE_EXTENSIONS_PATTERNS:-false}" == "true" ]]; then
	TEST_ARGS+=("-p:EnableExtensionsPatterns=true")
fi

if [[ "$TEST_TIER" != "All" ]]; then
	TEST_ARGS+=(--filter "TestCategory=$TEST_TIER")
fi

echo "Running $TEST_TIER UITest tier"
RESULT_PATH="$BUILD_SOURCESDIRECTORY/build/TestResult.xml"
rm -f "$RESULT_PATH"
set +e
dotnet test "${TEST_ARGS[@]}"
TEST_EXIT=$?
set -e
if [[ "$TEST_EXIT" -eq 0 ]]; then
	if [[ ! -f "$RESULT_PATH" ]]; then
		echo "dotnet test succeeded without producing $RESULT_PATH." >&2
		TEST_EXIT=1
	elif grep -Eq '<test-run[^>]+total="0"' "$RESULT_PATH"; then
		echo "dotnet test matched zero tests." >&2
		TEST_EXIT=1
	fi
fi
exit "$TEST_EXIT"

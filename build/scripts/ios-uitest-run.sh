#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

# Ensure this script has no BOM even if it ever gets committed with one again
# (the BOM only breaks the shebang at exec time, but this is a safety net).
sed -i '' $'1s/^\xEF\xBB\xBF//' "$0"

export UNO_UITEST_PLATFORM=iOS
export UNO_UITEST_SCREENSHOT_PATH=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/ios
export UNO_UITEST_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery.UITests
export UNO_UITEST_LOGFILE=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/ios/nunit-log.txt
export UNO_UITEST_IOS_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery
export UITEST_TEST_TIMEOUT=60m

# Select the simulator to use
export UNO_UITEST_SIMULATOR_VERSION="com.apple.CoreSimulator.SimRuntime.iOS-18-5"
export UNO_UITEST_SIMULATOR_NAME="iPad Pro 13-inch (M4)"

echo "Listing iOS simulators"
xcrun simctl list devices --json

echo "Starting simulator"
/Applications/Xcode.app/Contents/Developer/Applications/Simulator.app/Contents/MacOS/Simulator &

# Prime the output directory
mkdir -p $UNO_UITEST_SCREENSHOT_PATH/_logs

##
## Pre-install the application to avoid https://github.com/microsoft/appcenter/issues/2389
##

# Wait while ios runtime 16.1 is not having simulators. The install process may
# take a few seconds and "simctl list devices" may not return devices.
while true; do
	export UITEST_IOSDEVICE_ID=`xcrun simctl list -j | jq -r --arg sim "$UNO_UITEST_SIMULATOR_VERSION" --arg name "$UNO_UITEST_SIMULATOR_NAME" '.devices[$sim] | .[] | select(.name==$name) | .udid'`
	export UITEST_IOSDEVICE_DATA_PATH=`xcrun simctl list -j | jq -r --arg sim "$UNO_UITEST_SIMULATOR_VERSION" --arg name "$UNO_UITEST_SIMULATOR_NAME" '.devices[$sim] | .[] | select(.name==$name) | .dataPath'`

	if [ -n "$UITEST_IOSDEVICE_ID" ]; then
    break
  fi

  echo "Waiting for the simulator to be available"
  sleep 5
done

echo "Simulator Data Path: $UITEST_IOSDEVICE_DATA_PATH"
cp "$UITEST_IOSDEVICE_DATA_PATH/../device.plist" $UNO_UITEST_SCREENSHOT_PATH/_logs

echo "Starting simulator: [$UITEST_IOSDEVICE_ID] ($UNO_UITEST_SIMULATOR_VERSION / $UNO_UITEST_SIMULATOR_NAME)"

# Check for the presence of idb, and install it if it's not present
# NOTE: fb-idb currently breaks under Python 3.14 (asyncio get_event_loop change),
# so we pin fb-idb to Python 3.12 to avoid "There is no current event loop in thread 'MainThread'".
# Historical context: prior installs referenced an App Center issue/workaround.
# https://github.com/microsoft/appcenter/issues/2605#issuecomment-1854414963
export PATH=$PATH:~/.local/bin

if ! command -v idb >/dev/null 2>&1; then
  echo "Installing idb (fb-idb + idb-companion) pinned to Python 3.12"

  # 1) Make sure we have a usable python3.12, but don't fail if Homebrew linking conflicts
  if ! command -v python3.12 >/dev/null 2>&1; then
    # Install, but ignore link-step failure; we'll use the keg path explicitly
    brew list --versions python@3.12 >/dev/null 2>&1 || brew install python@3.12 || true
  fi
  # Prefer an existing python3.12 on PATH; otherwise use the keg path
  PY312_BIN="$(command -v python3.12 || echo "$(brew --prefix)/opt/python@3.12/bin/python3.12")"
  export PIPX_DEFAULT_PYTHON="$PY312_BIN"
  echo "Using Python for pipx: $PIPX_DEFAULT_PYTHON"

  # 2) Install helpers
  brew list --versions pipx >/dev/null 2>&1 || brew install pipx
  brew tap facebook/fb >/dev/null 2>&1 || true
  brew list --versions idb-companion >/dev/null 2>&1 || brew install idb-companion

  # 3) Install fb-idb under Python 3.12
  pipx install --force fb-idb
else
  echo "Using idb from: $(command -v idb)"
fi

echo "Booting the simulator"
# Clean state, then boot and wait until the device is fully booted.
xcrun simctl boot "$UITEST_IOSDEVICE_ID" || true

# Try publish/, then non-publish, then the downloaded artifact
if [ -z "${UNO_UITEST_IOSBUNDLE_PATH:-}" ]; then
  echo "iOS app bundle path not found, trying publish"
  UNO_UITEST_IOSBUNDLE_PATH="$(ls "$BUILD_SOURCESDIRECTORY/Uno.Gallery/bin/Release/net10.0-ios/iossimulator-x64/publish/"*.app 2>/dev/null | head -n 1)"
fi
if [ -z "${UNO_UITEST_IOSBUNDLE_PATH:-}" ]; then
  echo "iOS app bundle (publish) not found, trying non-publish"
  UNO_UITEST_IOSBUNDLE_PATH="$(ls "$BUILD_SOURCESDIRECTORY/Uno.Gallery/bin/Release/net10.0-ios/iossimulator-x64/"*.app 2>/dev/null | head -n 1)"
fi
if [ -z "${UNO_UITEST_IOSBUNDLE_PATH:-}" ]; then
  echo "iOS app bundle (non-publish) not found, trying artifact download"
  UNO_UITEST_IOSBUNDLE_PATH="$(ls "$PIPELINE_WORKSPACE/iOS_UITest/"*.app 2>/dev/null | head -n 1)"
fi
if [ -z "${UNO_UITEST_IOSBUNDLE_PATH:-}" ] || [ ! -d "$UNO_UITEST_IOSBUNDLE_PATH" ]; then
  echo "ERROR: iOS app bundle not found in publish/, net10.0-ios/, or under \$PIPELINE_WORKSPACE/iOS_UITest"
  exit 1
fi

echo "Installing the app"
idb install --udid "$UITEST_IOSDEVICE_ID" "$UNO_UITEST_IOSBUNDLE_PATH"

# Run the tests

cd $UNO_UITEST_PROJECT

echo "Running tests"

dotnet test \
	-c Release \
	-l:"console;verbosity=normal" \
	--logger "nunit;LogFileName=$BUILD_SOURCESDIRECTORY/build/TestResult.xml" \
	--blame-hang-timeout $UITEST_TEST_TIMEOUT \
	-v m \
	|| true

# export the simulator logs
export LOG_FILEPATH=$UNO_UITEST_SCREENSHOT_PATH/_logs
export TMP_LOG_FILEPATH=/tmp/DeviceLog-`date +"%Y%m%d%H%M%S"`.logarchive
export LOG_FILEPATH_FULL=$LOG_FILEPATH/DeviceLog-`date +"%Y%m%d%H%M%S"`.txt

xcrun simctl spawn booted log collect --output $TMP_LOG_FILEPATH
log show --style syslog $TMP_LOG_FILEPATH > $LOG_FILEPATH_FULL

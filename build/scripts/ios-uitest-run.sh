#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

export UNO_UITEST_PLATFORM=iOS
export UNO_UITEST_IOSBUNDLE_PATH=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.Mobile/bin/Release/net7.0-maccatalyst/iossimulator-x64/Uno.Gallery.app
export UNO_UITEST_SCREENSHOT_PATH=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/ios
export UNO_UITEST_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.UITest
export UNO_UITEST_LOGFILE=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/ios/nunit-log.txt
export UNO_UITEST_IOS_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.Mobile
export UITEST_TEST_TIMEOUT=60m

echo "Lising iOS simulators"
xcrun simctl list devices --json

/Applications/Xcode.app/Contents/Developer/Applications/Simulator.app/Contents/MacOS/Simulator &

cd $BUILD_SOURCESDIRECTORY

cd $UNO_UITEST_IOS_PROJECT 
dotnet build -f net7.0-ios -r iossimulator-x64 -c Release -p:IsUiAutomationMappingEnabled=True

mkdir -p $UNO_UITEST_SCREENSHOT_PATH

cd $UNO_UITEST_PROJECT

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

mkdir -p $LOG_FILEPATH
xcrun simctl spawn booted log collect --output $TMP_LOG_FILEPATH
log show --style syslog $TMP_LOG_FILEPATH > $LOG_FILEPATH_FULL

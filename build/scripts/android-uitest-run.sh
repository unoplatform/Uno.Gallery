#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

export UNO_UITEST_SCREENSHOT_PATH=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/android
export UNO_UITEST_PLATFORM=Android
export UNO_UITEST_ANDROIDAPK_PATH=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.Droid/bin/Release/net7.0-android/android-x64/com.nventive.uno.ui.demo-Signed.apk
export UNO_UITEST_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.UITest/Uno.Gallery.UITest.csproj
export UNO_UITEST_ANDROID_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.Mobile
export UNO_UITEST_BINARY=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.UITest/bin/Release/net47/Uno.Gallery.UITest.dll
export UNO_EMULATOR_INSTALLED=$BUILD_SOURCESDIRECTORY/build/.emulator_started
export UITEST_TEST_TIMEOUT=60m

if [ ! -f "$UNO_EMULATOR_INSTALLED" ];
then
	# Install AVD files
	echo "y" | $ANDROID_HOME/tools/bin/sdkmanager --install 'system-images;android-28;google_apis_playstore;x86'

	# Create emulator
	echo "no" | $ANDROID_HOME/tools/bin/avdmanager create avd -n xamarin_android_emulator -k 'system-images;android-28;google_apis_playstore;x86' --force

	echo $ANDROID_HOME/emulator/emulator -list-avds

	echo "Starting emulator"

	# Start emulator in background
	$ANDROID_HOME/emulator/emulator -avd xamarin_android_emulator -no-snapshot > /dev/null 2>&1 &

	touch "$UNO_EMULATOR_INSTALLED"
fi

# Build the sample, while the emulator is starting
cd $UNO_UITEST_ANDROID_PROJECT
dotnet publish -f net7.0-android -c Release -p:RuntimeIdentifier=android-x64 /p:IsUiAutomationMappingEnabled=True /p:AndroidUseSharedRuntime=false /p:AotAssemblies=false

mkdir -p $UNO_UITEST_SCREENSHOT_PATH

cp $UNO_UITEST_ANDROIDAPK_PATH $UNO_UITEST_SCREENSHOT_PATH

# Wait for the emulator to finish booting
$ANDROID_HOME/platform-tools/adb wait-for-device shell 'while [[ -z $(getprop sys.boot_completed | tr -d '\r') ]]; do sleep 1; done; input keyevent 82'

$ANDROID_HOME/platform-tools/adb devices

echo "Emulator started"

cd $UNO_UITEST_PROJECT

dotnet test \
	-c Release \
	-l:"console;verbosity=normal" \
	--logger "nunit;LogFileName=$BUILD_SOURCESDIRECTORY/build/TestResult.xml" \
	--blame-hang-timeout $UITEST_TEST_TIMEOUT \
	-v m \
	|| true

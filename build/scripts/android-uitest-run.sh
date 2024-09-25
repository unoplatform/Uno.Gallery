#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

# echo commands
set -x

export UNO_UITEST_SCREENSHOT_PATH=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/android
export UNO_UITEST_PLATFORM=Android
export UNO_UITEST_ANDROIDAPK_PATH=$BUILD_SOURCESDIRECTORY/Uno.Gallery/bin/Release/net8.0-android/android-x64/com.nventive.uno.ui.demo-Signed.apk
export UNO_UITEST_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery.UITests
export UNO_UITEST_ANDROID_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery
export UNO_UITEST_BINARY=$BUILD_SOURCESDIRECTORY/Uno.Gallery.UITests/bin/Release/net47/Uno.Gallery.UITests.dll
export UNO_EMULATOR_INSTALLED=$BUILD_SOURCESDIRECTORY/build/.emulator_started
export UITEST_TEST_TIMEOUT=60m
export ANDROID_SIMULATOR_APILEVEL=28
# Override Android SDK tooling
export ANDROID_HOME=$BUILD_SOURCESDIRECTORY/build/android-sdk
export ANDROID_SDK_ROOT=$BUILD_SOURCESDIRECTORY/build/android-sdk
export CMDLINETOOLS=commandlinetools-mac-8512546_latest.zip
mkdir -p $ANDROID_HOME
wget https://dl.google.com/android/repository/$CMDLINETOOLS
unzip $CMDLINETOOLS -d $ANDROID_HOME/cmdline-tools
rm $CMDLINETOOLS
mv $ANDROID_SDK_ROOT/cmdline-tools/cmdline-tools $ANDROID_SDK_ROOT/cmdline-tools/latest

if [[ -f $ANDROID_HOME/platform-tools/platform-tools/adb ]]
then
	# It appears that the platform-tools 29.0.6 are extracting into an incorrect path
	mv $ANDROID_HOME/platform-tools/platform-tools/* $ANDROID_HOME/platform-tools
fi

AVD_NAME=xamarin_android_emulator
AVD_CONFIG_FILE=~/.android/avd/$AVD_NAME.avd/config.ini

# Install Android SDK emulators and SDKs
if [ ! -f "$UNO_EMULATOR_INSTALLED" ];
then
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'tools'| tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'platform-tools'  | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'build-tools;34.0.0' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'platforms;android-34' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install "platforms;android-$ANDROID_SIMULATOR_APILEVEL" | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'extras;android;m2repository' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'system-images;android-34;google_apis_playstore;x86_64' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install "system-images;android-$ANDROID_SIMULATOR_APILEVEL;google_apis_playstore;x86_64" | tr '\r' '\n' | uniq

	# Create emulator
	echo "no" | $ANDROID_HOME/cmdline-tools/latest/bin/avdmanager create avd -n $AVD_NAME --abi "x86_64" -k "system-images;android-$ANDROID_SIMULATOR_APILEVEL;google_apis_playstore;x86_64"  --sdcard 128M --force

	# based on https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/hosted?view=azure-devops&tabs=yaml#hardware
	# >> Agents that run macOS images are provisioned on Mac pros with a 3 core CPU, 14 GB of RAM, and 14 GB of SSD disk space.
	#echo "hw.cpu.ncore=3" >> $AVD_CONFIG_FILE

	# Bump the heap size as the tests are stressing the application
	echo "vm.heapSize=256M" >> $AVD_CONFIG_FILE

	echo $ANDROID_HOME/emulator/emulator -list-avds

	echo "Starting emulator"

	# Start emulator in background
	nohup $ANDROID_HOME/emulator/emulator \
		-avd $AVD_NAME \
		-skin 1280x800 \
		-memory 4096 \
		-no-window \
		-gpu swiftshader_indirect \
		-no-snapshot \
		-noaudio \
		-no-boot-anim \
		> /dev/null 2>&1 &

	# Wait for the emulator to finish booting
	source $BUILD_SOURCESDIRECTORY/build/scripts/android-uitest-wait-systemui.sh 500

	touch "$UNO_EMULATOR_INSTALLED"
else
	# Restart the emulator to avoid running first-time tasks
	$ANDROID_HOME/platform-tools/adb reboot

	# Wait for the emulator to finish booting
	source $BUILD_SOURCESDIRECTORY/build/scripts/android-uitest-wait-systemui.sh 500
fi

# Build the sample, while the emulator is starting
cd $UNO_UITEST_ANDROID_PROJECT
dotnet publish -f net8.0-android -p:TargetFrameworkOverride=net8.0-android -c Release -p:RuntimeIdentifier=android-x64 /p:IsUiAutomationMappingEnabled=True /p:AndroidUseSharedRuntime=false /p:AotAssemblies=false  -bl:$BUILD_ARTIFACTSTAGINGDIRECTORY/android-app.binlog

mkdir -p $UNO_UITEST_SCREENSHOT_PATH

cp $UNO_UITEST_ANDROIDAPK_PATH $UNO_UITEST_SCREENSHOT_PATH

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
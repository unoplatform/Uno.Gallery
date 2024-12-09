#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

# echo commands
set -x

export UNO_UITEST_ANDROID_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery
export UNO_EMULATOR_INSTALLED=$BUILD_SOURCESDIRECTORY/build/.emulator_started

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

# Install Android SDK emulators and SDKs
if [ ! -f "$UNO_EMULATOR_INSTALLED" ];
then
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'tools'| tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'platform-tools'  | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'build-tools;35.0.0' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'platforms;android-28' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'platforms;android-34' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'extras;android;m2repository' | tr '\r' '\n' | uniq
fi

# Build the sample, while the emulator is starting
cd $UNO_UITEST_ANDROID_PROJECT
dotnet publish -f net8.0-android -p:TargetFrameworkOverride=net8.0-android -c Release -p:RuntimeIdentifier=android-x64 /p:IsUiAutomationMappingEnabled=True /p:AndroidUseSharedRuntime=false /p:AotAssemblies=true  -bl:$BUILD_ARTIFACTSTAGINGDIRECTORY/android-app.binlog

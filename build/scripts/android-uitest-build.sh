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
unzip -o $CMDLINETOOLS -d $ANDROID_HOME/cmdline-tools
rm $CMDLINETOOLS
mv $ANDROID_SDK_ROOT/cmdline-tools/cmdline-tools $ANDROID_SDK_ROOT/cmdline-tools/latest

if [[ -d $ANDROID_HOME/platform-tools/platform-tools ]]
then
	# It appears that the platform-tools 29.0.6 are extracting into an incorrect path
	mv $ANDROID_HOME/platform-tools/platform-tools/* $ANDROID_HOME/platform-tools
fi

# Install Android SDK emulators and SDKs
if [ ! -f "$UNO_EMULATOR_INSTALLED" ];
then
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'tools'| tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'platform-tools'  | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'build-tools;36.0.0' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'platforms;android-28' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'platforms;android-35' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'platforms;android-36' | tr '\r' '\n' | uniq
	echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'extras;android;m2repository' | tr '\r' '\n' | uniq

	if [ "${NAOT:-0}" = "1" ]; then
		# NDK r27+ is required for NativeAOT builds
		echo "y" | $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'ndk;28.2.13676358' | tr '\r' '\n' | uniq
	fi
fi

# Build the sample, while the emulator is starting
cd $UNO_UITEST_ANDROID_PROJECT

publish_extra=(-c Release)
BINLOG_SUFFIX=""
if [ "${NAOT:-0}" = "1" ]; then
	# Build the sample with NativeAOT enabled
	publish_extra+=("-m:1" "-p:SkiaPublishAot=true" "-p:ApplicationTitleVendorSuffix= (NAOT)" "-p:ApplicationIdVendorSuffix=.naot")
	BINLOG_SUFFIX="-naot"
else
	# Build the sample without NativeAOT
	publish_extra+=("/p:AndroidUseAssemblyStore=false" "/p:RunAOTCompilation=false" "/p:PublishTrimmed=false" "/p:AndroidUseSharedRuntime=false")
fi

if [ "${SKIA:-0}" = "1" ]; then
	publish_extra+=("-p:UseSkiaRendering=true")
else
	publish_extra+=("-p:UseNativeRendering=true" "/p:IsUiAutomationMappingEnabled=true")

fi

dotnet publish -f net10.0-android -p:TargetFrameworkOverride=net10.0-android "${publish_extra[@]}" \
	/p:AndroidPackageFormat=apk /p:RuntimeIdentifier=android-x64 \
	-bl:"$BUILD_ARTIFACTSTAGINGDIRECTORY/android-app${BINLOG_SUFFIX}.binlog"

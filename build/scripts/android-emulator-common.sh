#!/usr/bin/env bash
# Common Android emulator setup script.
# Source this script from other scripts that need to set up the Android SDK and emulator.
# Requires BUILD_SOURCESDIRECTORY to be set by the caller.

# Override Android SDK tooling
export ANDROID_HOME=$BUILD_SOURCESDIRECTORY/build/android-sdk
export ANDROID_SDK_ROOT=$BUILD_SOURCESDIRECTORY/build/android-sdk
export LATEST_CMDLINE_TOOLS_PATH=$ANDROID_SDK_ROOT/cmdline-tools/latest
export CMDLINETOOLS=commandlinetools-mac-8512546_latest.zip
mkdir -p $ANDROID_HOME

if [ -d $LATEST_CMDLINE_TOOLS_PATH ];
then
	rm -rf $LATEST_CMDLINE_TOOLS_PATH
fi

wget https://dl.google.com/android/repository/$CMDLINETOOLS
unzip $CMDLINETOOLS -d $ANDROID_HOME/cmdline-tools
rm $CMDLINETOOLS
mv $ANDROID_SDK_ROOT/cmdline-tools/cmdline-tools $LATEST_CMDLINE_TOOLS_PATH

if [[ -f $ANDROID_HOME/platform-tools/platform-tools/adb ]]
then
	# It appears that the platform-tools 29.0.6 are extracting into an incorrect path
	mv $ANDROID_HOME/platform-tools/platform-tools/* $ANDROID_HOME/platform-tools
fi

AVD_NAME=xamarin_android_emulator
AVD_CONFIG_FILE=~/.android/avd/$AVD_NAME.avd/config.ini
EMU_UPDATE_FILE=~/.android/emu-update-last-check.ini
SDK_MGR_TOOLS_FLAG=.sdk_toolkit_installed

install_android_sdk() {
	SIMULATOR_APILEVEL=$1

	if [[ ! -f $SDK_MGR_TOOLS_FLAG ]];
	then
		touch $SDK_MGR_TOOLS_FLAG 

		echo "y" | $LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'tools'| tr '\r' '\n' | uniq
		echo "y" | $LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'platform-tools'  | tr '\r' '\n' | uniq
		echo "y" | $LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'build-tools;36.0.0' | tr '\r' '\n' | uniq
		echo "y" | $LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install 'extras;android;m2repository' | tr '\r' '\n' | uniq
	fi
	
	echo "y" | $LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install "platforms;android-$SIMULATOR_APILEVEL" | tr '\r' '\n' | uniq
	echo "y" | $LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager --sdk_root=${ANDROID_HOME} --install "system-images;android-$SIMULATOR_APILEVEL;google_apis_playstore;x86_64" | tr '\r' '\n' | uniq
}

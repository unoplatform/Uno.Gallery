# Prepare and launch the Android Emulator
#
# Usage:
#
#   source build/scripts/android-sdk-emu.inc.sh
#
# Input Environment Variables:
#
#   BUILD_ARTIFACTSTAGINGDIRECTORY: Required
#   BUILD_SOURCESDIRECTORY: Required
#   ANDROID_SIMULATOR_APILEVEL: Optional; The Android API level to target for the emulator. Default is API-28.
#   ANDROID_SIMULATOR_ABI: Optional; The Android ABI to target for the emulator. Default is x86_64.
#   CMDLINETOOLS: Optional; Android SDK cmdlinetools package name to install.
#     Default is commandlinetools-mac-8512546_latest.zip on macOS and commandlinetools-linux-8512546_latest.zip on Linux.
#
# Output Environment Variables:
#
#   ANDROID_HOME, ANDROID_SDK_ROOT: Where an Android SDK is provisioned; subdir of $BUILD_SOURCESDIRECTORY
#   ANDROID_SERIAL: If an emulator is started, the serial identifier to use for adb commands targeting that emulator
#     Note: used *automatically* by `adb`; see `adb --help` for details.
#   CMDLINETOOLS: Android SDK cmdlinetools package name that was installed.
#   LATEST_CMDLINE_TOOLS_PATH: Path to Android SDK cmdlinetools installation
#   UNO_UITEST_PLATFORM: The platform being tested (Android)
#   UNO_UITEST_SCREENSHOT_PATH: Where screenshots/etc. should be copied to; subdir of $BUILD_ARTIFACTSTAGINGDIRECTORY

# Override Android SDK tooling

# also used for log files
export ANDROID_HOME="$BUILD_SOURCESDIRECTORY/build/android-sdk"
export ANDROID_SDK_ROOT="$BUILD_SOURCESDIRECTORY/build/android-sdk"
export LATEST_CMDLINE_TOOLS_PATH="$ANDROID_SDK_ROOT/cmdline-tools/latest"
export UNO_EMULATOR_INSTALLED="$BUILD_SOURCESDIRECTORY/build/.emulator_started"
export UNO_UITEST_PLATFORM=Android
export UNO_UITEST_SCREENSHOT_PATH="$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/android"

if [ -z "${CMDLINETOOLS:-}" ]; then
	case `uname` in
		Darwin)
			export CMDLINETOOLS="commandlinetools-mac-8512546_latest.zip"
			;;
		Linux)
			export CMDLINETOOLS="commandlinetools-linux-8512546_latest.zip"
			;;
		*)
			echo "Unsupported OS: `uname`. Please set the CMDLINETOOLS environment variable to the appropriate cmdline-tools Android SDK package basename for your OS."
			exit 1
			;;
	esac
fi

mkdir -p "$ANDROID_HOME"

if [ -d "$LATEST_CMDLINE_TOOLS_PATH" ]; then
	rm -rf "$LATEST_CMDLINE_TOOLS_PATH"
fi

wget "https://dl.google.com/android/repository/$CMDLINETOOLS"
unzip "$CMDLINETOOLS" -d "$ANDROID_HOME/cmdline-tools"
rm "$CMDLINETOOLS"
mv "$ANDROID_SDK_ROOT/cmdline-tools/cmdline-tools" "$LATEST_CMDLINE_TOOLS_PATH"

if [[ -f "$ANDROID_HOME/platform-tools/platform-tools/adb" ]]; then
	# It appears that the platform-tools 29.0.6 are extracting into an incorrect path
	mv "$ANDROID_HOME/platform-tools/platform-tools"/* "$ANDROID_HOME/platform-tools"
fi

export ANDROID_SIMULATOR_APILEVEL="${ANDROID_SIMULATOR_APILEVEL:-28}"
export ANDROID_SIMULATOR_ABI="${ANDROID_SIMULATOR_ABI:-x86_64}"

AVD_NAME=xamarin_android_emulator
AVD_CONFIG_FILE="$HOME/.android/avd/$AVD_NAME.avd/config.ini"
SDK_MGR_TOOLS_FLAG=.sdk_toolkit_installed
EMU_UPDATE_FILE="$HOME/.android/emu-update-last-check.ini"

mkdir -p "$UNO_UITEST_SCREENSHOT_PATH"

install_android_sdk() {
	SIMULATOR_APILEVEL="$1"

	if [[ ! -f "$SDK_MGR_TOOLS_FLAG" ]]; then
		touch "$SDK_MGR_TOOLS_FLAG"

		echo "y" | "$LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager" "--sdk_root=${ANDROID_HOME}" --install 'tools'| tr '\r' '\n' | uniq
		echo "y" | "$LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager" "--sdk_root=${ANDROID_HOME}" --install 'platform-tools'  | tr '\r' '\n' | uniq
		echo "y" | "$LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager" "--sdk_root=${ANDROID_HOME}" --install 'build-tools;36.0.0' | tr '\r' '\n' | uniq
		echo "y" | "$LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager" "--sdk_root=${ANDROID_HOME}" --install 'extras;android;m2repository' | tr '\r' '\n' | uniq
	fi

	echo "y" | "$LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager" "--sdk_root=${ANDROID_HOME}" --install "platforms;android-$SIMULATOR_APILEVEL" | tr '\r' '\n' | uniq
	echo "y" | "$LATEST_CMDLINE_TOOLS_PATH/bin/sdkmanager" "--sdk_root=${ANDROID_HOME}" --install "system-images;android-$SIMULATOR_APILEVEL;google_apis_playstore;$ANDROID_SIMULATOR_ABI" | tr '\r' '\n' | uniq
}

if [[ ! -f "$EMU_UPDATE_FILE" ]]; then
	touch "$EMU_UPDATE_FILE"
fi

if [[ -f "$AVD_CONFIG_FILE" ]]; then

	echo "warning: not creating new AVD, as config file already exists at $AVD_CONFIG_FILE."
	echo "warning: If the emulator fails to start or work correctly, try deleting the AVD and letting this script create it again."

else
	# Install AVD files
	install_android_sdk "$ANDROID_SIMULATOR_APILEVEL"
	install_android_sdk 36

	if [[ -f "$ANDROID_HOME/platform-tools/platform-tools/adb" ]]; then
		# It appears that the platform-tools 29.0.6 are extracting into an incorrect path
		mv "$ANDROID_HOME/platform-tools/platform-tools"/* "$ANDROID_HOME/platform-tools"
	fi

	AVD_IMAGE_NAME="system-images;android-$ANDROID_SIMULATOR_APILEVEL;google_apis_playstore;$ANDROID_SIMULATOR_ABI"

	# Create emulator
	echo "no" | "$LATEST_CMDLINE_TOOLS_PATH/bin/avdmanager" create avd -n "$AVD_NAME" \
	  --abi "$ANDROID_SIMULATOR_ABI" -k "$AVD_IMAGE_NAME" --sdcard 128M --force

	# based on https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/hosted?view=azure-devops&tabs=yaml#hardware
	# >> Agents that run macOS images are provisioned on Mac pros with a 3 core CPU, 14 GB of RAM, and 14 GB of SSD disk space.
	echo "hw.cpu.ncore=3" >> "$AVD_CONFIG_FILE"

	# Bump the heap size as the tests are stressing the application
	echo "vm.heapSize=256M" >> "$AVD_CONFIG_FILE"

	"$ANDROID_HOME/emulator/emulator" -list-avds

	echo "Checking for hardware acceleration"
	"$ANDROID_HOME/emulator/emulator" -accel-check
fi

# kickstart ADB
if "$ANDROID_HOME/platform-tools/adb" devices | grep -v 'List of devices attached' | grep device >/dev/null 2>&1 ; then
	# we have an existing device/emulator; restart it to avoid running first-time tasks
	"$ANDROID_HOME/platform-tools/adb" reboot

	# Wait for the emulator to finish booting
	source "$BUILD_SOURCESDIRECTORY/build/scripts/android-uitest-wait-systemui.sh" 500
else
	echo "Starting emulator"

	# Start emulator in background
	nohup "$ANDROID_HOME/emulator/emulator" -avd "$AVD_NAME" -skin 1280x800 -no-window -gpu swiftshader_indirect -no-snapshot -noaudio -no-boot-anim > "$UNO_UITEST_SCREENSHOT_PATH/android-emulator-log.txt" 2>&1 &

	# Wait for the emulator to finish booting
	source "$BUILD_SOURCESDIRECTORY/build/scripts/android-uitest-wait-systemui.sh" 500

	export ANDROID_SERIAL=$("$ANDROID_HOME/platform-tools/adb" devices | grep -v 'List of devices attached' | grep emulator- | awk '{print $1}' | tail -n 1)
fi

# Wait for the emulator to finish booting
source "$BUILD_SOURCESDIRECTORY/build/scripts/android-uitest-wait-systemui.sh" 500

"$ANDROID_HOME/platform-tools/adb" devices
echo "Emulator started"

echo "Device System Properties:"
"$ANDROID_HOME/platform-tools/adb" shell getprop

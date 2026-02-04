#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

export UNO_UITEST_SCREENSHOT_PATH=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/wasm
export UNO_UITEST_WASM_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.csproj

cd $BUILD_SOURCESDIRECTORY

dotnet publish -f net10.0-browserwasm -p:Configuration=Release $UNO_UITEST_WASM_PROJECT -p:UseNativeRendering=true -p:IsUiAutomationMappingEnabled=True -bl:$UNO_UITEST_SCREENSHOT_PATH/msbuild.binlog

WASM_OUT="$BUILD_SOURCESDIRECTORY/Uno.Gallery/bin/Release/net10.0-browserwasm/publish"
if [ ! -d "$WASM_OUT" ]; then
  WASM_OUT="$BUILD_SOURCESDIRECTORY/Uno.Gallery/bin/Release/net10.0-browserwasm/publish"
fi

echo "Contents of WASM output folder:"
ls -la "$BUILD_SOURCESDIRECTORY/Uno.Gallery/bin/Release/net10.0-browserwasm/publish"

echo "##vso[task.setvariable variable=UNO_UITEST_WASM_OUTPUT_PATH]$WASM_OUT"

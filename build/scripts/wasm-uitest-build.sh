#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

export UNO_UITEST_SCREENSHOT_PATH=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/wasm
export UNO_UITEST_WASM_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.csproj
export UNO_UITEST_WASM_PUBLISH_OUT=$BUILD_ARTIFACTSTAGINGDIRECTORY/wasm-uitest-publish

cd $BUILD_SOURCESDIRECTORY

dotnet publish -f net10.0-browserwasm -c Release $UNO_UITEST_WASM_PROJECT -p:UseNativeRendering=true -p:IsUiAutomationMappingEnabled=True -o "$UNO_UITEST_WASM_PUBLISH_OUT" -bl:$UNO_UITEST_SCREENSHOT_PATH/msbuild.binlog

WASM_OUT="$UNO_UITEST_WASM_PUBLISH_OUT/wwwroot"

echo "Contents of WASM output folder:"
ls -la "$WASM_OUT"

echo "##vso[task.setvariable variable=UNO_UITEST_WASM_OUTPUT_PATH]$WASM_OUT"

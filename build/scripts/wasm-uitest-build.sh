#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

export UNO_UITEST_SCREENSHOT_PATH=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/wasm
export UNO_UITEST_WASM_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.csproj

cd $BUILD_SOURCESDIRECTORY

dotnet publish -f net9.0-browserwasm -p:Configuration=Release $UNO_UITEST_WASM_PROJECT -p:IsUiAutomationMappingEnabled=True -bl:$UNO_UITEST_SCREENSHOT_PATH/msbuild.binlog

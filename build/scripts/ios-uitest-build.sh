#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

export UNO_UITEST_IOS_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery

cd $UNO_UITEST_IOS_PROJECT

dotnet build -f net9.0-ios -r iossimulator-x64 -p:TargetFrameworkOverride=net9.0-ios -c Release -p:UseNativeRendering=true -p:IsUiAutomationMappingEnabled=true -p:CodesignDisable=true -bl:$BUILD_ARTIFACTSTAGINGDIRECTORY/ios-app.binlog

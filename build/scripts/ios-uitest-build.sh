#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

export UNO_UITEST_IOS_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery

cd $UNO_UITEST_IOS_PROJECT

########
# Warning using net8.0-ios is required because of xamarin.uitest not supporting net9
########
dotnet build -p:TargetFrameworkOverride=net8.0-ios -r iossimulator-x64 -c Release -p:IsUiAutomationMappingEnabled=True -bl:$BUILD_ARTIFACTSTAGINGDIRECTORY/ios-app.binlog

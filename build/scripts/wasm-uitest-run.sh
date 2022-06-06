#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

export UNO_UITEST_TARGETURI=http://localhost:5000
export UNO_UITEST_DRIVERPATH_CHROME=$BUILD_SOURCESDIRECTORY/build/node_modules/chromedriver/lib/chromedriver
export UNO_UITEST_CHROME_BINARY_PATH=$BUILD_SOURCESDIRECTORY/build/node_modules/puppeteer/.local-chromium/linux-1002410/chrome-linux/chrome
export UNO_UITEST_SCREENSHOT_PATH=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/wasm
export UNO_UITEST_PLATFORM=Browser
export UNO_UITEST_CHROME_CONTAINER_MODE=true
export UNO_UITEST_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.UITest/Uno.Gallery.UITest.csproj
export UNO_UITEST_BINARY=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.UITest/bin/Release/net47/Uno.Gallery.UITest.dll
export UNO_UITEST_LOGFILE=$BUILD_ARTIFACTSTAGINGDIRECTORY/screenshots/wasm/nunit-log.txt
export UNO_UITEST_WASM_PROJECT=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.Wasm/Uno.Gallery.Wasm.csproj
export UNO_UITEST_WASM_OUTPUT_PATH=$BUILD_SOURCESDIRECTORY/Uno.Gallery/Uno.Gallery.Wasm/bin/Release/net5/dist/
export UNO_UITEST_NUNIT_VERSION=3.11.1
export UNO_UITEST_NUGET_URL=https://dist.nuget.org/win-x86-commandline/v5.7.0/nuget.exe

cd $BUILD_SOURCESDIRECTORY

msbuild /r /p:Configuration=Release $UNO_UITEST_PROJECT
dotnet build /r /p:Configuration=Release $UNO_UITEST_WASM_PROJECT /p:IsUiAutomationMappingEnabled=True

# Start the server
dotnet run --project $UNO_UITEST_WASM_PROJECT -c Release --no-build &

cd $BUILD_SOURCESDIRECTORY/build

npm i chromedriver@100.0.0
npm i puppeteer@14.0.0
wget $UNO_UITEST_NUGET_URL
mono nuget.exe install NUnit.ConsoleRunner -Version $UNO_UITEST_NUNIT_VERSION

mkdir -p $UNO_UITEST_SCREENSHOT_PATH

mono $BUILD_SOURCESDIRECTORY/build/NUnit.ConsoleRunner.$UNO_UITEST_NUNIT_VERSION/tools/nunit3-console.exe \
  --trace=Verbose --inprocess --agents=1 --workers=1 \
  $UNO_UITEST_BINARY || true

#!/bin/bash
export BUILD_SOURCESDIRECTORY=`pwd`/../..
export BUILD_ARTIFACTSTAGINGDIRECTORY=/tmp/uno-uitests-results
export UNO_UITEST_IOSBUNDLE_PATH=$BUILD_SOURCESDIRECTORY/Uno.Gallery/bin/Release/net10.0-ios/iossimulator-x64/Uno.Gallery.app

./ios-uitest-build.sh
./ios-uitest-run.sh

#!/bin/bash

if [ -z "$1" ]; then
    echo "Usage: $0 <PACKAGE FOLDER>"
    exit 1
fi

if [ -z "$2" ]; then
    echo "Usage: $0 <ARTIFACTS FOLDER>"
    exit 1
fi

ARTIFACT_DIRECTORY="$1"
PACKAGE_FOLDER="$2"
METRICS_FOLDER="$ARTIFACT_DIRECTORY/metrics"


mkdir -p "$METRICS_FOLDER"

echo "[] Target folder: $ARTIFACT_DIRECTORY exists"
echo "[] Measuring package size..."

folder_size=$(du -sh --block-size=1M "$PACKAGE_FOLDER" | cut -f1)
timestamp=$(date '+%d/%m/%Y %H:%M:%S')

echo "[] Writing output to metrics.json in $METRICS_FOLDER"

cat <<EOF > "$METRICS_FOLDER/metrics.json"
{
    "buildId": "${BUILD_ID:-unknown}",
    "commit": "${SOURCE_VERSION:-unknown}",
    "size": "${folder_size}",
    "isTrimmed": false,
    "timeStamp": "${timestamp}"
}
EOF

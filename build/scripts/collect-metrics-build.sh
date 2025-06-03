#!/bin/bash

if [ -z "$1" ]; then
    echo "Usage: $0 <PACKAGE FOLDER>"
    exit 1
fi

if [ -z "$2" ]; then
    echo "Usage: $0 <ARTIFACTS FOLDER>"
    exit 1
fi

if [ -z "$3" ]; then
    echo "Usage: $0 <PLATFORM>"
    exit 1
fi

ARTIFACT_DIRECTORY="$1"
PACKAGE_FOLDER="$2"
PLATFORM="$3"
METRICS_FOLDER="$ARTIFACT_DIRECTORY/metrics"


mkdir -p "$METRICS_FOLDER"

echo "[] Target folder: $ARTIFACT_DIRECTORY exists"
echo "[] Measuring package size..."

size_bytes=$(du -sb "$PACKAGE_FOLDER" | cut -f1)
size_mib=$((size_bytes / 1024 / 1024))

timestamp=$(date '+%d/%m/%Y %H:%M:%S')

echo "[] Writing output to metrics.json in $METRICS_FOLDER"

cat <<EOF > "$METRICS_FOLDER/metrics.json"
{
    "app": "Uno.Gallery",
    "platform": "WebAssembly",
    "buildId": "${BUILD_ID:-unknown}",
    "commit": "${SOURCE_VERSION:-unknown}",
    "size": "${size_mib}",
    "isTrimmed": false,
    "timeStamp": "${timestamp}"
}
EOF

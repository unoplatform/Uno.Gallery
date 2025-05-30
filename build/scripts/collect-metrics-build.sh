#!/bin/bash

if [ -z "$1" ]; then
    echo "Usage: $0 <BUILD FOLDER>"
    exit 1
fi

TARGET_FOLDER="$1"
METRICS_FOLDER="$(Build.ArtifactStagingDirectory)/metrics"

mkdir -p "$METRICS_FOLDER"

echo "[] Target folder: $TARGET_FOLDER exists"
echo "[] Measuring package size..."

folder_size=$(du -sh --block-size=1M "$TARGET_FOLDER" | cut -f1)
timestamp=$(date +"%T")

echo "[] Writing output to metrics.json in $METRICS_FOLDER"

cat <<EOF > "$METRICS_FOLDER/metrics.json"
{
    "buildId": "$(Build.BuildId)",
    "commit": "$(Build.SourceVersion)",
    "size": "${folder_size}",
    "isTrimmed": false,
    "timeStamp": "${timestamp}"
}
EOF

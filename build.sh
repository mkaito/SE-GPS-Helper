#!/usr/bin/bash
set -euo pipefail
IFS=$'\n\t'

rm -rf Upload
mkdir -p ./Upload/Data/Scripts/UDSEGPS
cp -t ./Upload/Data/Scripts/UDSEGPS ./Scripts/UDSEGPS/*.cs
rsync -rt ./Assets/ ./Upload/

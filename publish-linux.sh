#!/bin/sh
# Produces a self-contained Linux x64 build of SabreTools Studio in dist/linux-x64,
# with the SabreTools CLI bundled alongside it.
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
RUNTIME="${1:-linux-x64}"
CONFIGURATION="${2:-Release}"

# 1. Bundle a self-contained CLI so target machines don't need a .NET runtime
"$SCRIPT_DIR/build-cli.sh" "$SCRIPT_DIR/../SabreTools" net10.0 "$CONFIGURATION" "$RUNTIME"

# 2. Publish the GUI self-contained
OUTPUT="$SCRIPT_DIR/dist/$RUNTIME"
dotnet publish "$SCRIPT_DIR/src/SabreToolsStudio/SabreToolsStudio.csproj" \
    -c "$CONFIGURATION" -r "$RUNTIME" --self-contained true \
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "$OUTPUT"

echo ""
echo "SabreTools Studio published to $OUTPUT"

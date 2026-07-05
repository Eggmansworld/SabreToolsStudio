#!/bin/sh
# Publishes the SabreTools CLI from the sibling checkout into tools/sabretools
# so it gets bundled with SabreTools Studio builds.
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SABRETOOLS_REPO="${1:-$SCRIPT_DIR/../SabreTools}"
FRAMEWORK="${2:-net10.0}"
CONFIGURATION="${3:-Release}"

PROJECT="$SABRETOOLS_REPO/SabreTools/SabreTools.csproj"
if [ ! -f "$PROJECT" ]; then
    echo "SabreTools checkout not found at '$SABRETOOLS_REPO'." >&2
    exit 1
fi

dotnet publish "$PROJECT" -f "$FRAMEWORK" -c "$CONFIGURATION" -o "$SCRIPT_DIR/tools/sabretools"

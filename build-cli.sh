#!/bin/sh
# Publishes the SabreTools CLI from the sibling checkout into tools/sabretools
# so it gets bundled with SabreTools Studio builds.
# Pass a runtime id (e.g. linux-x64) as the 4th argument to produce a
# self-contained CLI for distributable builds; omit it for a
# framework-dependent CLI during development.
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SABRETOOLS_REPO="${1:-$SCRIPT_DIR/../SabreTools}"
FRAMEWORK="${2:-net10.0}"
CONFIGURATION="${3:-Release}"
RUNTIME="${4:-}"

PROJECT="$SABRETOOLS_REPO/SabreTools/SabreTools.csproj"
if [ ! -f "$PROJECT" ]; then
    echo "SabreTools checkout not found at '$SABRETOOLS_REPO'." >&2
    exit 1
fi

if [ -n "$RUNTIME" ]; then
    dotnet publish "$PROJECT" -f "$FRAMEWORK" -c "$CONFIGURATION" -r "$RUNTIME" --self-contained true -o "$SCRIPT_DIR/tools/sabretools"
else
    dotnet publish "$PROJECT" -f "$FRAMEWORK" -c "$CONFIGURATION" -o "$SCRIPT_DIR/tools/sabretools"
fi

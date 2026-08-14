#!/usr/bin/env bash
# Publishes the generator as a single-file self-contained executable for the given
# runtime(s) into /bin/<rid>. Run from the repo root (or folder containing config.json).
#
#   bash scripts/publish.sh            # publishes ALL supported RIDs
#   bash scripts/publish.sh linux-x64  # publishes a single RID
#   bash scripts/publish.sh linux-x64 linux-arm64  # publishes several

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJ="$ROOT/Kolpa.Generator/Kolpa.Generator.csproj"
OUT="$ROOT/bin"

# If no RID given, publish all.
RIDS=("${@:-win-x64 linux-x64 linux-arm64 osx-x64 osx-arm64}")

for rid in "${RIDS[@]}"; do
    echo ">> Publishing $rid -> $OUT/$rid"
    dotnet publish "$PROJ" -c Release -r "$rid" --self-contained true -o "$OUT/$rid" \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -p:DebugType=embedded \
    -p:InvariantGlobalization=true \
    -p:UseAppHost=true
done

echo ">> Done. Executables written under $OUT:"
ls -1 "$OUT"/*/Kolpa.Generator* 2>/dev/null || true

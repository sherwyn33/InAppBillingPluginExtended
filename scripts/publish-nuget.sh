#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT_DIR/src/Plugin.InAppBilling/Plugin.InAppBilling.csproj"
OUTPUT_DIR="$ROOT_DIR/artifacts/nuget"
SOURCE_URL="${NUGET_SOURCE_URL:-https://api.nuget.org/v3/index.json}"
PUBLISH=false

usage() {
  cat <<'USAGE'
Usage: scripts/publish-nuget.sh [--publish] [--version VERSION]

Builds and validates Plugin.InAppBilling.Extended NuGet packages.

Options:
  --publish          Push the .nupkg and matching .snupkg to NuGet.org.
                     Requires NUGET_API_KEY in the environment.
  --version VERSION  Override the package version for this run.

Environment:
  NUGET_API_KEY      NuGet API key used only when --publish is provided.
  NUGET_SOURCE_URL   Package source URL. Defaults to NuGet.org.
USAGE
}

MSBUILD_ARGS=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    --publish)
      PUBLISH=true
      shift
      ;;
    --version)
      if [[ $# -lt 2 ]]; then
        echo "Missing value for --version" >&2
        exit 2
      fi
      MSBUILD_ARGS+=("-p:Version=$2" "-p:PackageVersion=$2" "-p:AssemblyVersion=$2.0" "-p:AssemblyFileVersion=$2.0")
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 2
      ;;
  esac
done

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

echo "Building Release package..."
dotnet build "$PROJECT" -c Release "${MSBUILD_ARGS[@]+"${MSBUILD_ARGS[@]}"}" -p:PackageOutputPath="$OUTPUT_DIR"

PACKAGES=()
while IFS= read -r package; do
  PACKAGES+=("$package")
done < <(find "$OUTPUT_DIR" -maxdepth 1 -type f -name '*.nupkg' ! -name '*.symbols.nupkg' | sort)

SYMBOL_PACKAGES=()
while IFS= read -r package; do
  SYMBOL_PACKAGES+=("$package")
done < <(find "$OUTPUT_DIR" -maxdepth 1 -type f -name '*.snupkg' | sort)

if [[ ${#PACKAGES[@]} -ne 1 ]]; then
  echo "Expected exactly one .nupkg in $OUTPUT_DIR, found ${#PACKAGES[@]}" >&2
  exit 1
fi

if [[ ${#SYMBOL_PACKAGES[@]} -ne 1 ]]; then
  echo "Expected exactly one .snupkg in $OUTPUT_DIR, found ${#SYMBOL_PACKAGES[@]}" >&2
  exit 1
fi

echo "Validating package contents..."
unzip -tq "${PACKAGES[0]}"
unzip -tq "${SYMBOL_PACKAGES[0]}"

echo "Package: ${PACKAGES[0]}"
echo "Symbols: ${SYMBOL_PACKAGES[0]}"

if [[ "$PUBLISH" != true ]]; then
  echo "Validation complete. Re-run with --publish to push to NuGet."
  exit 0
fi

if [[ -z "${NUGET_API_KEY:-}" ]]; then
  echo "NUGET_API_KEY is not set. Export it before publishing." >&2
  exit 1
fi

echo "Publishing to $SOURCE_URL..."
dotnet nuget push "${PACKAGES[0]}" \
  --source "$SOURCE_URL" \
  --api-key "$NUGET_API_KEY" \
  --skip-duplicate \
  --timeout 600

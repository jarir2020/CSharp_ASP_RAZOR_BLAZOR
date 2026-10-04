#!/usr/bin/env bash
set -euo pipefail

# Publishing creates a deployable directory. It does not deploy credentials,
# database data, or environment-specific secrets.
project_root="$(cd "$(dirname "$0")/.." && pwd)"
output_dir="$project_root/artifacts/production-lab"

rm -rf "$output_dir"
dotnet publish "$project_root/ProductionLab/ProductionLab.csproj" \
  --configuration Release \
  --output "$output_dir"

echo "Published ProductionLab to $output_dir"

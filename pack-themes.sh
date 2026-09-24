#!/usr/bin/env bash
# Packs Uno.Themes, Material, Simple and Cupertino from an Uno.Themes checkout into ./packages, the local
# feed nuget.config reads. The macOS / Linux twin of pack-themes.ps1 (see there for why each step is needed).
# Usage: ./pack-themes.sh [themes-path] [version]
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
themes="$(cd "${1:-$root/../uno.themes}" && pwd)"
version="${2:-9.0.0-cupertino-v2-local}"
out="$root/packages"
mkdir -p "$out"
rm -f "$out"/*.nupkg

# NuGet never refreshes a cached version: drop it so a repack of the same version is picked up.
cache="${NUGET_PACKAGES:-$HOME/.nuget/packages}"
for id in uno.themes.winui uno.material.winui uno.simple.winui uno.cupertino.winui; do
	rm -rf "$cache/$id/$version"
done

# From the checkout so its global.json picks its SDKs; build (not pack) because of GeneratePackageOnBuild.
cd "$themes"
for project in Uno.Themes/Uno.Themes.WinUI.csproj Uno.Material/Uno.Material.WinUI.csproj \
	Uno.Simple.WinUI/Uno.Simple.WinUI.csproj Uno.Cupertino/Uno.Cupertino.WinUI.csproj; do
	dotnet build "src/library/$project" -c Release -p:TargetFrameworkOverride=desktop \
		"-p:PackageVersion=$version" "-p:PackageOutputPath=$out" -nologo -v q
done

echo "Packed $version from $(git rev-parse --abbrev-ref HEAD)@$(git rev-parse --short HEAD) into $out"

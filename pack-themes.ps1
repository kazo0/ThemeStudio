# Packs Uno.Themes, Material, Simple and Cupertino from an Uno.Themes checkout into ./packages, the local
# feed nuget.config reads. All four come from the same checkout: Material and Simple use Uno.Themes internals,
# so they must match the Uno.Themes.WinUI that Cupertino needs.
# Runs in Windows PowerShell and in PowerShell 7 on macOS / Linux; pack-themes.sh does the same without PowerShell.
param(
	[string]$ThemesPath = (Join-Path $PSScriptRoot '../uno.themes'),
	[string]$Version = '9.0.0-cupertino-v2-local'
)
$ErrorActionPreference = 'Stop'
$ThemesPath = (Resolve-Path $ThemesPath).Path
$out = Join-Path $PSScriptRoot 'packages'
New-Item -ItemType Directory -Force $out | Out-Null
Remove-Item (Join-Path $out '*.nupkg') -ErrorAction SilentlyContinue

# NuGet never refreshes a cached version: drop it so a repack of the same version is picked up.
$cache = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $HOME '.nuget/packages' }
foreach ($id in 'uno.themes.winui', 'uno.material.winui', 'uno.simple.winui', 'uno.cupertino.winui') {
	Remove-Item (Join-Path (Join-Path $cache $id) $Version) -Recurse -Force -ErrorAction SilentlyContinue
}

# Run from the checkout so its global.json selects its own .NET and Uno SDKs.
# TargetFrameworkOverride=desktop builds net10.0 only, which both the desktop and WebAssembly heads consume.
# Build, not pack: the repo sets GeneratePackageOnBuild for Release, and with it `dotnet pack` skips the build
# and packs whatever stale output is on disk. Its CI builds with PackageOutputPath the same way.
Push-Location $ThemesPath
try {
	foreach ($project in 'Uno.Themes/Uno.Themes.WinUI.csproj', 'Uno.Material/Uno.Material.WinUI.csproj',
		'Uno.Simple.WinUI/Uno.Simple.WinUI.csproj', 'Uno.Cupertino/Uno.Cupertino.WinUI.csproj') {
		dotnet build "src/library/$project" -c Release -p:TargetFrameworkOverride=desktop "-p:PackageVersion=$Version" "-p:PackageOutputPath=$out" -nologo -v q
		if ($LASTEXITCODE -ne 0) { throw "Packing $project failed." }
	}
	$source = "$(git rev-parse --abbrev-ref HEAD)@$(git rev-parse --short HEAD)"
}
finally {
	Pop-Location
}
Write-Output "Packed $Version from $source into $out"

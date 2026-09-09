$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
	dotnet run --project ThemeStudio.csproj -f net10.0-browserwasm --launch-profile 'Theme Studio (WebAssembly)'
	if ($LASTEXITCODE -ne 0) { throw "Theme Studio exited with code $LASTEXITCODE." }
}
finally {
	Pop-Location
}

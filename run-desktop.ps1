$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
	dotnet run --project ThemeStudio.csproj -f net10.0-desktop --launch-profile 'Theme Studio (Desktop)'
	if ($LASTEXITCODE -ne 0) { throw "Theme Studio exited with code $LASTEXITCODE." }
}
finally {
	Pop-Location
}

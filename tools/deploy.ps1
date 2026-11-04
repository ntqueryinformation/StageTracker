# Build StageTracker and copy it into the r2modman/TSMM profile's plugins folder.
# Close the game first — Windows locks the DLL while it is loaded.
param(
    [string]$ProfilePlugins = "$env:APPDATA\r2modmanPlus-local\RiskOfRain2\profiles\lookingglass\BepInEx\plugins"
)

$ErrorActionPreference = "Stop"
dotnet build "$PSScriptRoot\..\src\StageTracker.csproj" -c Release
if ($LASTEXITCODE -ne 0) { exit 1 }

New-Item -ItemType Directory -Force -Path "$ProfilePlugins\StageTracker" | Out-Null
Copy-Item "$PSScriptRoot\..\Release\plugins\StageTracker\StageTracker.dll" "$ProfilePlugins\StageTracker\" -Force
Write-Host "`nDeployed to: $ProfilePlugins\StageTracker" -ForegroundColor Green

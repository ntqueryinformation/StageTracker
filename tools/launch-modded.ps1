# Launch Risk of Rain 2 modded with the given mod-manager profile, without going through
# the mod manager UI. Temporarily points the game's doorstop loader at the profile's
# BepInEx (exactly what Thunderstore Mod Manager does when you press "Start modded"),
# then restores the original file when the game closes. Steam must be running.
param(
    [string]$Profile = "$env:APPDATA\r2modmanPlus-local\RiskOfRain2\profiles\lookingglass",
    [string]$GameDir = "D:\SteamLibrary\steamapps\common\Risk of Rain 2"
)

$ErrorActionPreference = "Stop"
$ini = Join-Path $GameDir "doorstop_config.ini"
$preloader = Join-Path $Profile "BepInEx\core\BepInEx.Preloader.dll"

if (!(Test-Path $preloader)) {
    throw "BepInEx preloader not found in profile: $preloader"
}

Copy-Item $ini "$ini.stagetracker.bak" -Force
$normalizedProfile = ($Profile -replace '\\', '/')
(Get-Content $ini) -replace 'target_assembly=.*', "target_assembly=$normalizedProfile/BepInEx/core/BepInEx.Preloader.dll" |
    Set-Content $ini

Write-Host "Launching modded with profile: $Profile (doorstop restored automatically on exit)"
try {
    $process = Start-Process -FilePath (Join-Path $GameDir "Risk of Rain 2.exe") -WorkingDirectory $GameDir -PassThru
    $process.WaitForExit()
}
finally {
    Copy-Item "$ini.stagetracker.bak" $ini -Force
    Remove-Item "$ini.stagetracker.bak" -Force -ErrorAction SilentlyContinue
    Write-Host "doorstop_config.ini restored."
}

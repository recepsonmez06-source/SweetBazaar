# Generates the shipped levels in batch mode and writes Game\Assets\_Game\Resources\Levels\levels.json.
# Usage: powershell -File tools\build-levels.ps1 [-Count 200] [-Rebuild]
# Append-only by default: existing levels stay untouched, only missing numbers are generated.
# -Rebuild regenerates ALL levels (changes what players see; only before release or on purpose).
# The Unity editor must NOT have the project open (project lock).
param(
    [int]$Count = 200,
    [switch]$Rebuild,
    [string]$OutDir = (Join-Path $env:TEMP 'SweetBazaar-tests')
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\unity.ps1"

$project = Get-ProjectPath
Assert-EditorClosed $project

New-Item -ItemType Directory -Force $OutDir | Out-Null
$log = Join-Path $OutDir 'unity-build-levels.log'
Remove-Item $log -ErrorAction SilentlyContinue

$unityArgs = @(
    '-batchmode', '-nographics', '-quit',
    '-projectPath', "`"$project`"",
    '-executeMethod', 'SweetBazaar.EditorTools.LevelPackBuilder.BuildFromCommandLine',
    '-levelCount', $Count,
    '-logFile', "`"$log`""
)
if ($Rebuild) { $unityArgs += '-rebuild' }

$exitCode = Invoke-Unity $project $unityArgs
Write-Host "Unity exit code: $exitCode  (0 = ok)"
Select-String -Path $log -Pattern 'Level pack (written|build failed)' | ForEach-Object { Write-Host $_.Line.Trim() }
Write-Host "Log: $log"
exit $exitCode

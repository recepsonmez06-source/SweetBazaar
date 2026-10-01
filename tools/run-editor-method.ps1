# Runs a static editor method of the Unity project in batch mode, e.g. creating the scene.
# Usage: powershell -File tools\run-editor-method.ps1 -Method SweetBazaar.EditorTools.SceneBuilder.BuildFromCommandLine [-ExtraArgs '-shotDir','C:\x']
# The method must call EditorApplication.Exit itself. The Unity editor must NOT have the project open.
param(
    [Parameter(Mandatory = $true)][string]$Method,
    [string[]]$ExtraArgs = @(),
    [string]$LogName = 'unity-method.log',
    # Rendering needs the graphics device; use -Graphics for tools that draw (screenshots).
    [switch]$Graphics,
    [string]$OutDir = (Join-Path $env:TEMP 'SweetBazaar-tests')
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\unity.ps1"

$project = Get-ProjectPath
Assert-EditorClosed $project

New-Item -ItemType Directory -Force $OutDir | Out-Null
$log = Join-Path $OutDir $LogName
Remove-Item $log -ErrorAction SilentlyContinue

$unityArgs = @('-batchmode')
if (-not $Graphics) { $unityArgs += '-nographics' }
$unityArgs += @('-quit', '-projectPath', "`"$project`"", '-executeMethod', $Method, '-logFile', "`"$log`"")
$unityArgs += $ExtraArgs

$exitCode = Invoke-Unity $project $unityArgs
Write-Host "Unity exit code: $exitCode  (0 = ok)"
Select-String -Path $log -Pattern 'failed:|error CS|Scene build|Main scene created|Preview screenshots written|Exception' |
    Select-Object -First 15 | ForEach-Object { Write-Host $_.Line.Trim() }
Write-Host "Log: $log"
exit $exitCode

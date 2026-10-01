# Runs the Unity tests in batch mode (no editor window) and prints a summary.
# Usage: powershell -File tools\run-tests.ps1 [-Platform EditMode|PlayMode]
# The Unity editor must NOT have the project open (project lock).
param(
    [ValidateSet('EditMode', 'PlayMode')]
    [string]$Platform = 'EditMode',
    [string]$OutDir = (Join-Path $env:TEMP 'SweetBazaar-tests')
)

$ErrorActionPreference = 'Stop'

$project = Join-Path (Split-Path -Parent $PSScriptRoot) 'Game'
$versionFile = Join-Path $project 'ProjectSettings\ProjectVersion.txt'
$version = (Select-String -Path $versionFile -Pattern '^m_EditorVersion:\s*(\S+)').Matches[0].Groups[1].Value
$unity = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"

if (-not (Test-Path $unity)) { throw "Unity $version not found: $unity" }
if (Test-Path (Join-Path $project 'Temp\UnityLockfile')) {
    throw 'The project is open in a Unity editor (Temp\UnityLockfile exists). Close the editor first.'
}

New-Item -ItemType Directory -Force $OutDir | Out-Null
$results = Join-Path $OutDir "results-$Platform.xml"
$log = Join-Path $OutDir "unity-$Platform.log"
Remove-Item $results, $log -ErrorAction SilentlyContinue

$unityArgs = @(
    '-batchmode', '-nographics',
    '-projectPath', "`"$project`"",
    '-runTests', '-testPlatform', $Platform,
    '-testResults', "`"$results`"",
    '-logFile', "`"$log`""
)
# Not Start-Process -Wait: it also waits for helper processes Unity leaves behind (e.g. the Android ADB server).
$process = Start-Process -FilePath $unity -ArgumentList $unityArgs -PassThru
$null = $process.Handle   # keeps the exit code readable after the process ends
$process.WaitForExit()
Write-Host "Unity exit code: $($process.ExitCode)  (0 = all passed, 2 = test failures, other = error)"

if (Test-Path $results) {
    $run = ([xml](Get-Content $results)).'test-run'
    Write-Host "Total: $($run.total)  Passed: $($run.passed)  Failed: $($run.failed)  Skipped: $($run.skipped)"
} else {
    Write-Host "No results file was written. See log: $log"
}
Write-Host "Results: $results"
Write-Host "Log:     $log"
exit $process.ExitCode

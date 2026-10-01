# Runs the Unity tests in batch mode (no editor window) and prints a summary.
# Usage: powershell -File tools\run-tests.ps1 [-Platform EditMode|PlayMode] [-Filter <name>]
# The Unity editor must NOT have the project open (project lock).
param(
    [ValidateSet('EditMode', 'PlayMode')]
    [string]$Platform = 'EditMode',
    [string]$OutDir = (Join-Path $env:TEMP 'SweetBazaar-tests'),
    # Only run tests whose name matches (also the way to run [Explicit] tests such as SolverBenchmark).
    [string]$Filter = ''
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\unity.ps1"

$project = Get-ProjectPath
Assert-EditorClosed $project

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
if ($Filter) { $unityArgs += @('-testFilter', "`"$Filter`"") }

$exitCode = Invoke-Unity $project $unityArgs
Write-Host "Unity exit code: $exitCode  (0 = all passed, 2 = test failures, other = error)"

if (Test-Path $results) {
    $run = ([xml](Get-Content $results)).'test-run'
    Write-Host "Total: $($run.total)  Passed: $($run.passed)  Failed: $($run.failed)  Skipped: $($run.skipped)"
} else {
    Write-Host "No results file was written. See log: $log"
}
Write-Host "Results: $results"
Write-Host "Log:     $log"
exit $exitCode

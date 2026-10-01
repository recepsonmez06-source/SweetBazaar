# Builds an installable Android APK (debug-signed, for testing on a phone) in Game\Builds\Android\.
# Usage: powershell -File tools\build-apk.ps1
# The first build takes several minutes (IL2CPP + Gradle) and needs internet for Gradle dependencies.
# The Unity editor must NOT have the project open.
param(
    [string]$OutDir = (Join-Path $env:TEMP 'SweetBazaar-tests')
)

$ErrorActionPreference = 'Stop'

& "$PSScriptRoot\run-editor-method.ps1" -Method SweetBazaar.EditorTools.AndroidBuilder.BuildFromCommandLine `
    -ExtraArgs '-buildTarget', 'Android' -LogName 'unity-build-apk.log' -OutDir $OutDir
$code = $LASTEXITCODE

$project = Join-Path (Split-Path -Parent $PSScriptRoot) 'Game'
$apk = Join-Path $project 'Builds\Android\SweetBazaar-debug.apk'
Select-String -Path (Join-Path $OutDir 'unity-build-apk.log') -Pattern 'APK built|APK build failed|BuildFailedException|error CS|Gradle|FAILURE' |
    Select-Object -First 10 | ForEach-Object { Write-Host $_.Line.Trim() }
if ($code -eq 0 -and (Test-Path $apk)) { Write-Host "APK: $apk ($([int]((Get-Item $apk).Length / 1MB)) MB)" }
exit $code

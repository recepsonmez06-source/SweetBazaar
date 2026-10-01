# Renders the game screen in a few situations to PNG files (levels, selection, packed boxes, win, stuck, Turkish).
# Usage: powershell -File tools\preview.ps1 [-OutDir <folder>]
# Look at the PNGs to check the layout without playing. The Unity editor must NOT have the project open.
param(
    [string]$OutDir = (Join-Path $env:TEMP 'SweetBazaar-preview')
)

$ErrorActionPreference = 'Stop'

Remove-Item $OutDir -Recurse -Force -ErrorAction SilentlyContinue
& "$PSScriptRoot\run-editor-method.ps1" -Method SweetBazaar.EditorTools.PreviewScreenshots.CaptureFromCommandLine `
    -ExtraArgs '-shotDir', $OutDir -Graphics -LogName 'unity-preview.log'
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Get-ChildItem $OutDir -Filter *.png | Where-Object { $_.Name -ne 'warm-up.png' } | ForEach-Object { Write-Host $_.FullName }

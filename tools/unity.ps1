# Shared helpers for the scripts in tools\. Dot-source it:  . "$PSScriptRoot\unity.ps1"

function Get-ProjectPath {
    Join-Path (Split-Path -Parent $PSScriptRoot) 'Game'
}

# Path of the Unity editor that matches the project's ProjectVersion.txt.
function Get-UnityExe([string]$Project) {
    $versionFile = Join-Path $Project 'ProjectSettings\ProjectVersion.txt'
    $version = (Select-String -Path $versionFile -Pattern '^m_EditorVersion:\s*(\S+)').Matches[0].Groups[1].Value
    $unity = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
    if (-not (Test-Path $unity)) { throw "Unity $version not found: $unity" }
    $unity
}

function Assert-EditorClosed([string]$Project) {
    if (Test-Path (Join-Path $Project 'Temp\UnityLockfile')) {
        throw 'The project is open in a Unity editor (Temp\UnityLockfile exists). Close the editor first.'
    }
}

# Runs Unity with the given arguments and returns its exit code.
# Not Start-Process -Wait: it also waits for helper processes Unity leaves behind (e.g. the Android ADB server).
function Invoke-Unity([string]$Project, [string[]]$UnityArgs) {
    $process = Start-Process -FilePath (Get-UnityExe $Project) -ArgumentList $UnityArgs -PassThru
    $null = $process.Handle   # keeps the exit code readable after the process ends
    $process.WaitForExit()
    $process.ExitCode
}

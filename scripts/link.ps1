param (
[Parameter(Mandatory = $true)]
[string]$GameDir
)

$ErrorActionPreference = "Stop"

$ProjectRoot = Join-Path $PSScriptRoot ".."
$RefsDir = Join-Path $ProjectRoot "TCGProfiler\Refs"
$BepInExDir = Join-Path $GameDir "BepInEx\core"
$ManagedDir = Join-Path $GameDir "Card Shop Simulator_Data\Managed"

$GameReferences = @(
"Assembly-CSharp.dll",
"Unity.InputSystem.dll"
)
$ModReferences = @(
"BepInEx.dll"
)

function Test-IsElevated {
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)

return $principal.IsInRole(
[Security.Principal.WindowsBuiltInRole]::Administrator
)
}

function New-ReferenceFile {
param (
[Parameter(Mandatory = $true)]
[string]$Source,

[Parameter(Mandatory = $true)]
[string]$Destination
)

if (Test-Path $Destination) {
Remove-Item $Destination -Force
}

if (Test-IsElevated) {
try {
New-Item `
                -ItemType SymbolicLink `
                -Path $Destination `
                -Target $Source `
                -ErrorAction Stop | Out-Null

Write-Host "Symlinked:"
Write-Host "  $Destination -> $Source"
return
}
catch {
Write-Warning "Failed to create symbolic link: $($_.Exception.Message)"
}
}

try {
New-Item `
            -ItemType HardLink `
            -Path $Destination `
            -Target $Source `
            -ErrorAction Stop | Out-Null

Write-Host "Hard linked:"
Write-Host "  $Destination => $Source"
return
}
catch {
Write-Warning "Failed to create hard link: $($_.Exception.Message)"
}

Copy-Item `
        -Path $Source `
        -Destination $Destination `
        -Force `
        -ErrorAction Stop

Write-Host "Copied:"
Write-Host "  $Source -> $Destination"
}

if (-not (Test-Path $ManagedDir -PathType Container)) {
throw "Managed directory not found: $ManagedDir"
}

if (-not (Test-Path $RefsDir -PathType Container)) {
New-Item -ItemType Directory -Path $RefsDir | Out-Null
}

foreach ($Reference in $GameReferences) {
$Source = Join-Path $ManagedDir $Reference
$Destination = Join-Path $RefsDir $Reference

if (-not (Test-Path $Source -PathType Leaf)) {
throw "Reference not found: $Source"
}

New-ReferenceFile `
        -Source $Source `
        -Destination $Destination
}

foreach ($Reference in $ModReferences) {
    $Source = Join-Path $BepInExDir $Reference
    $Destination = Join-Path $RefsDir $Reference

    if (-not (Test-Path $Source -PathType Leaf)) {
        throw "Reference not found: $Source"
    }

    New-ReferenceFile `
        -Source $Source `
        -Destination $Destination
}

Write-Host ""
Write-Host "Reference files prepared successfully."

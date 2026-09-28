param(
[Parameter(Mandatory = $true)]
[string]$Version
)

$ErrorActionPreference = "Stop"

$ProjectRoot = Join-Path $PSScriptRoot ".."

dotnet build `
    -c Release `
    -p:Version=$Version `
    -p:InformationalVersion=$Version

New-Item -ItemType Directory -Path (Join-Path $ProjectRoot "releases") -Force | Out-Null

Compress-Archive `
    -Path (Join-Path $ProjectRoot "TCGProfiler\bin\Release\netstandard2.1\TCGProfiler.dll") `
    -DestinationPath (Join-Path $ProjectRoot "releases\TCGProfiler-$Version.zip") `
    -CompressionLevel Optimal `
    -Force

# Produces a self-contained Windows x64 build of SabreTools Studio in dist/win-x64,
# with the SabreTools CLI bundled alongside it.
param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# 1. Bundle a self-contained CLI so target machines don't need a .NET runtime
& (Join-Path $PSScriptRoot "build-cli.ps1") -Runtime $Runtime
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# 2. Publish the GUI self-contained
$output = Join-Path $PSScriptRoot "dist\$Runtime"
dotnet publish (Join-Path $PSScriptRoot "src\SabreToolsStudio\SabreToolsStudio.csproj") `
    -c $Configuration -r $Runtime --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $output
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "SabreTools Studio published to $output"

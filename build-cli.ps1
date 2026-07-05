# Publishes the SabreTools CLI from the sibling checkout into tools/sabretools
# so it gets bundled with SabreTools Studio builds.
param(
    [string]$SabreToolsRepo = (Join-Path $PSScriptRoot "..\SabreTools"),
    [string]$Framework = "net10.0",
    [string]$Configuration = "Release"
)

$project = Join-Path $SabreToolsRepo "SabreTools\SabreTools.csproj"
if (-not (Test-Path $project)) {
    Write-Error "SabreTools checkout not found at '$SabreToolsRepo'. Pass -SabreToolsRepo."
    exit 1
}

$output = Join-Path $PSScriptRoot "tools\sabretools"
dotnet publish $project -f $Framework -c $Configuration -o $output

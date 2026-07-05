# Publishes the SabreTools CLI from the sibling checkout into tools/sabretools
# so it gets bundled with SabreTools Studio builds.
# Pass -Runtime (e.g. win-x64, linux-x64) to produce a self-contained CLI for
# distributable builds; omit it for a framework-dependent CLI during development.
param(
    [string]$SabreToolsRepo = (Join-Path $PSScriptRoot "..\SabreTools"),
    [string]$Framework = "net10.0",
    [string]$Configuration = "Release",
    [string]$Runtime = ""
)

$project = Join-Path $SabreToolsRepo "SabreTools\SabreTools.csproj"
if (-not (Test-Path $project)) {
    Write-Error "SabreTools checkout not found at '$SabreToolsRepo'. Pass -SabreToolsRepo."
    exit 1
}

$output = Join-Path $PSScriptRoot "tools\sabretools"
$publishArgs = @($project, "-f", $Framework, "-c", $Configuration, "-o", $output)
if ($Runtime) {
    $publishArgs += @("-r", $Runtime, "--self-contained", "true")
}

dotnet publish @publishArgs

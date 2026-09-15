#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $GalleryManifestPath,

    [Parameter(Mandatory)]
    [string] $OutputPath,

    [string] $UpstreamManifestPath = '',
    [string] $CoveragePath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not $UpstreamManifestPath) {
    $UpstreamManifestPath = Join-Path $repoRoot 'build\manifest-fixtures\upstream-features-v1.json'
}
if (-not $CoveragePath) {
    $CoveragePath = Join-Path $repoRoot 'docs\catalog\feature-coverage-v1.json'
}

$comparisonError = $null
try {
    & (Join-Path $PSScriptRoot 'compare-feature-coverage.ps1') `
        -GalleryManifestPath $GalleryManifestPath `
        -UpstreamManifestPath $UpstreamManifestPath `
        -CoveragePath $CoveragePath `
        -OutputPath $OutputPath
} catch {
    $comparisonError = $_
}

if (-not (Test-Path $OutputPath -PathType Leaf)) {
    throw 'Feature coverage comparator did not produce a report.'
}
$reportJson = Get-Content $OutputPath -Raw
$reportSchema = Join-Path $repoRoot 'docs\catalog\feature-coverage-report-v1.schema.json'
if (-not ($reportJson | Test-Json -SchemaFile $reportSchema)) {
    throw 'Feature coverage report does not match schema version 1.'
}

if ($null -ne $comparisonError) {
    Write-Host "##vso[task.logissue type=warning]$($comparisonError.Exception.Message)"
    Write-Warning 'Feature coverage drift is advisory until upstream repositories publish authoritative manifests.'
}

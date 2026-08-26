#Requires -Version 7.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $GeneratedSourcePath,

    [Parameter(Mandatory)]
    [string] $OutputDirectory,

    [string] $TargetName = 'Desktop'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$exportScript = Join-Path $PSScriptRoot 'export-sample-manifest.ps1'
$compareScript = Join-Path $PSScriptRoot 'compare-feature-coverage.ps1'
$sampleSchema = Join-Path $repoRoot 'docs\catalog\sample-manifest-v1.schema.json'
$upstreamSchema = Join-Path $repoRoot 'docs\catalog\upstream-feature-manifest-v1.schema.json'
$coverageSchema = Join-Path $repoRoot 'docs\catalog\feature-coverage-v1.schema.json'
$baselinePath = Join-Path $repoRoot 'docs\catalog\sample-manifest-baseline-v1.json'
$baselineSchema = Join-Path $repoRoot 'docs\catalog\sample-manifest-baseline-v1.schema.json'
$upstreamManifest = Join-Path $repoRoot 'build\manifest-fixtures\upstream-features-v1.json'
$coverageManifest = Join-Path $repoRoot 'docs\catalog\feature-coverage-v1.json'
$contractGalleryPath = Join-Path $repoRoot 'build\manifest-fixtures\comparator-gallery-v1.json'
$contractUpstreamPath = Join-Path $repoRoot 'build\manifest-fixtures\comparator-upstream-v1.json'
$contractCoveragePath = Join-Path $repoRoot 'build\manifest-fixtures\comparator-coverage-v1.json'

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$manifestPath = Join-Path $OutputDirectory 'sample-manifest.json'
$scratchDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "uno-gallery-manifest-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $scratchDirectory | Out-Null
$invalidCoveragePath = Join-Path $scratchDirectory 'invalid-coverage.json'
$previewUpstreamPath = Join-Path $scratchDirectory 'preview-upstream.json'
$previewCoveragePath = Join-Path $scratchDirectory 'preview-coverage.json'
$invalidSourcePath = Join-Path $scratchDirectory 'invalid-source.json'

& $exportScript -GeneratedSourcePath $GeneratedSourcePath -OutputPath $manifestPath

foreach ($pair in @(
    @($manifestPath, $sampleSchema),
    @($upstreamManifest, $upstreamSchema),
    @($coverageManifest, $coverageSchema),
    @($baselinePath, $baselineSchema)
)) {
    $json = Get-Content $pair[0] -Raw
    $schemaErrors = @()
    $isValid = $json | Test-Json -SchemaFile $pair[1] -ErrorAction SilentlyContinue -ErrorVariable +schemaErrors
    if (-not $isValid) {
        throw "JSON schema validation failed for '$($pair[0])': $([string]::Join(' | ', $schemaErrors))"
    }
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json -Depth 100
$baseline = Get-Content $baselinePath -Raw | ConvertFrom-Json -Depth 10
$targetProperty = $baseline.targets.PSObject.Properties[$TargetName]
if ($baseline.schemaVersion -ne 1 -or $null -eq $targetProperty) {
    throw "Sample manifest baseline has no schema-v1 target '$TargetName'."
}
$targetBaseline = $targetProperty.Value
if ([int]$targetBaseline.minimumSampleCount -lt 1) {
    throw "Sample manifest baseline target '$TargetName' requires a positive minimum count."
}
if (@($manifest.samples).Count -lt [int]$targetBaseline.minimumSampleCount) {
    throw "Manifest target '$TargetName' contains $(@($manifest.samples).Count) samples; baseline requires at least $($targetBaseline.minimumSampleCount)."
}
$manifestSlugs = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($sample in $manifest.samples) {
    [void]$manifestSlugs.Add([string]$sample.slug)
}
foreach ($requiredSlug in $targetBaseline.requiredSlugs) {
    if (-not $manifestSlugs.Contains([string]$requiredSlug)) {
        throw "Manifest baseline target '$TargetName' requires missing slug '$requiredSlug'."
    }
}

$invalidCoverage = Get-Content $contractCoveragePath -Raw | ConvertFrom-Json -Depth 100
$invalidCoverage.features[0].gallerySlugs = @('missing-sample-slug')
$invalidCoverage | ConvertTo-Json -Depth 20 | Set-Content $invalidCoveragePath -Encoding utf8NoBOM
$invalidWasRejected = $false
try {
    & $compareScript `
        -GalleryManifestPath $contractGalleryPath `
        -UpstreamManifestPath $contractUpstreamPath `
        -CoveragePath $invalidCoveragePath `
        -Quiet
} catch {
    if ($_.Exception.Message -notmatch "missing-sample-slug") {
        throw
    }
    $invalidWasRejected = $true
}
if (-not $invalidWasRejected) {
    throw 'The coverage comparator accepted a missing Gallery slug.'
}

$previewUpstream = Get-Content $contractUpstreamPath -Raw | ConvertFrom-Json -Depth 100
$previewCoverage = Get-Content $contractCoveragePath -Raw | ConvertFrom-Json -Depth 100
$previewFeatureId = [string]$previewUpstream.features[0].id
$previewUpstream.features[0].status = 'preview'
$previewCoverage.features = @($previewCoverage.features | Where-Object { [string]$_.id -ne $previewFeatureId })
$previewUpstream | ConvertTo-Json -Depth 20 | Set-Content $previewUpstreamPath -Encoding utf8NoBOM
$previewCoverage | ConvertTo-Json -Depth 20 | Set-Content $previewCoveragePath -Encoding utf8NoBOM
& $compareScript `
    -GalleryManifestPath $contractGalleryPath `
    -UpstreamManifestPath $previewUpstreamPath `
    -CoveragePath $previewCoveragePath `
    -Quiet

$invalidSource = Get-Content $contractUpstreamPath -Raw | ConvertFrom-Json -Depth 100
$invalidSource.features[0].repository = 'unoplatform/not-pinned'
$invalidSource | ConvertTo-Json -Depth 20 | Set-Content $invalidSourcePath -Encoding utf8NoBOM
$invalidSourceWasRejected = $false
try {
    & $compareScript `
        -GalleryManifestPath $contractGalleryPath `
        -UpstreamManifestPath $invalidSourcePath `
        -CoveragePath $contractCoveragePath `
        -Quiet
} catch {
    if ($_.Exception.Message -notmatch 'no pinned source') {
        throw
    }
    $invalidSourceWasRejected = $true
}
if (-not $invalidSourceWasRejected) {
    throw 'The coverage comparator accepted a feature from an unpinned repository.'
}

Remove-Item $invalidCoveragePath, $previewUpstreamPath, $previewCoveragePath, $invalidSourcePath -Force
Remove-Item $scratchDirectory -Force
Write-Host "Manifest contract validation passed. Artifact directory: $OutputDirectory"

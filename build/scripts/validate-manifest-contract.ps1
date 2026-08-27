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
$contractReportScript = Join-Path $PSScriptRoot 'report-sample-contract.ps1'
$compareScript = Join-Path $PSScriptRoot 'compare-feature-coverage.ps1'
$sampleSchemaV1 = Join-Path $repoRoot 'docs\catalog\sample-manifest-v1.schema.json'
$sampleSchemaV2 = Join-Path $repoRoot 'docs\catalog\sample-manifest-v2.schema.json'
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
$scratchDirectory = Join-Path $OutputDirectory '.validation-scratch'
New-Item -ItemType Directory -Path $scratchDirectory -Force | Out-Null
$invalidCoveragePath = Join-Path $scratchDirectory 'invalid-coverage.json'
$previewUpstreamPath = Join-Path $scratchDirectory 'preview-upstream.json'
$previewCoveragePath = Join-Path $scratchDirectory 'preview-coverage.json'
$invalidSourcePath = Join-Path $scratchDirectory 'invalid-source.json'
$invalidContractPath = Join-Path $scratchDirectory 'invalid-contract.json'
$escapedStablePath = Join-Path $scratchDirectory 'escaped-stable.json'
$newImplicitStablePath = Join-Path $scratchDirectory 'new-implicit-stable.json'
$promotedLegacyPath = Join-Path $scratchDirectory 'promoted-legacy.json'
$contractReportPath = Join-Path $OutputDirectory 'sample-contract-report.json'
$secondContractReportPath = Join-Path $scratchDirectory 'sample-contract-report-second.json'

& $exportScript -GeneratedSourcePath $GeneratedSourcePath -OutputPath $manifestPath
$manifestEnvelope = Get-Content $manifestPath -Raw | ConvertFrom-Json -Depth 5
$sampleSchema = switch ([int]$manifestEnvelope.schemaVersion) {
    1 { $sampleSchemaV1 }
    2 { $sampleSchemaV2 }
    default { throw "Unsupported sample manifest schema version '$($manifestEnvelope.schemaVersion)'." }
}

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

& $contractReportScript `
    -ManifestPath $manifestPath `
    -BaselinePath $baselinePath `
    -TargetName $TargetName `
    -OutputPath $contractReportPath
& $contractReportScript `
    -ManifestPath $manifestPath `
    -BaselinePath $baselinePath `
    -TargetName $TargetName `
    -OutputPath $secondContractReportPath `
    -Quiet
if ((Get-Content $contractReportPath -Raw) -cne (Get-Content $secondContractReportPath -Raw)) {
    throw 'The contract completeness report is not deterministic.'
}

$invalidContract = Get-Content $manifestPath -Raw | ConvertFrom-Json -Depth 100
$invalidContractSample = @($invalidContract.samples | Where-Object { $_.contractVersion -eq 1 })[0]
$invalidContractSample.accessibilityNotes = @()
$invalidContract | ConvertTo-Json -Depth 20 -Compress | Set-Content $invalidContractPath -Encoding utf8NoBOM
$invalidContractWasRejected = $false
try {
    & $contractReportScript `
        -ManifestPath $invalidContractPath `
        -BaselinePath $baselinePath `
        -TargetName $TargetName `
        -OutputPath (Join-Path $scratchDirectory 'invalid-contract-report.json') `
        -Quiet
} catch {
    if ($_.Exception.Message -notmatch 'accessibilityNotes') { throw }
    $invalidContractWasRejected = $true
}
if (-not $invalidContractWasRejected) {
    throw 'The contract completeness report accepted incomplete contract-v1 metadata.'
}

$escapedStable = Get-Content $manifestPath -Raw | ConvertFrom-Json -Depth 100
$escapedStableSample = @($escapedStable.samples | Where-Object { $_.contractVersion -eq 1 })[0]
$escapedStableSample.contractVersion = 0
$escapedStableSample.status.value = 0
$escapedStableSample.status.name = 'Stable'
$escapedStableSample.statusExplicit = $true
$escapedStable | ConvertTo-Json -Depth 20 -Compress | Set-Content $escapedStablePath -Encoding utf8NoBOM
$escapedStableWasRejected = $false
try {
    & $contractReportScript `
        -ManifestPath $escapedStablePath `
        -BaselinePath $baselinePath `
        -TargetName $TargetName `
        -OutputPath (Join-Path $scratchDirectory 'escaped-stable-report.json') `
        -Quiet
} catch {
    if ($_.Exception.Message -notmatch 'escaped contract-v1 enforcement') { throw }
    $escapedStableWasRejected = $true
}
if (-not $escapedStableWasRejected) {
    throw 'The contract completeness report accepted an explicitly Stable legacy entry.'
}

$newImplicitStable = Get-Content $manifestPath -Raw | ConvertFrom-Json -Depth 100
$newImplicitStableSample = @($newImplicitStable.samples | Where-Object { $_.contractVersion -eq 0 })[0]
$newImplicitStableSample.slug = 'new-implicit-stable'
$newImplicitStableSample.status.value = 0
$newImplicitStableSample.status.name = 'Stable'
$newImplicitStableSample.statusExplicit = $false
$newImplicitStable | ConvertTo-Json -Depth 20 -Compress | Set-Content $newImplicitStablePath -Encoding utf8NoBOM
$newImplicitStableWasRejected = $false
try {
    & $contractReportScript `
        -ManifestPath $newImplicitStablePath `
        -BaselinePath $baselinePath `
        -TargetName $TargetName `
        -OutputPath (Join-Path $scratchDirectory 'new-implicit-stable-report.json') `
        -Quiet
} catch {
    if ($_.Exception.Message -notmatch 'not in the frozen legacy allowlist') { throw }
    $newImplicitStableWasRejected = $true
}
if (-not $newImplicitStableWasRejected) {
    throw 'The contract completeness report accepted a new implicit-Stable legacy entry.'
}

$promotedLegacy = Get-Content $manifestPath -Raw | ConvertFrom-Json -Depth 100
$promotedLegacySample = @($promotedLegacy.samples | Where-Object { $_.slug -eq 'diagnostics' })[0]
if ($null -eq $promotedLegacySample) {
    throw "The contract self-test requires the Experimental 'diagnostics' sample."
}
$promotedLegacySample.status.value = 0
$promotedLegacySample.status.name = 'Stable'
$promotedLegacySample.statusExplicit = $false
$promotedLegacy | ConvertTo-Json -Depth 20 -Compress | Set-Content $promotedLegacyPath -Encoding utf8NoBOM
$promotedLegacyWasRejected = $false
try {
    & $contractReportScript `
        -ManifestPath $promotedLegacyPath `
        -BaselinePath $baselinePath `
        -TargetName $TargetName `
        -OutputPath (Join-Path $scratchDirectory 'promoted-legacy-report.json') `
        -Quiet
} catch {
    if ($_.Exception.Message -notmatch 'not in the frozen legacy allowlist') { throw }
    $promotedLegacyWasRejected = $true
}
if (-not $promotedLegacyWasRejected) {
    throw 'The contract completeness report accepted a legacy Experimental sample promoted implicitly to Stable.'
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

Remove-Item $scratchDirectory -Recurse -Force
Write-Host "Manifest contract validation passed. Artifact directory: $OutputDirectory"

#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $GalleryManifestPath,

    [Parameter(Mandatory)]
    [string] $UpstreamManifestPath,

    [Parameter(Mandatory)]
    [string] $CoveragePath,

    [string] $OutputPath = '',

    [switch] $Quiet
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$gallery = Get-Content $GalleryManifestPath -Raw | ConvertFrom-Json -Depth 100
$upstream = Get-Content $UpstreamManifestPath -Raw | ConvertFrom-Json -Depth 100
$coverage = Get-Content $CoveragePath -Raw | ConvertFrom-Json -Depth 100
if ($gallery.schemaVersion -notin @(1, 2) -or $upstream.schemaVersion -ne 1 -or $coverage.schemaVersion -ne 1) {
    throw 'Gallery manifest must use schema version 1 or 2; upstream and coverage manifests must use version 1.'
}

$gallerySlugs = [System.Collections.Generic.Dictionary[string, bool]]::new([StringComparer]::Ordinal)
foreach ($sample in $gallery.samples) {
    $gallerySlugs.Add([string]$sample.slug, $true)
}
$coverageById = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
$upstreamById = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
$sourceRepositories = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$errors = [System.Collections.Generic.List[string]]::new()
foreach ($source in $upstream.sources) {
    $repository = [string]$source.repository
    if (-not $sourceRepositories.Add($repository)) {
        $errors.Add("Duplicate upstream source repository '$repository'.")
    }
}
foreach ($feature in $upstream.features) {
    $id = [string]$feature.id
    if (-not $sourceRepositories.Contains([string]$feature.repository)) {
        $errors.Add("Upstream feature '$id' references repository '$($feature.repository)' with no pinned source.")
    }
    if ($upstreamById.ContainsKey($id)) {
        $errors.Add("Duplicate upstream feature '$id'.")
    } else {
        $upstreamById[$id] = $feature
    }
}
foreach ($entry in $coverage.features) {
    $id = [string]$entry.id
    if ($coverageById.ContainsKey($id)) {
        $errors.Add("Duplicate coverage entry '$id'.")
    } else {
        $coverageById[$id] = $entry
    }
}

foreach ($entry in $coverage.features) {
    $id = [string]$entry.id
    $disposition = [string]$entry.disposition
    if ($disposition -notin @('covered-core', 'optional-flavor', 'external-companion', 'docs-only', 'not-applicable', 'blocked')) {
        $errors.Add("Feature '$id' has unknown disposition '$disposition'.")
    }
    if ([string]::IsNullOrWhiteSpace([string]$entry.owner)) {
        $errors.Add("Feature '$id' has no owner.")
    }
    $slugs = @($entry.gallerySlugs)
    if ($disposition -eq 'covered-core') {
        if ($slugs.Count -eq 0) {
            $errors.Add("Covered feature '$id' has no Gallery slug.")
        }
        foreach ($slug in $slugs) {
            if (-not $gallerySlugs.ContainsKey([string]$slug)) {
                $errors.Add("Feature '$id' references missing Gallery slug '$slug'.")
            }
        }
    } elseif ($slugs.Count -gt 0) {
        $errors.Add("Non-core feature '$id' must not claim Gallery slugs.")
    }
    if ($disposition -in @('blocked', 'external-companion', 'docs-only', 'not-applicable', 'optional-flavor') -and
        [string]::IsNullOrWhiteSpace([string]$entry.reason)) {
        $errors.Add("Feature '$id' with disposition '$disposition' requires a reason.")
    }
    if ($disposition -eq 'blocked' -and
        [string]$entry.issue -notmatch '^https://github\.com/unoplatform/Uno\.Gallery/issues/\d+$') {
        $errors.Add("Blocked feature '$id' requires a Uno.Gallery issue URL.")
    }
}

$results = [System.Collections.Generic.List[object]]::new()
foreach ($feature in $upstream.features) {
    if ([string]$feature.status -ne 'stable') {
        continue
    }
    $id = [string]$feature.id
    if (-not $coverageById.ContainsKey($id)) {
        $errors.Add("Stable upstream feature '$id' has no Gallery classification.")
        continue
    }

    $entry = $coverageById[$id]
    $disposition = [string]$entry.disposition
    $slugs = @($entry.gallerySlugs)

    $results.Add([ordered]@{
        id = $id
        disposition = $disposition
        gallerySlugs = $slugs
        owner = [string]$entry.owner
        issue = [string]$entry.issue
    })
}

foreach ($coverageId in $coverageById.Keys) {
    if (-not $upstreamById.ContainsKey($coverageId)) {
        $errors.Add("Coverage entry '$coverageId' is not present in the upstream manifest.")
    }
}

$report = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    advisory = $true
    upstreamScope = [string]$upstream.scope
    upstreamComplete = [bool]$upstream.complete
    sources = @($upstream.sources)
    gallerySampleCount = @($gallery.samples).Count
    upstreamFeatureCount = @($upstream.features | Where-Object { [string]$_.status -eq 'stable' }).Count
    classifiedFeatureCount = $results.Count
    errorCount = $errors.Count
    results = $results
    errors = $errors
}

if ($OutputPath) {
    $outputDirectory = Split-Path -Parent $OutputPath
    if ($outputDirectory) {
        New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    }
    $report | ConvertTo-Json -Depth 10 | Set-Content $OutputPath -Encoding utf8NoBOM
}

if ($errors.Count -gt 0) {
    if (-not $Quiet) {
        $errors | ForEach-Object { Write-Host "ERROR: $_" -ForegroundColor Red }
    }
    throw "Feature coverage comparison failed: $([string]::Join(' | ', $errors))"
}

Write-Host "Feature coverage comparison passed: $($results.Count) classified feature(s), $(@($gallery.samples).Count) Gallery samples."

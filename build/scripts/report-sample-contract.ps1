#Requires -Version 7.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ManifestPath,

    [Parameter(Mandatory)]
    [string] $BaselinePath,

    [Parameter(Mandatory)]
    [string] $TargetName,

    [Parameter(Mandatory)]
    [string] $OutputPath,

    [switch] $Quiet
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json -Depth 100
$baseline = Get-Content $BaselinePath -Raw | ConvertFrom-Json -Depth 20
$targetProperty = $baseline.targets.PSObject.Properties[$TargetName]
if ($manifest.schemaVersion -ne 2 -or $baseline.schemaVersion -ne 1 -or $null -eq $targetProperty) {
    throw "Contract completeness requires a schema-v2 manifest, schema-v1 baseline, and '$TargetName' target."
}

$designNames = [ordered]@{ 1 = 'Material'; 2 = 'Fluent'; 4 = 'Cupertino'; 8 = 'Native'; 16 = 'Agnostic' }
$rendererNames = [ordered]@{ 1 = 'Native'; 2 = 'Skia'; 4 = 'DOM' }

function Get-ExpectedFlagsName([int] $Value, [System.Collections.IDictionary] $Names) {
    $parts = foreach ($entry in $Names.GetEnumerator()) {
        if (($Value -band [int]$entry.Key) -ne 0) {
            [string]$entry.Value
        }
    }
    if (@($parts).Count -eq 0) { return 'None' }
    return [string]::Join(', ', [string[]]@($parts))
}

function Test-NonEmpty([object] $Value) {
    return $null -ne $Value -and -not [string]::IsNullOrWhiteSpace([string]$Value)
}

$contractSlugs = [System.Collections.Generic.List[string]]::new()
$legacySlugs = [System.Collections.Generic.List[string]]::new()
$errors = [System.Collections.Generic.List[string]]::new()
$allowedLegacySlugs = [System.Collections.Generic.HashSet[string]]::new(
    [string[]]@($baseline.allowedLegacySlugs),
    [StringComparer]::Ordinal)
foreach ($slug in @($targetProperty.Value.additionalAllowedLegacySlugs)) {
    [void]$allowedLegacySlugs.Add([string]$slug)
}

foreach ($sample in @($manifest.samples)) {
    $slug = [string]$sample.slug
    $expectedStatusNames = @('Stable', 'Preview', 'Experimental', 'Deprecated', 'Incomplete')
    $statusValue = [int]$sample.status.value
    if ($statusValue -lt 0 -or $statusValue -ge $expectedStatusNames.Count -or
        [string]$sample.status.name -cne $expectedStatusNames[$statusValue]) {
        $errors.Add("${slug}: unknown or inconsistent status '$($sample.status.value)/$($sample.status.name)'.")
        continue
    }
    $isContractV1 = [int]$sample.contractVersion -eq 1
    if ([int]$sample.contractVersion -notin @(0, 1)) {
        $errors.Add("${slug}: unsupported contractVersion '$($sample.contractVersion)'; expected 0 or 1.")
        continue
    }

    if ([bool]$sample.statusExplicit -and
        ([int]$sample.status.value -eq 0 -or [string]$sample.status.name -eq 'Stable') -and
        -not $isContractV1) {
        $errors.Add("${slug}: explicitly authored Stable status escaped contract-v1 enforcement.")
    }

    if (-not $isContractV1) {
        $legacySlugs.Add($slug)
        $isStable = [int]$sample.status.value -eq 0 -and [string]$sample.status.name -eq 'Stable'
        if ($isStable -and -not $allowedLegacySlugs.Contains($slug)) {
            $errors.Add("${slug}: implicit Stable sample is not in the frozen legacy allowlist; author ContractVersion = 1.")
        }
        continue
    }

    $contractSlugs.Add($slug)
    $invalid = [System.Collections.Generic.List[string]]::new()
    foreach ($field in @('description', 'documentationLink', 'owner', 'resetBehavior')) {
        if (-not (Test-NonEmpty $sample.$field)) { $invalid.Add($field) }
    }
    foreach ($field in @('tags', 'requirements', 'accessibilityNotes', 'variants')) {
        $values = @($sample.$field)
        if ($values.Count -eq 0 -or @($values | Where-Object { -not (Test-NonEmpty $_) }).Count -gt 0) {
            $invalid.Add($field)
        }
    }

    $reviewed = [DateTime]::MinValue
    if (-not [DateTime]::TryParseExact(
        [string]$sample.reviewedOn,
        'yyyy-MM-dd',
        [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]::None,
        [ref]$reviewed)) {
        $invalid.Add('reviewedOn')
    }

    $designValue = [int]$sample.supportedDesigns.value
    $rendererValue = [int]$sample.supportedRenderers.value
    if ($designValue -lt 1 -or ($designValue -band (-bnot 31)) -ne 0 -or
        [string]$sample.supportedDesigns.name -cne (Get-ExpectedFlagsName $designValue $designNames)) {
        $invalid.Add('supportedDesigns')
    }
    if ($rendererValue -lt 1 -or ($rendererValue -band (-bnot 7)) -ne 0 -or
        [string]$sample.supportedRenderers.name -cne (Get-ExpectedFlagsName $rendererValue $rendererNames)) {
        $invalid.Add('supportedRenderers')
    }

    foreach ($field in @('documentationLink', 'issueLink', 'apiLink')) {
        $value = [string]$sample.$field
        if ($value -and -not [Uri]::IsWellFormedUriString($value, [UriKind]::Absolute)) {
            $invalid.Add($field)
        }
    }

    if ($invalid.Count -gt 0) {
        $errors.Add("${slug}: incomplete contract-v1 fields: $([string]::Join(', ', $invalid)).")
    }
}

$contractSlugs.Sort([StringComparer]::Ordinal)
$legacySlugs.Sort([StringComparer]::Ordinal)
$target = $targetProperty.Value
if ($contractSlugs.Count -lt [int]$target.minimumContractV1Count) {
    $errors.Add("${TargetName}: contract-v1 count $($contractSlugs.Count) is below baseline $($target.minimumContractV1Count).")
}
$contractSet = [System.Collections.Generic.HashSet[string]]::new($contractSlugs, [StringComparer]::Ordinal)
foreach ($requiredSlug in @($target.requiredContractV1Slugs)) {
    if (-not $contractSet.Contains([string]$requiredSlug)) {
        $errors.Add("${TargetName}: required contract-v1 slug '$requiredSlug' is missing.")
    }
}

if ($errors.Count -gt 0) {
    throw "Sample contract completeness failed:`n - $([string]::Join("`n - ", $errors))"
}

$report = [ordered]@{
    schemaVersion = 1
    target = $TargetName
    totalSampleCount = @($manifest.samples).Count
    contractV1Count = $contractSlugs.Count
    legacyBacklogCount = $legacySlugs.Count
    contractV1Slugs = [string[]]$contractSlugs
    legacyBacklogSlugs = [string[]]$legacySlugs
}
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
[System.IO.File]::WriteAllText(
    $OutputPath,
    ($report | ConvertTo-Json -Depth 10) + "`n",
    [System.Text.UTF8Encoding]::new($false))

if (-not $Quiet) {
    Write-Host "Contract report: $TargetName has $($contractSlugs.Count) contract-v1 samples and $($legacySlugs.Count) legacy backlog samples."
}

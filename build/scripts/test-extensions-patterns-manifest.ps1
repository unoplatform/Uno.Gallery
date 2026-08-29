param(
    [Parameter(Mandatory = $true)]
    [string]$DefaultGeneratedSourcePath,
    [Parameter(Mandatory = $true)]
    [string]$OptionalGeneratedSourcePath,
    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$export = Join-Path $PSScriptRoot 'export-sample-manifest.ps1'
$reportContract = Join-Path $PSScriptRoot 'report-sample-contract.ps1'
$manifestSchema = Join-Path $repoRoot 'docs\catalog\sample-manifest-v2.schema.json'
$manifestBaseline = Join-Path $repoRoot 'docs\catalog\sample-manifest-baseline-v1.json'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$defaultPath = Join-Path $OutputDirectory 'default-sample-manifest.json'
$optionalPath = Join-Path $OutputDirectory 'extensions-patterns-sample-manifest.json'

& $export -GeneratedSourcePath $DefaultGeneratedSourcePath -OutputPath $defaultPath
& $export -GeneratedSourcePath $OptionalGeneratedSourcePath -OutputPath $optionalPath
foreach ($manifestPath in @($defaultPath, $optionalPath)) {
    $schemaErrors = @()
    $isValid = (Get-Content $manifestPath -Raw) |
        Test-Json -SchemaFile $manifestSchema -ErrorAction SilentlyContinue -ErrorVariable +schemaErrors
    if (-not $isValid) {
        throw "Schema validation failed for '$manifestPath': $([string]::Join(' | ', $schemaErrors))"
    }
}
& $reportContract `
    -ManifestPath $defaultPath `
    -BaselinePath $manifestBaseline `
    -TargetName 'Desktop' `
    -OutputPath (Join-Path $OutputDirectory 'default-contract-report.json') `
    -Quiet
& $reportContract `
    -ManifestPath $optionalPath `
    -BaselinePath $manifestBaseline `
    -TargetName 'Desktop-ExtensionsPatterns' `
    -OutputPath (Join-Path $OutputDirectory 'extensions-patterns-contract-report.json') `
    -Quiet

$default = Get-Content $defaultPath -Raw | ConvertFrom-Json -Depth 100
$optional = Get-Content $optionalPath -Raw | ConvertFrom-Json -Depth 100
$patternSlugs = @(
    'extensions-mvux-feedview',
    'extensions-localization',
    'extensions-storage',
    'extensions-configuration',
    'extensions-validation'
)
$defaultSlugs = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$optionalSlugs = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$default.samples.slug | ForEach-Object { [void]$defaultSlugs.Add([string]$_) }
$optional.samples.slug | ForEach-Object { [void]$optionalSlugs.Add([string]$_) }

foreach ($sample in @($optional.samples)) {
    if ($null -eq $sample.sourcePath) {
        throw "Optional manifest sample '$($sample.slug)' has no repository source path."
    }
    $sourcePath = [string]$sample.sourcePath
    if ($sourcePath -notmatch '^Uno\.Gallery(?:\.ExtensionsPatterns)?/' -or
        -not (Test-Path (Join-Path $repoRoot $sourcePath) -PathType Leaf)) {
        throw "Optional manifest sample '$($sample.slug)' has invalid source path '$sourcePath'."
    }
}

foreach ($slug in $patternSlugs) {
    if ($defaultSlugs.Contains($slug)) {
        throw "Default manifest unexpectedly contains optional slug '$slug'."
    }
    if (-not $optionalSlugs.Contains($slug)) {
        throw "Optional manifest does not contain '$slug'."
    }
    $sample = @($optional.samples | Where-Object { [string]$_.slug -eq $slug })[0]
    if ([int]$sample.contractVersion -ne 1) {
        throw "Optional sample '$slug' must use contract version 1."
    }
    $sourcePath = [string]$sample.sourcePath
    if (-not $sourcePath.StartsWith('Uno.Gallery.ExtensionsPatterns/', [StringComparison]::Ordinal) -or
        -not (Test-Path (Join-Path $repoRoot $sourcePath) -PathType Leaf)) {
        throw "Optional sample '$slug' has invalid source path '$sourcePath'."
    }
}
if (@($optional.samples).Count -ne @($default.samples).Count + $patternSlugs.Count) {
    throw "Optional manifest count must equal default count plus $($patternSlugs.Count)."
}

Write-Host "Extensions patterns manifest gating passed: default=$(@($default.samples).Count), optional=$(@($optional.samples).Count)."

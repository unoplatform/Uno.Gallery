param(
    [Parameter(Mandatory = $true)]
    [string]$DefaultGeneratedSourcePath,
    [Parameter(Mandatory = $true)]
    [string]$OptionalGeneratedSourcePath,
    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$export = Join-Path $PSScriptRoot 'export-sample-manifest.ps1'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$defaultPath = Join-Path $OutputDirectory 'default-sample-manifest.json'
$optionalPath = Join-Path $OutputDirectory 'extensions-patterns-sample-manifest.json'

& $export -GeneratedSourcePath $DefaultGeneratedSourcePath -OutputPath $defaultPath
& $export -GeneratedSourcePath $OptionalGeneratedSourcePath -OutputPath $optionalPath

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

foreach ($slug in $patternSlugs) {
    if ($defaultSlugs.Contains($slug)) {
        throw "Default manifest unexpectedly contains optional slug '$slug'."
    }
    if (-not $optionalSlugs.Contains($slug)) {
        throw "Optional manifest does not contain '$slug'."
    }
}
if (@($optional.samples).Count -ne @($default.samples).Count + $patternSlugs.Count) {
    throw "Optional manifest count must equal default count plus $($patternSlugs.Count)."
}

Write-Host "Extensions patterns manifest gating passed: default=$(@($default.samples).Count), optional=$(@($optional.samples).Count)."

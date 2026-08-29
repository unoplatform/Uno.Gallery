#Requires -Version 7.0
[CmdletBinding()]
param(
    [string] $SourcePath,
    [string] $OutputPath,
    [switch] $Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not $SourcePath) {
    $SourcePath = Join-Path $repoRoot 'Uno.Gallery\Strings\en\Resources.resw'
}
if (-not $OutputPath) {
    $OutputPath = Join-Path $repoRoot 'Uno.Gallery\Strings\qps-ploc\Resources.resw'
}

$accented = @{
    'a' = 'á'; 'b' = 'ƀ'; 'c' = 'ç'; 'd' = 'ð'; 'e' = 'ë'; 'f' = 'ƒ'
    'g' = 'ğ'; 'h' = 'ħ'; 'i' = 'ï'; 'j' = 'ĵ'; 'k' = 'ķ'; 'l' = 'ľ'
    'm' = 'ɱ'; 'n' = 'ñ'; 'o' = 'ö'; 'p' = 'þ'; 'q' = 'ʠ'; 'r' = 'ř'
    's' = 'š'; 't' = 'ŧ'; 'u' = 'ü'; 'v' = 'ṽ'; 'w' = 'ŵ'; 'x' = 'ẋ'
    'y' = 'ÿ'; 'z' = 'ž'
}

function ConvertTo-PseudoText([string] $value) {
    $builder = [System.Text.StringBuilder]::new()
    foreach ($character in $value.ToCharArray()) {
        $lower = [char]::ToLowerInvariant($character).ToString()
        if ($accented.ContainsKey($lower)) {
            $replacement = [string]$accented[$lower]
            if ([char]::IsUpper($character)) {
                $replacement = $replacement.ToUpperInvariant()
            }
            [void]$builder.Append($replacement)
        } else {
            [void]$builder.Append($character)
        }
    }

    $paddingLength = [Math]::Max(3, [Math]::Ceiling($value.Length * 0.3))
    return "[!! $builder $('~' * $paddingLength) !!]"
}

[xml]$source = Get-Content $SourcePath -Raw
$expected = [ordered]@{}
foreach ($data in $source.root.data) {
    $name = [string]$data.name
    if ($expected.Contains($name)) {
        throw "Duplicate English resource key: $name"
    }
    $expected[$name] = ConvertTo-PseudoText ([string]$data.value)
}

if ($Check) {
    if (-not (Test-Path $OutputPath -PathType Leaf)) {
        throw "Pseudo resource file is missing: $OutputPath"
    }
    [xml]$actualDocument = Get-Content $OutputPath -Raw
    $actual = @{}
    foreach ($data in $actualDocument.root.data) {
        $actual[[string]$data.name] = [string]$data.value
    }
    if ($actual.Count -ne $expected.Count) {
        throw "Pseudo resource count is $($actual.Count); expected $($expected.Count)."
    }
    foreach ($entry in $expected.GetEnumerator()) {
        if (-not $actual.ContainsKey($entry.Key) -or $actual[$entry.Key] -ne $entry.Value) {
            throw "Pseudo resource is stale or missing: $($entry.Key)"
        }
    }
    Write-Host "Pseudo resources are current ($($expected.Count) entries)."
    return
}

foreach ($data in $source.root.data) {
    $data.value = $expected[[string]$data.name]
}

$outputDirectory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$settings = [System.Xml.XmlWriterSettings]::new()
$settings.Encoding = [System.Text.UTF8Encoding]::new($false)
$settings.Indent = $true
$settings.NewLineChars = "`r`n"
$writer = [System.Xml.XmlWriter]::Create($OutputPath, $settings)
try {
    $source.Save($writer)
} finally {
    $writer.Dispose()
}

Write-Host "Generated $($expected.Count) pseudo resources at $OutputPath"

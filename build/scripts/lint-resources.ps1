#Requires -Version 7.0
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$englishPath = Join-Path $repoRoot 'Uno.Gallery\Strings\en\Resources.resw'
$pseudoPath = Join-Path $repoRoot 'Uno.Gallery\Strings\qps-ploc\Resources.resw'
$errors = [System.Collections.Generic.List[string]]::new()

function Read-Resources([string] $path) {
    [xml]$document = Get-Content $path -Raw
    $resources = @{}
    foreach ($data in $document.root.data) {
        $key = [string]$data.name
        if ($resources.ContainsKey($key)) {
            $errors.Add("$path contains duplicate resource key '$key'.")
            continue
        }
        $value = [string]$data.value
        if ([string]::IsNullOrWhiteSpace($value)) {
            $errors.Add("$path contains an empty resource value for '$key'.")
        }
        $resources[$key] = $value
    }
    return $resources
}

$english = Read-Resources $englishPath
$pseudo = Read-Resources $pseudoPath

foreach ($key in $english.Keys) {
    if (-not $pseudo.ContainsKey($key)) {
        $errors.Add("Pseudo resources are missing '$key'.")
    }
}
foreach ($key in $pseudo.Keys) {
    if (-not $english.ContainsKey($key)) {
        $errors.Add("Pseudo resources contain unknown key '$key'.")
    }
}

$xamlNamespace = 'http://schemas.microsoft.com/winfx/2006/xaml'
$attachedResourceNames = @{
    'AutomationProperties.Name' = '[using:Microsoft.UI.Xaml.Automation]AutomationProperties.Name'
    'AutomationProperties.HelpText' = '[using:Microsoft.UI.Xaml.Automation]AutomationProperties.HelpText'
    'ToolTipService.ToolTip' = '[using:Microsoft.UI.Xaml.Controls]ToolTipService.ToolTip'
}
$xamlChecks = @(
    @{
        Path = Join-Path $repoRoot 'Uno.Gallery\Views\Shell.xaml'
        Properties = @('Content', 'Header', 'PlaceholderText', 'Text', 'ToolTipService.ToolTip')
    },
    @{
        Path = Join-Path $repoRoot 'Uno.Gallery\Views\Styles\SamplePageLayout.xaml'
        Properties = @('Content', 'Header', 'PlaceholderText', 'Text')
    },
    @{
        Path = Join-Path $repoRoot 'Uno.Gallery\Views\SamplePages\AccessibilitySamplePage.xaml'
        Properties = @(
            'AutomationProperties.Name',
            'AutomationProperties.HelpText',
            'Content',
            'Header',
            'OffContent',
            'OnContent',
            'PlaceholderText',
            'Text')
    },
    @{
        Path = Join-Path $repoRoot 'Uno.Gallery\Views\SamplePages\LocalizationSamplePage.xaml'
        Properties = @('Content', 'Header', 'PlaceholderText', 'Text')
    }
)

foreach ($check in $xamlChecks) {
    $xamlPath = $check.Path
    $localizableProperties = $check.Properties
    $document = [System.Xml.Linq.XDocument]::Load(
        $xamlPath,
        [System.Xml.Linq.LoadOptions]::SetLineInfo)
    foreach ($element in $document.Descendants()) {
        $uidAttribute = $element.Attributes() |
            Where-Object { $_.Name.NamespaceName -eq $xamlNamespace -and $_.Name.LocalName -eq 'Uid' } |
            Select-Object -First 1

        foreach ($attribute in $element.Attributes()) {
            if ($attribute.Name.NamespaceName -ne '' -or $attribute.Name.LocalName -notin $localizableProperties) {
                continue
            }
            $value = $attribute.Value
            if ($value.StartsWith('{') -or $value -notmatch '[A-Za-z]') {
                continue
            }
            $line = [System.Xml.IXmlLineInfo]$element
            if ($null -eq $uidAttribute) {
                $errors.Add("$xamlPath`:$($line.LineNumber) has literal $($attribute.Name.LocalName)='$value' without x:Uid.")
                continue
            }
            $propertyResourceName = $attachedResourceNames[$attribute.Name.LocalName]
            if ($null -eq $propertyResourceName) {
                $propertyResourceName = $attribute.Name.LocalName
            }
            $resourceKey = "$($uidAttribute.Value).$propertyResourceName"
            if (-not $english.ContainsKey($resourceKey)) {
                $errors.Add("$xamlPath`:$($line.LineNumber) references missing English resource '$resourceKey'.")
            } elseif ($english[$resourceKey] -ne $value) {
                $errors.Add("$xamlPath`:$($line.LineNumber) literal '$value' does not match English resource '$resourceKey'.")
            }
        }
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Host "ERROR: $_" -ForegroundColor Red }
    throw "Resource lint failed with $($errors.Count) error(s)."
}

Write-Host "Resource lint passed: $($english.Count) English and pseudo-localized entries."

#Requires -Version 7.0
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$pagePath = Join-Path $repoRoot 'Uno.Gallery\Views\SamplePages\AccessibilitySamplePage.xaml'
$codeBehindPath = "$pagePath.cs"
$helperPath = Join-Path $repoRoot 'Uno.Gallery\Helpers\AccessibilityHelper.cs'
$document = [System.Xml.Linq.XDocument]::Load($pagePath)
$requirements = @{
    'Accessibility_Email' = @('AutomationProperties.Name', 'AutomationProperties.HelpText')
    'Accessibility_Notifications' = @('AutomationProperties.Name', 'AutomationProperties.HelpText')
    'Accessibility_Save' = @('AutomationProperties.Name', 'AutomationProperties.HelpText')
    'Accessibility_Announcement' = @('AutomationProperties.LiveSetting')
    'Accessibility_ContrastPass' = @('AutomationProperties.Name')
    'Accessibility_ContrastFail' = @('AutomationProperties.Name')
    'Accessibility_MotionTarget' = @('AutomationProperties.Name')
}

$elementsById = @{}
foreach ($element in $document.Descendants()) {
    $id = $element.Attributes() |
        Where-Object { $_.Name.LocalName -eq 'AutomationProperties.AutomationId' } |
        Select-Object -First 1
    if ($null -ne $id) {
        $elementsById[$id.Value] = $element
    }
}

$errors = [System.Collections.Generic.List[string]]::new()
foreach ($entry in $requirements.GetEnumerator()) {
    if (-not $elementsById.ContainsKey($entry.Key)) {
        $errors.Add("Missing accessibility test target '$($entry.Key)'.")
        continue
    }
    $attributeNames = @($elementsById[$entry.Key].Attributes() | ForEach-Object { $_.Name.LocalName })
    foreach ($requiredAttribute in $entry.Value) {
        if ($requiredAttribute -notin $attributeNames) {
            $errors.Add("'$($entry.Key)' is missing $requiredAttribute.")
        }
    }
}

$announcement = $elementsById['Accessibility_Announcement']
$liveSetting = if ($null -ne $announcement) {
    $announcement.Attributes() |
        Where-Object { $_.Name.LocalName -eq 'AutomationProperties.LiveSetting' } |
        Select-Object -First 1
}
if ($null -eq $liveSetting -or $liveSetting.Value -ne 'Polite') {
    $errors.Add("Accessibility_Announcement must use the Polite live setting.")
}
if ((Get-Content $codeBehindPath -Raw) -notmatch 'AccessibilityHelper\.Announce') {
    $errors.Add("The live-region sample must use AccessibilityHelper.Announce.")
}
if ((Get-Content $helperPath -Raw) -notmatch 'RaiseAutomationEvent\(AutomationEvents\.LiveRegionChanged\)') {
    $errors.Add("AccessibilityHelper must raise AutomationEvents.LiveRegionChanged.")
}

$sampleRoot = Join-Path $repoRoot 'Uno.Gallery\Views\SamplePages'
foreach ($xamlFile in Get-ChildItem $sampleRoot -File -Filter '*.xaml') {
    if ((Get-Content $xamlFile.FullName -Raw) -notmatch 'AutomationProperties\.LiveSetting="Polite"') {
        continue
    }
    $codeBehind = "$($xamlFile.FullName).cs"
    if (-not (Test-Path $codeBehind -PathType Leaf) -or
        (Get-Content $codeBehind -Raw) -notmatch 'AccessibilityHelper\.Announce') {
        $errors.Add("$($xamlFile.Name) declares a polite live region without AccessibilityHelper.Announce.")
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Host "ERROR: $_" -ForegroundColor Red }
    throw "Accessibility metadata lint failed with $($errors.Count) error(s)."
}

Write-Host "Accessibility metadata lint passed ($($requirements.Count) targets)."

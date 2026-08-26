# Localization and RTL testing

English resources live in `Uno.Gallery/Strings/en/Resources.resw`.
`build/scripts/generate-pseudo-resources.ps1` creates the expanded, accented
`qps-ploc` catalog. Run the resource lint after changing shell or sample-detail
chrome:

```powershell
pwsh build/scripts/generate-pseudo-resources.ps1
pwsh build/scripts/generate-pseudo-resources.ps1 -Check
pwsh build/scripts/lint-resources.ps1
```

Build the real pseudo-locale path with:

```powershell
dotnet build Uno.Gallery/Uno.Gallery.csproj `
  -p:TargetFrameworkOverride=net10.0-desktop `
  -p:EnablePseudoLocalization=true
```

Add `-p:EnableRtlTestMode=true` to set right-to-left flow on the window content.
Popup-hosted flyouts, suggestions, and tooltips live outside that subtree and
still require renderer-specific manual verification. The dedicated Localization
and RTL sample also toggles its preview at runtime so normal interaction tests
cover mirroring and state preservation.

The release check must verify:

- no missing or duplicate English/pseudo keys;
- visible expansion without clipping;
- long-string wrapping;
- mixed English, Arabic, Hebrew, and numeric content;
- shell and sample layout mirroring;
- control state preserved across direction changes.

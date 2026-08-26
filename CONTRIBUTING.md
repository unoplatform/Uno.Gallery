# Contributing to Uno Gallery

Thank you for contributing! This guide covers the practical steps to set up your
environment, build the project, and submit well-formed changes.

---

## Contents

- [Prerequisites](#prerequisites)
- [Fast Desktop Build](#fast-desktop-build)
- [WebAssembly Builds](#webassembly-builds)
- [Windows WASM UI Tests](#windows-wasm-ui-tests)
- [Adding or Updating a Sample Page](#adding-or-updating-a-sample-page)
- [Tests and Checklist](#tests-and-checklist)
- [Commit and PR Expectations](#commit-and-pr-expectations)
- [Issue Claiming](#issue-claiming)
- [Generated Files and Build Outputs](#generated-files-and-build-outputs)
- [Cupertino Design System](#cupertino-design-system)
- [Localization and Accessibility](#localization-and-accessibility)
- [Maintenance and Release Policy](#maintenance-and-release-policy)

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) —
  see `global.json` for the Uno.Sdk version pin (`6.6.42` at the time of writing)
- Uno workloads: follow the
  [Uno Platform Get Started guide](https://platform.uno/docs/articles/get-started-dotnet-new.html)
- PowerShell 7+ (`pwsh`) for the lint and Windows UITest scripts
- For WASM development: no extra workload is needed beyond `uno` when building
  with `TargetFrameworkOverride` (see below)

---

## Fast Desktop Build

The project comment in `Uno.Gallery/Uno.Gallery.csproj` explains that `-f` forces
a restore across _all_ target frameworks (requiring every workload to be installed).
Use `TargetFrameworkOverride` instead to target only the platform you need:

```bash
# macOS / Linux
dotnet build Uno.Gallery/Uno.Gallery.csproj \
    -p:TargetFrameworkOverride=net10.0-desktop
```

```powershell
# Windows
dotnet build Uno.Gallery\Uno.Gallery.csproj `
    -p:TargetFrameworkOverride=net10.0-desktop
```

This produces a Debug build of the Skia desktop head.
The binary appears in `Uno.Gallery/bin/Debug/net10.0-desktop/`.

---

## WebAssembly Builds

> **CI vs. local:** CI pipelines pass `-f net10.0-browserwasm` because hosted
> agents install the complete workload matrix. Local developers should use
> `-p:TargetFrameworkOverride=net10.0-browserwasm` instead to avoid restore
> errors from unrelated workloads (iOS, Android, etc.) that are not installed
> locally.

### DOM renderer (native WebAssembly)

```bash
dotnet publish Uno.Gallery/Uno.Gallery.csproj \
    -p:TargetFrameworkOverride=net10.0-browserwasm \
    -c Release \
    -p:UseNativeRendering=true \
    -o out/wasm-dom
```

### Skia renderer (WebAssembly)

```bash
dotnet publish Uno.Gallery/Uno.Gallery.csproj \
    -p:TargetFrameworkOverride=net10.0-browserwasm \
    -c Release \
    -p:UseNativeRendering=false \
    -o out/wasm-skia
```

The published output lives under `<out-dir>/wwwroot/`.  
CI pipeline: `build/stage-build-wasm.yml`.

---

## Windows WASM UI Tests

UITests require a DOM WASM build with UIAutomation mapping enabled.
Debug builds start significantly faster in the browser than AOT Release builds
and use a 60-second element-wait timeout that is comfortable on typical developer
hardware.

**Step 1 — Publish a UITest-ready WASM artifact:**

```powershell
dotnet publish Uno.Gallery\Uno.Gallery.csproj `
    -p:TargetFrameworkOverride=net10.0-browserwasm `
    -c Debug `
    -p:UseNativeRendering=true `
    -o out\wasm-uitest
```

**Step 2 — Run the committed Windows test runner:**

```powershell
pwsh build/scripts/wasm-uitest-run-windows.ps1 `
    -WasmOutputPath out\wasm-uitest\wwwroot
```

The script:
- Downloads and caches a matched Chrome for Testing + ChromeDriver pair into
  `%LOCALAPPDATA%\uno-uitest-chrome\` (outside the repo, never git-tracked)
- Sets all `UNO_UITEST_*` environment variables consumed by `Uno.UITest.Selenium`
- Starts `dotnet-serve`, polls until the server is ready, runs the NUnit suite,
  then stops only that server process (by PID)

Pass `-WasmOutputPath` to point at your published `wwwroot/`.
See the script header for the full parameter and environment-variable reference.

Linux / CI pipeline: `build/stage-uitests-wasm.yml`  
(runs `build/scripts/wasm-uitest-build.sh` then `build/scripts/wasm-uitest-run.sh`)

---

## Adding or Updating a Sample Page

### File location

```
Uno.Gallery/Views/SamplePages/
    MyControlSamplePage.xaml
    MyControlSamplePage.xaml.cs
```

### `[SamplePage]` attribute

```csharp
[SamplePage(SampleCategory.UIComponents, "My Control",
    Description = "A one-line description of the control.",
    DocumentationLink = "https://learn.microsoft.com/en-us/windows/apps/design/controls/my-control")]
public sealed partial class MyControlSamplePage : Page { }
```

| Property | Required | Notes |
|---|---|---|
| `SampleCategory` | ✔ | `UIComponents`, `UIFeatures`, `NonUIFeatures`, `Toolkit`, `CommunityToolkit`, … |
| `Title` | ✔ | Display name shown in the nav tree and the search box |
| `Description` | — | One-line summary displayed in the sample header |
| `DocumentationLink` | — | URL shown in the sample footer (Microsoft Docs or Uno Docs) |
| `DataType` | — | ViewModel type; the sample receives an instance via `DataContext` |
| `Source` | — | Defaults to `SourceSdk.WinUI`; use `UnoMaterial`, `UnoToolkit`, `WCT`, etc. |
| `SortOrder` | — | Display order within the same category (lower = earlier; default = last) |
| `Slug` | — | URL-friendly identifier. Derived from `Title` if not set (spaces/punctuation → hyphens, uppercase → lowercase; e.g. `"My Control"` → `"my-control"`). Must be lowercase ASCII alphanumeric with interior hyphens only (`^[a-z0-9]+(-[a-z0-9]+)*$`). **Set this when renaming a sample to keep its URL stable** (see below). |
| `Tags` | — | String array of categorization tags for filtering/search, e.g. `new[] { "layout", "input" }` |
| `Status` | — | Production-readiness indicator. Defaults to `SampleStatus.Stable`. Other values: `Preview`, `Experimental`, `Deprecated`, `Incomplete`. |
| `Owner` | — | GitHub user or team slug of the maintainer, e.g. `"username"` or `"org/team-name"` |
| `ReviewedOn` | — | Date of the last quality review in ISO 8601 format (`YYYY-MM-DD`) |
| `RelatedSamples` | — | Slugs of related samples for cross-linking, e.g. `new[] { "button", "hyperlink-button" }`. Each entry is validated at build time (UGG0007) — use the final slug, not the display title. |

### Renaming a sample — keeping the old URL

When you rename a sample's `Title`, the source generator derives a new slug from the new title,
breaking any existing bookmarks or deep links.  Set `Slug` to the **old** slug to preserve the URL:

```csharp
// Before rename:
[SamplePage(SampleCategory.Controls, "AutoCompleteBox")]
// Derived slug: "autocomplete-box"

// After rename — keep old slug so the URL doesn't change:
[SamplePage(SampleCategory.Controls, "AutoSuggestBox", Slug = "autocomplete-box")]
```

The build emits **UGG0005** (error) if the explicit slug is not lowercase ASCII alphanumeric with
interior hyphens only, and **UGG0006** (warning) if the final slug collides with another sample.

### Design system templates

`SamplePageLayout` exposes one `DataTemplate` property per design system.
Set only the ones your sample supports:

```xml
<local:SamplePageLayout>
    <local:SamplePageLayout.MaterialTemplate>
        <DataTemplate><!-- Material XAML --></DataTemplate>
    </local:SamplePageLayout.MaterialTemplate>

    <local:SamplePageLayout.FluentTemplate>
        <DataTemplate><!-- Fluent XAML --></DataTemplate>
    </local:SamplePageLayout.FluentTemplate>

    <!-- CupertinoTemplate: see Cupertino section below -->
</local:SamplePageLayout>
```

For controls that look identical across all themes, use `DesignAgnosticTemplate`
and set `IsDesignAgnostic="True"` on `SamplePageLayout`.  
For mobile-only native rendering, use `NativeTemplate`.

### Platform conditionals

Apply `[SampleConditional]` when the sample should only be visible on specific
platforms. The source generator reads this attribute at compile time and excludes
the page from the catalog on platforms not listed.

```csharp
// Only show on Android and iOS
[SampleConditional(SampleConditionals.Mobile, Reason = "API only available on mobile")]

// Only show on Windows and WASM
[SampleConditional(SampleConditionals.Windows | SampleConditionals.Wasm)]

// All platforms except Skia Desktop
[SampleConditional(SampleConditionals.Always ^ SampleConditionals.SkiaDesktop,
    Reason = "Not supported on Skia Desktop")]

// Completely hidden (work-in-progress)
[SampleConditional(SampleConditionals.Disabled, Reason = "API not yet implemented")]
```

Available flags (`SampleConditionals` enum):  
`Windows`, `Wasm`, `SkiaDesktop`, `Droid`, `iOS`, `macOS` — and the composites
`Desktop` (Windows + Wasm + SkiaDesktop + macOS), `Mobile` (Droid + iOS),
`SkiaBased` (Wasm + SkiaDesktop), `Always`, `Disabled`.

### Automation IDs

UITests use `App.Marked("id")` to locate elements.
In Debug builds and when `IsUiAutomationMappingEnabled=True`, `x:Name` values are
automatically exposed as `AutomationProperties.AutomationId`.
You may also set the property explicitly:

```xml
<Button x:Name="PART_SubmitButton" />
<!-- or -->
<Button AutomationProperties.AutomationId="SubmitButton" />
```

Key IDs used by `TestBase`: `AppShell`, `NavToggle`, `RootSplitView`,
`PART_MaterialRadioButton`, `PART_FluentRadioButton`,
`PART_CupertinoRadioButton`, `PART_NativeRadioButton`.

### Documentation links

Set `DocumentationLink` on `[SamplePage]` to the relevant Microsoft Docs or
Uno Platform Docs URL. The link renders in the sample footer and is used by
`TestBase.NavigateToSample` for backdoor navigation in UITests.

### Sample states — quick reference

| Goal | How |
|---|---|
| Hide everywhere (WIP) | `[SampleConditional(SampleConditionals.Disabled, Reason = "...")]` |
| Restrict to platforms | `[SampleConditional(SampleConditionals.Mobile)]` etc. |
| Canary-only (hidden in stable Release) | `SampleCategory.Canary` in `[SamplePage]` |
| Wire a ViewModel | `DataType = typeof(MyViewModel)` in `[SamplePage]` |

---

## Tests and Checklist

### Lint ignored tests

Every `[Ignore]` attribute or `Assert.Ignore()` call in `Uno.Gallery.UITests/`
**must** include:

1. A GitHub issue URL: `https://github.com/unoplatform/Uno.Gallery/issues/<n>`
2. A non-past review date: `review-date: YYYY-MM-DD`

Run the lint script before committing:

```powershell
pwsh build/scripts/lint-test-ignores.ps1
```

Exit code 0 = all compliant.
The CI enforces this in the `Validation / Lint ignored tests` stage on every PR.

### Intentional skipped-test policy

If you add an `[Ignore]`, open (or link to an existing) tracking issue, set a
realistic `review-date`, and explain the reason in the ignore message.
Do not omit the URL or use a past date — the lint script treats both as violations.

### Pre-merge checklist

- [ ] Desktop build succeeds
- [ ] Any new `[Ignore]` entries carry a valid issue URL and a non-past `review-date`
- [ ] Lint script exits 0
- [ ] New sample pages have a filled `[SamplePage]` attribute, a `DocumentationLink`
      (if docs exist), and accurate platform conditionals
- [ ] Cupertino samples use `SampleCategory.Canary`
      (see [Cupertino Design System](#cupertino-design-system))

---

## Commit and PR Expectations

- Use a clear subject line; conventional-commit prefixes (`fix:`, `feat:`, `test:`,
  `build:`, `ci:`, `docs:`) are welcome
- Link related issues (`Fixes #1234`) in the PR description
- Keep each PR focused on a single concern; avoid unrelated refactors
- Fill in the PR template honestly — in particular the platform checklist and
  evidence of testing

---

## Issue Claiming

Before starting work on an existing issue, leave a comment on it to avoid
duplicate effort. Maintainers may re-open a claim if the issue has been idle.

---

## Generated Files and Build Outputs

Do **not** commit:

| Path pattern | Reason |
|---|---|
| `bin/`, `obj/` | Compiler output |
| `*.binlog` | MSBuild binary log |
| `out/` (if created locally) | Publish output |
| `AppPackages/`, `*.msix`, `*.ipa`, `*.apk` | Platform packages |
| `wwwroot/` (publish output) | WASM deploy artifact |

These paths are listed in `.gitignore` and regenerated by the build.

### Route constants and manifest

`SamplesGenerator` emits two additional files per compilation target alongside
`App.Samples.g.cs`:

- **`SampleRoutes.g.cs`** — `internal static class SampleRoutes` with one
  `public const string` per sample slug (PascalCase identifier → slug value).
  Reference these instead of hard-coding slug strings in feature code or tests.

- **`SampleManifest.g.cs`** — `internal static class SampleManifest` exposing
  `public static string GetJson()`, which returns a deterministic, FQN-sorted
  JSON catalog (schema version 1) for the current compilation target.
  The source generator performs no file I/O. WebAssembly CI exports the exact
  generated content with `build/scripts/export-sample-manifest.ps1`, validates
  target baselines, and publishes target-specific catalog artifacts. See
  [docs/decisions/0002-generated-sample-catalog.md](docs/decisions/0002-generated-sample-catalog.md)
  for the internal contract and
  [docs/catalog/README.md](docs/catalog/README.md) for export and upstream drift
  policy.

Both files are regenerated on every build and must not be committed.

---

## Cupertino Design System

**Cupertino controls and pages are hidden from stable Release builds.**
They remain compiled and covered by UITests so that silent regressions are
caught in CI, but they are not visible to users in a stable Release because:

- The Cupertino tab in `SamplePageLayout` receives a `null` template in Release
- `CupertinoPalettePage` and `SegmentedControlSamplePage` use `SampleCategory.Canary`
  which is filtered out of `Shell.Samples` before assignment

Cupertino UI is fully visible in:

| Build type | Visible? |
|---|---|
| `Debug` configuration | ✔ |
| Canary branches (`IS_CANARY_BUILD`) | ✔ |
| UITest builds (`USE_UITESTS` / `IsUiAutomationMappingEnabled=True`) | ✔ |
| AOT profile generation (`AOT_PROFILE_GEN`) | ✔ |
| Stable Release | ✘ |

When adding a Cupertino-specific sample, use `SampleCategory.Canary` in
`[SamplePage]` to prevent it from appearing in stable Release builds.

Full rationale: [docs/decisions/0001-cupertino-containment.md](docs/decisions/0001-cupertino-containment.md)

---

## Localization and Accessibility

Shell and sample-detail chrome use `x:Uid` resources from
`Uno.Gallery/Strings/en/Resources.resw`. After changing those strings, regenerate
and validate the pseudo-locale:

```powershell
pwsh build/scripts/generate-pseudo-resources.ps1
pwsh build/scripts/lint-localization-accessibility.ps1
```

Do not hand-edit `Strings/qps-ploc/Resources.resw`; it is generated from English
with expansion and diacritics. Build with `EnablePseudoLocalization=true` and
`EnableRtlTestMode=true` to validate the actual resource and right-to-left paths.
See [localization testing](docs/localization/testing.md).

Interactive controls must retain invariant automation IDs. Localize visible
labels, `AutomationProperties.Name`, and `AutomationProperties.HelpText`.
Live-region text changes must raise `AutomationEvents.LiveRegionChanged`.
Renderer-specific automated and manual expectations are documented in the
[accessibility matrix](docs/accessibility/renderer-matrix.md).

---

## Maintenance and Release Policy

Maintainers classify every issue using the process in
[docs/maintainers/backlog-triage.md](docs/maintainers/backlog-triage.md).
The issue-classification and pull-request first-response targets are monitored
by the scheduled `Triage SLA` workflow; automation reports breaches but does not
infer priority, assign owners, or close work. Critical-issue investigation is a
manual maintainer gate.

Dependency updates follow
[docs/maintainers/dependency-policy.md](docs/maintainers/dependency-policy.md).
Stable release candidates must complete
[docs/releases/stable-quality-checklist.md](docs/releases/stable-quality-checklist.md)
and publish the current
[compatibility matrix](docs/releases/compatibility-matrix.md).

Store credentials, certificates, provisioning profiles, and service-connection
secrets must remain in protected CI environments. See
[docs/releases/native-store-delivery-plan.md](docs/releases/native-store-delivery-plan.md)
for channel, promotion, and rollback responsibilities.

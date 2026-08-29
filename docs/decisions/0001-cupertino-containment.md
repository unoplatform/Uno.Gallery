# ADR 0001 — Cupertino Containment in Gallery

**Date:** 2026-08-24  
**Status:** Proposed  
**Branch:** `modernization/trust-baseline`

---

## Context

Uno.Cupertino is shipped as a stable NuGet package but its Gallery integration surfaced
a pattern of rough edges that confused users and created CI noise:

- **#1210** — Cupertino Palette tab appears in stable Release where Uno.Cupertino support
  is incomplete, misleading users about production readiness.
- **#874** — SegmentedControl is Cupertino-only but was filed under the general Toolkit
  category, implying broader platform coverage.
- **#1079** — Cupertino design tab shown on platforms where the theme is not applied,
  causing blank/broken previews.
- **#1224** — Overview page copy explicitly promises Cupertino parity, invalidating user
  trust when controls render incorrectly.

At the same time, the Cupertino package, templates, resources, and asset files must
remain compiled and tested to avoid silent regressions.

---

## Decision

Apply _build-type containment_: hide Cupertino-specific Gallery surfaces from stable
Release consumers while keeping them fully visible to Debug, canary, and UI-test builds.

### Mechanism

| Surface | Stable Release | DEBUG / IS_CANARY_BUILD / USE_UITESTS / AOT_PROFILE_GEN |
|---------|---------------|---------------------------------------|
| `SamplePageLayout` Cupertino tab | `default` (tab collapses via existing availability logic) | `CupertinoTemplate` |
| `SamplePageLayout.OnApplyTemplate` | Sets `CupertinoTemplate = null` before `base.OnApplyTemplate()` so the control template's Cupertino ContentPresenter never instantiates the sample's DataTemplate | DP unchanged |
| `CupertinoPalettePage` category | `SampleCategory.Canary` (hidden from nav/search) | `SampleCategory.Theming` |
| `SegmentedControlSamplePage` category | `SampleCategory.Canary` | `SampleCategory.Toolkit` |
| Shell `Samples` catalog | Canary entries filtered out before `Shell.Samples` assignment | Full catalog (Canary included) |
| `AddNavigationItems` Canary filter | Removed (catalog already filtered upstream) | Same |
| Overview page copy | Neutral: "supported Uno design systems" | Same (no conditional XAML) |

All filtering is done at a single site (`BuildShell` → `Shell.Samples`) so navigation,
sidebar search, backdoor navigation, and `NavigateToAllPages` all see a consistent catalog.
`AddNavigationItems` receives the pre-filtered array and no longer needs its own Canary guard.

`AOT_PROFILE_GEN` is included on the "expose Cupertino" side so that AOT profiling runs
exercise all pages and generate complete profile data.

---

## Consequences

### Positive

- Stable Release users never see broken or incomplete Cupertino UI.
- A single removal point (`Shell.Samples` filtering) keeps all shell surfaces consistent:
  catalog, sidebar, search, deep links, and `NavigateToAllPages` all operate on the same
  pre-filtered array, so no Cupertino sample is reachable from stable Release.
- `SamplePageLayout.OnApplyTemplate` nulls the `CupertinoTemplate` DP before the control
  template is applied, so the Cupertino ContentPresenter never instantiates a DataTemplate
  even if a page were somehow reached.
- Resources, XAML templates (including Cupertino brush templates and resource dictionaries),
  and all Cupertino UI test fixtures remain compiled and exercised by CI; no regressions go
  undetected.
- Rollback is mechanical: delete the `#if` guards and restore the two Overview strings.

### Negative / Risks

- Developers must remember to use `IS_CANARY_BUILD`, `DEBUG`, `USE_UITESTS`, or
  `AOT_PROFILE_GEN` to see Cupertino UI locally in a Release configuration.
- New Cupertino-only samples must explicitly opt in to `SampleCategory.Canary` in their
  `[SamplePage]` attribute, or they will appear in stable Release.

---

## Resources Retained (Not Removed)

- `UnoFeatures` entries for `Cupertino` in project files
- Resource dictionaries under `Styles/`
- XAML templates referencing Cupertino brush names
- Test pages exercising Cupertino controls
- All Cupertino UI test cases (Given_CheckBox_04_Cupertino, Given_RadioButton_03_Cupertino,
  Given_PasswordBox_Cupertino, etc.)

These are deliberately kept so that the package continues to be validated end-to-end.

---

## Rollback

```
git revert <merge-sha>
```

Or manually:
- Remove the `#if DEBUG || IS_CANARY_BUILD || USE_UITESTS || AOT_PROFILE_GEN` guards in
  `SamplePageLayout.cs`, `CupertinoPalettePage.xaml.cs`, `SegmentedControlSamplePage.xaml.cs`.
- Remove the `CupertinoTemplate = null` block from `SamplePageLayout.OnApplyTemplate`.
- Restore the `AddNavigationItems` Canary filter.
- Remove the `Where(Canary)` clause from `BuildShell`.
- Restore the two Overview bullet-point strings.

---

## References

- https://github.com/unoplatform/uno.gallery/issues/1210
- https://github.com/unoplatform/uno.gallery/issues/874
- https://github.com/unoplatform/uno.gallery/issues/1079
- https://github.com/unoplatform/uno.gallery/issues/1224
- Uno.Cupertino package: https://platform.uno/docs/articles/external/uno.themes/doc/cupertino-getting-started.html

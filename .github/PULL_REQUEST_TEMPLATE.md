<!--
  Fill in the relevant sections. Delete sections that do not apply.
  Link the issue this PR addresses before requesting review.
-->

## Summary

<!-- What does this PR change and why? -->

## Linked issue

<!-- Required for non-trivial changes. -->
Fixes # <!-- issue number -->

## Change type

<!-- Check all that apply. -->

- [ ] Bug fix
- [ ] New sample / updated sample
- [ ] Refactor / code quality
- [ ] Build / CI
- [ ] Documentation
- [ ] Other (describe below)

## Supported platforms / renderers

<!-- Mark every combination you verified. Leave unchecked if not applicable. -->

| Platform | Native / DOM | Skia |
|---|---|---|
| Windows (WinAppSDK) | | |
| Android | | |
| iOS | | |
| macOS (Catalyst) | | |
| Linux (Skia Desktop) | | |
| WebAssembly | | |

## Sample page checklist

<!-- Complete this section when adding or modifying a sample page. -->

- [ ] New Stable samples use `ContractVersion = 1` with description, docs, tags,
      owner, ISO review date, typed design/renderer support, requirements,
      accessibility notes, reset behavior, and meaningful variants
- [ ] Optional known limitations and issue/API links are accurate and omitted
      when they do not apply
- [ ] `[SampleConditional]` reflects actual platform support
- [ ] Cupertino samples use `SampleCategory.Canary`
      (hidden in stable Release — see [ADR 0001](docs/decisions/0001-cupertino-containment.md))
- [ ] Design templates are provided for each supported design system
- [ ] `SupportedDesigns` matches the authored templates and
      `SupportedRenderers` matches verified renderer support
- [ ] Automation IDs (`x:Name` / `AutomationProperties.AutomationId`) are set on
      interactive elements that UITests need to locate

## Test evidence

<!-- Describe how you verified this change works. Attach screenshots or recordings. -->

## Visual / accessibility / performance impact

<!-- Note any visual regressions, accessibility concerns, or performance impact. -->

## Maintainer classification

<!-- Maintainers: complete before merge for non-trivial changes. -->

- [ ] Kind, area, and priority labels are applied
- [ ] Platform, renderer, and design-system scope is recorded
- [ ] Owner and milestone (or explicit backlog disposition) are assigned
- [ ] Upstream issue is linked when Gallery does not own the root cause
- [ ] Dependency exceptions include an issue and next review date

## Release impact

- [ ] Compatibility matrix remains accurate
- [ ] Stable/canary exposure is intentional
- [ ] Rollout and rollback implications are documented
- [ ] No store credential, certificate, token, or signing material is committed

## Intentional skipped tests

<!-- If you added or retained a [Ignore] / Assert.Ignore, confirm it carries a
     valid GitHub issue URL and a non-past review-date, or explain why it was skipped. -->

- [ ] No new test ignores added
- [ ] New ignores: all carry `https://github.com/unoplatform/Uno.Gallery/issues/<n>`
      and a non-past `review-date: YYYY-MM-DD`

## Lint

- [ ] `pwsh build/scripts/lint-test-ignores.ps1` exits 0

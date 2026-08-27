# Stable quality checklist

## Identity and dependencies

- [ ] NBGV computes the intended stable version from `release/stable/*`.
- [ ] Build identity shows the release version and commit SHA.
- [ ] Uno.Sdk and package versions match the approved dependency update.
- [ ] Compatibility and coverage manifests are attached to the release.

## Build matrix

- [ ] Windows package builds and signing succeeds.
- [ ] Android Native and Skia bundles build; NativeAOT instrumentation passes.
- [ ] iOS Native and Skia archives build with trimming/AOT enabled as intended.
- [ ] macOS Catalyst and Linux/Desktop startup builds succeed.
- [ ] DOM WASM production publish succeeds.
- [ ] Skia WASM production AOT publish succeeds without profiling flags.

## Quality

- [ ] Source-generator/catalog tests pass.
- [ ] Stable all-sample smoke passes for each declared primary target.
- [ ] Curated interaction suite passes with only issue-tracked, unexpired skips.
- [ ] Visual pilot matches approved baselines.
- [ ] Visual report records the pinned browser, viewport, per-sample mismatch
      count, and five clean-session runs; current/diff/report artifacts are
      retained with release evidence.
- [ ] Baseline changes include fresh visual-mode Skia-WASM publish evidence,
      metadata/config digest updates, and two-person image approval.
- [ ] Pseudo-localization, long-string, bidi, and RTL checks pass.
- [ ] Accessibility automation passes and manual AT checks are recorded where
      automation is partial.
- [ ] No unhandled startup, navigation, or sample-host exceptions are present.

## Performance and delivery

- [ ] Fingerprinted WASM assets retain immutable caching; HTML and service worker
      remain revalidated.
- [ ] Production DOM and Skia artifacts pass the pinned-browser startup probe
      without interpreter assertions or local asset failures.
- [ ] Raw and compressed bundle deltas are within the approved budget.
- [ ] Startup, first-input, search, and navigation p75 results are attached.
- [ ] Known regressions, rollout stage, owner, and rollback decision are recorded.
- [ ] Store and web credentials are supplied only through protected environments.

## Release record

Create release notes from `release-notes-template.md`. Attach build/test metadata,
compatibility matrix, coverage changes, performance results, and known
limitations. Keep the current stable release fully supported and the previous
stable release limited to critical fixes.

The `Release_Validation` stage runs for rolling `master` builds and
`release/stable/*` branches, depends on the complete build and test matrix, and
publishes `release-metadata.json`. Rolling store publication and production web
stages from `master` depend on that gate; web staging remains available after
its WASM build and test stages so deployment problems can be found early.
Stable branches publish validation evidence without racing `master` for the
same rolling channels.

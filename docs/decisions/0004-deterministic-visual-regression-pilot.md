# ADR 0004 — Deterministic visual-regression pilot

**Date:** 2026-08-27  
**Status:** Proposed  
**Branch:** `modernization/visual-pilot`

## Context

Interaction tests catch behavior but not renderer, theme, font, spacing, or
template regressions. A broad screenshot suite would immediately inherit
unstable media, network, sensor, animation, time, random-data, and platform
surfaces. The first gate therefore needs a small, reviewable contract and a
measured path from advisory evidence to blocking.

## Decision

### Scope and storage

Version 1 covers 14 entries in `build/visual/visual.config.json`: shell/overview
in Material and Fluent; Material and Fluent buttons; Fluent TextBox; Material
design tokens and palette; Fluent palette; Grid and ListView; Toolkit AutoLayout
and CardContentControl; Accessibility; and Localization/RTL. Animated,
media, network, device, sensor, random, and clock-dependent pages are excluded.

Approved `1200x900` PNGs and `baselines/metadata.json` are ordinary Git files.
There is no Git LFS, external object store, or hidden snapshot service. Metadata
pins schema/suite, ordered routes, masks, viewport, browser, renderer, fonts,
configuration digest, and lockfile digest. Missing, extra, malformed,
wrong-dimension, or stale baselines fail.

### Deterministic host

Approval runs on Windows with the lockfile-installed Chrome for Testing
152.0.7977.42 and Puppeteer 25.8.0. Device scale is 1; locale is `en-US`;
timezone is UTC; color is light/sRGB; reduced motion is requested. Chromium uses
SwiftShader flags where viable. The tested app is a fresh Release Skia-WASM
publish with `EnableVisualRegression=true`, reusing the same publish artifact in
CI rather than rebuilding inside the comparator.

OpenSans, Material typefaces, and the symbol font come from app packages, not
host font discovery. Capture waits for canonical URL navigation, the app-owned
sequenced `app.visual_ready.<n>` performance marks emitted after four app render
frames, `document.fonts.ready`, and repeated four-frame groups until three
pixel-identical captures are observed. The harness removes only the bootstrap
loader overlay after the app mark and rejects low-content frames. Routes change
inside one warmed app tab, avoiding runtime-startup pixels while every top-level
run still uses a new browser process/profile. Automation IDs are not used
because Skia-WASM paints into a canvas.

### Compile mode and animation policy

`EnableVisualRegression=true` defines `VISUAL_REGRESSION` and performance marks
only for Skia-WASM. It fixes the app theme and build-identity label, hides
dynamic web chrome, suppresses analytics initialization/view tracking, and
turns off app-owned `SamplePageLayout` transitions. It does not write
`UISettings.AnimationsEnabled`. No pilot page starts an inherent animation.
Every hook is preprocessor-guarded; disabled Release builds retain their
existing code paths and labels.

### Tolerance and masks

PNG dimensions must match. Pixelmatch uses threshold `0.05`, anti-alias
classification excluded, and a maximum different-pixel ratio of `0.0001`
(`0.01%`, 90 pixels at this viewport). Current masks are empty. A mask is allowed
only for a reviewed, unavoidable nondeterministic subregion; it must use fixed
integer coordinates in config, be visible in metadata, and may not hide a whole
control or text region. Raising tolerance or adding a mask requires ADR-level
review and fresh five-run evidence.

### Approval and rebaseline

Only a Windows maintainer may run the explicit `update` command, and must also
set `VISUAL_ACCEPT_BASELINES=1`. CI environment variables make update fail
closed. The maintainer must:

1. produce a fresh visual-mode Release Skia-WASM publish;
2. run unit and negative tests;
3. update and inspect all changed PNGs and metadata;
4. run five clean comparisons and attach mismatch counts;
5. explain intentional visual changes in the pull request.

Two-person review is required for baseline, tolerance, mask, browser, viewport,
font, or sample-list changes. Reviewers inspect rendered current/diff evidence,
not only binary-file names.

### Advisory to blocking

`Visual_Regression` initially reports advisory because five repeated hosted
executions on the selected `windows-latest` image have not yet been accumulated.
It always publishes current, diff, and HTML/JSON reports. Promote only this
14-entry pilot to blocking after five consecutive local runs and five
consecutive hosted runs show zero failures, no stale-baseline bypass, and a
maximum mismatch within tolerance, with no browser/image drift. Any unexplained
flake returns it to advisory. Expansion beyond 15 entries is a separate decision.

## Consequences

The pilot gives small, auditable visual coverage without infrastructure beyond
Git and Azure artifacts. It costs repository bytes and Windows CI time, and a
browser/renderer/font update intentionally invalidates metadata. Tight scope and
fail-closed update behavior trade coverage breadth for trustworthy evidence.

## Rollback

Remove the `Visual_Regression` stage, `build/visual`, guarded
`VISUAL_REGRESSION` code, and this ADR. Normal builds are unaffected because the
mode is opt-in and compiled out otherwise.

# 0008: Versioned performance budgets

- Status: Accepted for advisory enforcement
- Date: 2026-08-27

## Context

Uno Gallery had isolated bundle and startup observations, but they were produced
with different publish directories, compression rules, browsers, AOT profiles,
and cache states. Those values could not safely block a pull request or certify
a release. Content work also had no repeatable way to show its bundle cost.

## Decision

Performance evidence is a versioned contract with four JSON schemas:

- `wasm-bundle-metrics-v1.schema.json` records production artifact sizes;
- `runtime-observation-v1.schema.json` records raw cold and warm runs plus
  nearest-rank p50 and p75 summaries;
- `performance-budget-v1.schema.json` stores reviewed baselines and maxima;
- `performance-report-v1.schema.json` records every evaluated budget check.

Bundle accounting excludes `.br`, `.gz`, and source-map sidecars from raw
payload bytes. Estimated transfer selects the committed Brotli sidecar when one
exists, then gzip, then the raw file. This prevents one artifact from being
counted two or three times. `dotnet.native*.wasm` is reported separately.

Runtime certification uses Chrome for Testing `152.0.7977.42`, the same pinned
browser as the visual pilot, a `1200x900@1` viewport, `en-US`, UTC, reduced
motion, light theme, and SwiftShader. A cold run launches a clean browser profile
with cache disabled. Warm runs share one browser profile after an unreported
warm-up launch. The local server serves precompressed Brotli assets and models
immutable fingerprinted-resource caching while keeping boot files revalidated.

The runtime artifact is Release DOM WebAssembly with
`EnablePerformanceMeasurements=true`; it is evidence, not a deployable artifact.
The production DOM and Skia artifacts remain uninstrumented and are the only
inputs to bundle budgets.

The measured runtime metrics are:

- browser first-contentful paint;
- `app.shell_loaded` from navigation start;
- first-input processing delay from the Event Timing API, with the app-owned
  `app.first_input` mark proving the routed handler ran;
- app-owned search completion after the next composition frame;
- app-owned navigation completion after the destination page renders two
  frames.

All raw observations are retained in the CI artifact. p50 and p75 use the
discrete nearest-rank definition rather than interpolation.

## Enforcement

Every production WebAssembly build emits bundle metrics and an advisory budget
report. Non-PR builds additionally collect ten cold and ten warm runtime
observations. Budget violations produce a warning while budget status is
`advisory`.

Changing a budget requires a reviewed budget-version increment and evidence,
not merely making CI green. Blocking status requires at least five consecutive
hosted runs with ten valid cold and warm observations, investigated outliers,
and explicit release-owner approval. Missing or insufficient observations fail
checks once the budget is blocking.

## Consequences

- Bundle changes are visible on every PR without running a browser benchmark.
- Runtime trends are reproducible enough to compare release candidates.
- The extra instrumented AOT build and browser runs increase non-PR CI time.
- Host-image drift is recorded in every observation and requires a new baseline
  rather than silent threshold widening.

# Performance baseline

## Status

Performance budget version 1 is advisory. The authoritative limits live in
`docs/performance/performance-budget-v1.json`; this page explains how to produce
and promote them.

The modernization baseline has already established:

- production WASM profiling is disabled unless explicitly requested;
- post-link native stripping is enabled;
- normal Release logging is disabled;
- fingerprinted framework assets are cached immutably;
- startup marks are compiled out of normal Release unless measurement is
  explicitly enabled.

Earlier ad-hoc size and startup values are not a baseline. They mixed stale
publish directories, compression sidecars, renderer outputs, and browser cache
states. The versioned scripts intentionally reject those comparisons.

## Metrics and artifacts

`build/scripts/measure-wasm-bundle.ps1` records raw payload, estimated Brotli
transfer, native WASM, and managed WebCIL bytes from a fresh publish directory.
`build/visual/src/performance-cli.mjs` records first-contentful paint, shell
readiness, first-input processing delay, search rendering, and sample navigation.

The CI `performance` artifact contains the ten cold and ten warm raw runs and
the comparison report. The `WASM-DOM-catalog` and `WASM-Skia-catalog` artifacts
contain production bundle metrics and their reports.

## Certification method

1. Publish production DOM and Skia artifacts into new directories and record
   their bundle metrics.
2. Publish the Release DOM instrumentation flavor without changing the
   deployable artifacts.
3. Run at least ten cold and ten warm launches with the committed configuration.
4. Store raw observations with build SHA, browser, OS release, architecture,
   Node version, renderer, viewport, locale, timezone, and GPU mode.
5. Calculate discrete nearest-rank p50 and p75 and investigate outliers.
6. Update the baseline and maxima only with a budget-version increment.
7. Promote to blocking only after the ADR's hosted-run and owner-approval gate.

Every stable release must publish the raw observation artifact and approved
budget version rather than only a pass/fail result.

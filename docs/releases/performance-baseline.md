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
DOM and Skia are measured with unprofiled AOT because their historical profiles
no longer cover evolving module and generated theme-resource initializers.
Runtime observations use a separate instrumented, unprofiled-AOT DOM flavor and
are labeled as such in their schema.

The CI `performance` artifact contains the ten cold and ten warm raw runs and
the comparison report. The `WASM-DOM-catalog` and `WASM-Skia-catalog` artifacts
contain production bundle metrics and their reports. Optional Extensions
catalog artifacts carry `flavor=extensions` metrics for an explicit
default-versus-optional delta.

## Budget version 1 observations

Fresh production artifacts establish these bundle baselines:

| Bundle metric | DOM | Skia |
|---|---:|---:|
| Raw payload | 158,981,031 B | 171,267,191 B |
| Estimated Brotli transfer | 46,154,083 B | 49,210,466 B |
| `dotnet.native.wasm` | 91,869,053 B | 97,129,152 B |
| `dotnet.native.wasm.br` | 12,864,467 B | 13,854,939 B |

The larger native payloads are the explicit correctness cost of unprofiled AOT;
the smaller historical profiles abort in evolving module/resource initializers.

For comparison, the rejected profile-guided artifacts measured approximately
90.87 MB raw / 38.74 MB estimated transfer for DOM and 101.00 MB / 42.32 MB for
Skia. They are not eligible baselines: both later aborted before the shell after
ordinary source/metadata changes. Release startup correctness takes precedence,
and the blocking probe prevents silently returning to those invalid outputs.

The initial Release DOM observation used the committed Chrome
`152.0.7977.42`/SwiftShader configuration and ten runs per cache profile:

| Runtime p75 | Cold | Warm |
|---|---:|---:|
| First-contentful paint | 956 ms | 208 ms |
| Shell ready | 4,075 ms | 3,005.5 ms |
| First-input processing delay | 55.8 ms | 39.1 ms |
| Search rendered | 106.8 ms | 91.9 ms |
| Sample navigation rendered | 541.6 ms | 523 ms |

These values establish the advisory baseline, not blocking status. CI records
its exact Windows image, Node version, and every raw observation so hosted-run
drift can be evaluated before promotion.

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

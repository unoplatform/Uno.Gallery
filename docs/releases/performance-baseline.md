# Performance baseline

## Status

Performance gates remain advisory until repeated runs on a pinned host establish
a stable false-positive rate. Do not convert one local measurement into a hard
budget.

The modernization baseline has already established:

- production WASM profiling is disabled unless explicitly requested;
- post-link native stripping is enabled;
- normal Release logging is disabled;
- fingerprinted framework assets are cached immutably;
- startup marks are compiled out of normal Release unless measurement is
  explicitly enabled.

Initial, non-certified observations on one Windows host were approximately:

| Metric | Observation |
|---|---|
| `dotnet.native.wasm`, raw | 27.2 MB |
| `dotnet.native.wasm`, Brotli | 5.7 MB |
| `catalog_ready` | 3.37 s |
| `shell_loaded` | 5.39 s |

These numbers are reference observations only. Host load, build profile,
renderer, cache state, browser, and GPU were not yet controlled tightly enough
for a release gate.

## Certification method

1. Pin agent image, browser, renderer, viewport, build configuration, and cache
   state.
2. Run at least ten cold and ten warm launches.
3. Record bundle sizes plus startup, shell-loaded, first-input, search, and
   navigation durations.
4. Store raw observations with build SHA and environment metadata.
5. Calculate p50 and p75; investigate outliers before approving a baseline.
6. Apply PR delta checks first, then make thresholds blocking only after the
   advisory period proves them stable.

Every stable release must publish the raw observation artifact and approved
budget version rather than only a pass/fail result.

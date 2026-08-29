# Compatibility matrix

This matrix describes the evidence expected for a stable Gallery release. A
successful compile is not equivalent to complete control, accessibility, or
renderer parity.

| Platform | Native / DOM | Skia | Stable certification |
|---|---|---|---|
| Windows | WinAppSDK package | Desktop renderer | Build, package signing, targeted native smoke, curated stable samples |
| Android | Native renderer | Skia renderer | Build, instrumentation smoke, permissions and lifecycle checks |
| iOS | Native renderer | Skia renderer | Build, TestFlight smoke, trimming/AOT checks, manual accessibility pass |
| macOS Catalyst | Native renderer where supported | Skia renderer | Build, startup/navigation smoke, manual VoiceOver pass |
| Linux | Not applicable | Skia Desktop | Build, startup/navigation smoke |
| WebAssembly | DOM renderer | Skia renderer | DOM all-sample smoke and curated interactions; Skia build/startup and semantic bridge checks |

## Design systems

- Material and Fluent are stable presentation surfaces.
- Design-agnostic samples must render without depending on a hidden theme tab.
- Cupertino resources remain available for validation, but Cupertino content is
  hidden from normal stable Release builds under ADR 0001.
- Canary and UITest builds may expose experimental or incomplete surfaces when
  their status and limitations are visible.

## Current known limitations

- Skia WASM UI automation does not yet provide the same test locator surface as
  DOM WASM; do not claim interaction-test parity from build success alone.
- iOS and macOS accessibility support needs a manual release pass where
  automated semantic checks are incomplete.
- Store publication depends on protected service connections, signing assets,
  and environment approvals outside this repository.
- Platform APIs requiring sensors, permissions, media runtimes, or external
  providers must show an unsupported or setup state rather than silently doing
  nothing.

Update this file for every stable release and link concrete pipeline artifacts
from the release notes.

# Accessibility renderer matrix

This matrix records the validation contract, not a claim of complete parity.
Review it after every Uno.Sdk minor update.

| Target | Automated release evidence | Manual release evidence | Current limitation |
|---|---|---|---|
| Windows WinAppSDK | Automation names, focus order, control patterns, state changes | Narrator keyboard pass | Custom templates still require peer-level review |
| Windows Skia Desktop | UI Automation provider and focus checks | Narrator keyboard pass | Pattern coverage can differ from native WinAppSDK |
| WebAssembly DOM | ARIA/name/state mapping, keyboard focus, all-sample smoke | Browser screen-reader spot check | Browser and screen-reader combinations vary |
| WebAssembly Skia | Semantic bridge and keyboard checks where exposed | Browser screen-reader pass | Locator and semantic parity remains partial |
| Android Native | Platform automation names/states and focus | TalkBack pass | Device and API-level behavior varies |
| Android Skia | Semantic surface checks where exposed | TalkBack Explore-by-Touch pass | Custom canvas semantics need explicit peers |
| iOS Native | Platform automation names/states | VoiceOver pass | Local automation does not replace VoiceOver |
| iOS Skia | Spike/manual evidence required | VoiceOver pass | Support remains unresolved until verified on the release SDK |
| macOS Catalyst / Skia | Startup, focus, and available semantic checks | VoiceOver pass | Automated VoiceOver coverage is partial |

Every stable sample should document keyboard behavior, accessible names and
states, known renderer limitations, and any manual assistive-technology step
that cannot yet be automated.

Run `pwsh build/scripts/lint-accessibility-metadata.ps1` before changing the
dedicated accessibility sample. The lint requires its semantic names, help text,
polite live-region setting, and explicit `LiveRegionChanged` automation event.

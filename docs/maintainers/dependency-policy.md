# Dependency freshness policy

## Response targets

- Open a tested Uno.Sdk stable update within fourteen days of an upstream
  stable release.
- Review stable Toolkit, Themes, and Extensions updates within twenty-one days.
- Handle security updates under the applicable accelerated security process.
- Keep canary builds on prerelease Uno packages to discover compatibility
  problems before a stable update.

Dependabot groups related changes but never merges dependency updates
automatically. Each group must pass its relevant platform, renderer, AOT,
trimming, and UI-test matrix.

## Required evidence

| Dependency area | Minimum evidence |
|---|---|
| Uno.Sdk / Uno.UI | Generator tests, Windows, Android, desktop, DOM WASM, Skia WASM AOT, curated UI suite |
| Toolkit / Themes | Affected samples, Material and Fluent design switching, resource/linker validation |
| Extensions | Optional-flavor builds, WASM AOT, mobile trimming, startup and bundle delta |
| Test tooling | Targeted tests plus one complete UI run on the supported runner |
| Native media/platform packages | Owning platform build and device or hosted-agent smoke |

## Existing exceptions

The version ignores in `.github/dependabot.yml` are historical compatibility
exceptions, not permanent policy. Revalidate them individually before removal:

- Xamarin.UITest and AndroidX packages can affect device-test infrastructure.
- TypeScript compiler and MSBuild packages must be aligned in a dedicated
  update.
- Microsoft.NET.Test.Sdk and logging changes must preserve the current NUnit and
  multi-target build behavior.
- Uno.Core changes must be validated with the Uno.Sdk dependency graph.

Every retained exception should gain an issue link and review date when it is
next touched. New broad or open-ended ignores are not allowed.

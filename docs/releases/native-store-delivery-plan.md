# Native store delivery plan

## Existing channels

The Azure pipeline already builds and signs:

- Android Native (`uno.platform.gallery_native`) and Skia
  (`com.nventive.uno.ui.demo`) bundles, then publishes to Play alpha;
- iOS Native (`uno.platform.gallery.native`) and Skia
  (`com.nventive.uno.gallery`) archives, then publishes to TestFlight;
- a signed Windows package artifact, without Store submission.

## Current automation

Protected `master` builds and publishes the existing rolling web, Play alpha,
and TestFlight channels after `Release_Validation`. A `release/stable/*` branch
produces the same validation evidence but does not publish into those rolling
channels, preventing independently moving branches from racing for one target.

## Target promotion model

1. Build immutable artifacts once from the stable release commit.
2. Run the stable quality checklist against those exact artifacts.
3. Publish Android and iOS to internal/alpha/TestFlight channels.
4. Record smoke, crash, startup, and platform-owner approval.
5. Promote the same artifacts in stages; do not rebuild between channels.
6. Keep the prior stable version available for rollback.

Native and Skia applications have distinct identities. Release notes and
telemetry must never combine their certification results.

The target model requires a future artifact-promotion workflow. Until then,
stable production promotion is a store-owner operation using the validated
stable artifacts and protected environments.

## Required protected resources

- Android signing material and Google Play service connection;
- iOS certificates, provisioning profiles, App Store Connect connection, and
  application-specific IDs;
- Windows signing identity and Partner Center connection;
- protected Azure environments with named approvers;
- documented credential rotation and emergency rollback owners.

These resources are intentionally not stored in this repository.

## Windows decision

Windows Store submission remains gated until product and store owners confirm:

- Store identity and package family name;
- MSIX/AppInstaller distribution strategy;
- Native versus Skia presentation;
- Partner Center service connection;
- staged rollout and rollback process.

Until accepted, CI produces and signs the Windows artifact but does not submit
it automatically.

## Release evidence

Each promotion must retain the commit/version, artifact hashes, target channel,
approval, compatibility matrix, test evidence, performance snapshot, known
limitations, and rollback version.

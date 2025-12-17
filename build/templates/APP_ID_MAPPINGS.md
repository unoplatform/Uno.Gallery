# Uno Gallery - Application ID Mappings

This document provides a comprehensive mapping of Application IDs, App Store IDs, and publish configurations across all platforms and variants (Production and Canary).

## macOS App Store Requirements

For macOS TestFlight submissions, the following properties **must** be present in the app's Info.plist:

- **`LSApplicationCategoryType`**: Application category (e.g., `public.app-category.utilities`)
- **`LSMinimumSystemVersion`**: Minimum macOS version required (e.g., `15.0`)

The following entitlements **must** be enabled in Entitlements.plist:

- **`com.apple.security.app-sandbox`**: `true` (required for App Store distribution)
- **`com.apple.security.network.client`**: `true` (for network access)

**Files**: `Uno.Gallery/Platforms/Desktop/Info.plist` and `Entitlements.plist`

---

## Production (Master) Builds

### Android

| Variant | Application ID | Bundle File | Publish Template | Google Play |
|---------|---------------|-------------|------------------|-------------|
| **Native** | `uno.platform.gallery.native` | `uno.platform.gallery.native-Signed.aab` | `stage-publish-android.yml` | Alpha Track |
| **Skia** | `com.nventive.uno.ui.demo` | `com.nventive.uno.ui.demo-Signed.aab` | `stage-publish-android.yml` | Alpha Track |

### iOS

| Variant | Application ID | Apple App ID | Publish Template | TestFlight |
|---------|---------------|--------------|------------------|------------|
| **Native** | `uno.platform.gallery.native` | `6745625441` | `stage-publish-ios.yml` | Yes |
| **Skia** | `com.nventive.uno.gallery` | `1380984680` | `stage-publish-ios.yml` | Yes |

### macOS Desktop

| Variant | Application ID | Apple App ID | Publish Template | TestFlight |
|---------|---------------|--------------|------------------|------------|
| **Skia** | `com.nventive.uno.gallery` | `1380984680` | `stage-publish-macos-desktop.yml` | Yes |

---

## Canary Builds

### Canary Updater Rules

The `canary-updater.yml` automatically transforms Application IDs during canary builds:

| Platform | Original ID | Canary Rule | Result ID | Note |
|----------|------------|-------------|-----------|------|
| **Android Skia** | `com.nventive.uno.ui.demo` | Replace with `.canary` | `com.nventive.uno.ui.demo.canary` | Dot + dash |
| **Android Native** | `uno.platform.gallery.native` | Replace with `_canary` | `uno.platform.gallery.native_canary` | **Underscore** (Android naming rules) |
| **iOS Native** | `uno.platform.gallery.native` | Replace with `-canary` | `uno.platform.gallery.native-canary` | **Dash** (Apple convention) |
| **iOS Skia** | `com.nventive.uno.gallery` | Replace with `-canary` | `com.nventive.uno.gallery-canary` | Dash |
| **macOS** | `com.nventive.uno.gallery` | Replace with `-canary` | `com.nventive.uno.gallery-canary` | Dash |

### Android Canary

| Variant | Application ID | Bundle File | Publish Template | Google Play |
|---------|---------------|-------------|------------------|-------------|
| **Native** | `uno.platform.gallery.native_canary` | `uno.platform.gallery.native_canary-Signed.aab` | `stage-publish-android-canary.yml` | Alpha Track |
| **Skia** | `com.nventive.uno.ui.demo.canary` | `com.nventive.uno.ui.demo.canary-Signed.aab` | `stage-publish-android-canary.yml` | Alpha Track |

### iOS Canary

| Variant | Application ID | Apple App ID | Publish Template | TestFlight |
|---------|---------------|--------------|------------------|------------|
| **Native** | `uno.platform.gallery.native-canary` | `6745625865` | `stage-publish-ios-canary.yml` | Yes |
| **Skia** | `com.nventive.uno.gallery-canary` | `1619130328` | `stage-publish-ios-canary.yml` | Yes |

### macOS Desktop Canary

| Variant | Application ID | Apple App ID | Publish Template | TestFlight |
|---------|---------------|--------------|------------------|------------|
| **Skia** | `com.nventive.uno.gallery-canary` | `1619130328` | `stage-publish-macos-desktop-canary.yml` | Yes |

---

## Platform Naming Conventions

### Android Package Names
- **Rules**: Only alphanumeric (a-z, A-Z, 0-9), dots (.), and underscores (_)
- **No dashes allowed** (Java package naming convention)
- **Canary suffix**: Uses underscore `_canary`

### iOS/macOS Bundle IDs
- **Rules**: Alphanumeric, dots (.), and dashes (-)
- **Dashes preferred** (Apple convention)
- **Canary suffix**: Uses dash `-canary`

---

## File Locations

### Configuration Files
- **Application IDs**: `Uno.Gallery/Uno.Gallery.csproj` (lines 27-35)
- **Canary Updater**: `build/templates/canary-updater.yml` (lines 56-90)

### Publish Templates
| Template | Path |
|----------|------|
| Android Master | `build/templates/master-publish/stage-publish-android.yml` |
| Android Canary | `build/templates/canary-publish/stage-publish-android-canary.yml` |
| iOS Master | `build/templates/master-publish/stage-publish-ios.yml` |
| iOS Canary | `build/templates/canary-publish/stage-publish-ios-canary.yml` |
| macOS Master | `build/templates/master-publish/stage-publish-macos-desktop.yml` |
| macOS Canary | `build/templates/canary-publish/stage-publish-macos-desktop-canary.yml` |

---

## Apple App Store Reference

### Active Apps
| App Name | Variant | Bundle ID | Apple App ID | Status |
|----------|---------|-----------|--------------|--------|
| **Uno Gallery** | Production | `com.nventive.uno.gallery` | `1380984680` | ✅ Active |
| **Uno Gallery (Canary)** | Canary | `com.nventive.uno.gallery-canary` | `1619130328` | ✅ Active |
| **Uno Gallery Native** | Production | `uno.platform.gallery.native` | `6745625441` | ✅ Active |
| **Uno Gallery Native (Canary)** | Canary | `uno.platform.gallery.native-canary` | `6745625865` | ✅ Active |

---

## Verification Checklist

When making changes to Application IDs, verify:

- [ ] `.csproj` ApplicationId matches the target platform conventions
- [ ] Canary updater transforms the ID correctly (underscore vs dash)
- [ ] Publish template `appIdentifier`/`applicationId` matches the transformed ID
- [ ] Publish template `bundleFile`/`ipaPath` uses the correct filename pattern
- [ ] Apple App ID in publish template matches the App Store Connect entry
- [ ] Provisioning profiles reference the correct bundle IDs

---

**Last Updated**: December 16, 2025
**Maintainer**: Uno Platform Team

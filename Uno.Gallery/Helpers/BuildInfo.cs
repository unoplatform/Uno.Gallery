// Copyright (c) Uno Platform Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Uno.Gallery.Helpers;

/// <summary>
/// Provides build and deployment identity read once at startup from assembly attributes.
/// </summary>
internal static class BuildInfo
{
	static BuildInfo()
	{
		var asm = typeof(BuildInfo).Assembly;

		// ---- AssemblyConfiguration -------------------------------------------------
		Configuration = asm.GetCustomAttribute<AssemblyConfigurationAttribute>()
			?.Configuration ?? string.Empty;

		// ---- AssemblyInformationalVersion ------------------------------------------
		// Expected format: {semver}+{40-char-sha}  (e.g. 1.7.0-dev.42+abc...def)
		// NBGV also emits:  {semver}+g{short-sha}  (e.g. 1.7.0-dev.42+g42b0e953)
		// If no '+' is present the whole string is treated as SemVer.
		string infoVer = asm
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
			?.InformationalVersion ?? string.Empty;

		int plusIdx = infoVer.IndexOf('+');
		if (plusIdx >= 0)
		{
			SemVer = infoVer.Substring(0, plusIdx);
			string afterPlus = infoVer.Substring(plusIdx + 1);

			// Strip optional leading 'g' (NBGV emits "+g<short-sha>").
			int hexStart = afterPlus.Length > 0 &&
			               (afterPlus[0] == 'g' || afterPlus[0] == 'G') ? 1 : 0;
			string candidate = afterPlus.Substring(hexStart);

			// Take leading contiguous hex chars, max 40.
			int hexLen = 0;
			while (hexLen < candidate.Length && hexLen < 40 && IsHexChar(candidate[hexLen]))
			{
				hexLen++;
			}

			if (hexLen >= 7)
			{
				// Normalize to lowercase for consistent display.
				CommitSha = candidate.Substring(0, hexLen).ToLowerInvariant();
				ShortSha  = CommitSha.Substring(0, 7);
			}
			else
			{
				CommitSha = string.Empty;
				ShortSha  = string.Empty;
			}
		}
		else
		{
			SemVer    = infoVer;
			CommitSha = string.Empty;
			ShortSha  = string.Empty;
		}

		// ---- Renderer resolution ---------------------------------------------------
		// 1. Prefer the injected AssemblyMetadataAttribute (set by CI per target).
		string runtimeId = string.Empty;
		string targetFramework = string.Empty;
		foreach (var attr in asm.GetCustomAttributes<AssemblyMetadataAttribute>())
		{
			if (attr.Key == "UnoUIRuntimeIdentifier")
			{
				runtimeId = attr.Value ?? string.Empty;
			}
			else if (attr.Key == "UnoGalleryTargetFramework")
			{
				targetFramework = attr.Value ?? string.Empty;
			}
		}

		// 2. Fall back to compile-time symbols when the metadata attribute is absent.
		// WINDOWS wins over HAS_SKIA_RENDERER because WinAppSDK is the native host
		// on Windows desktop and should be reported as such even when Skia rendering
		// is also active.
		if (!string.IsNullOrEmpty(runtimeId))
		{
			Renderer = runtimeId;
		}
		else
		{
#if WINDOWS
			Renderer = "WinAppSDK";
#elif __WASM__ && !HAS_SKIA_RENDERER
			Renderer = "DOM";
#elif HAS_SKIA_RENDERER
			Renderer = "Skia";
#else
			Renderer = "Native";
#endif
		}

		// ---- Platform/backend/build mode -------------------------------------------
		TargetFramework = string.IsNullOrEmpty(targetFramework)
			? asm.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName ?? "Unknown"
			: targetFramework;

#if WINDOWS
		Platform = "Windows";
		Backend = "WinAppSDK / DirectComposition";
#elif __WASM__
		Platform = "WebAssembly";
#if HAS_SKIA_RENDERER && (__WASM__ || __DESKTOP__)
		Backend = "CanvasKit / WebGL";
#else
		Backend = "Browser DOM / CSS";
#endif
#elif __ANDROID__
		Platform = "Android";
#if HAS_SKIA_RENDERER
		Backend = "Skia mobile";
#else
		Backend = "Android native views";
#endif
#elif __IOS__
		Platform = "iOS";
#if HAS_SKIA_RENDERER
		Backend = "Skia mobile";
#else
		Backend = "UIKit native views";
#endif
#elif __DESKTOP__
		Platform = "Desktop";
		Backend = "Skia desktop";
#else
		Platform = "Unknown";
		Backend = "Unknown";
#endif

#if AOT_PROFILE_GEN
		ExecutionMode = "AOT profile generation";
#elif WASM_PROFILED_AOT
		ExecutionMode = "Interpreter + profile-guided AOT";
#elif WASM_INTERPRETER_AOT
		ExecutionMode = "Interpreter + AOT";
#elif WASM_INTERPRETER
		ExecutionMode = "Interpreter / Jiterpreter";
#elif NATIVE_AOT
		ExecutionMode = "Native AOT";
#else
		ExecutionMode = "Managed runtime";
#endif

		FeatureAvailability =
#if __WASM__ && !HAS_SKIA_RENDERER
			"Composition child visuals: unavailable on DOM; SKCanvasElement: unavailable on this renderer; drag/drop sample: available (pointer support is platform-dependent); " +
#elif HAS_SKIA_RENDERER && (__WASM__ || __DESKTOP__)
			"Composition: available; SKCanvasElement: available; drag/drop sample: available (pointer support is platform-dependent); " +
#else
			"Composition: available; SKCanvasElement: unavailable on this renderer; drag/drop sample: available (pointer support is platform-dependent); " +
#endif
#if WINDOWS
			"SystemBackdrop/AppWindow: Windows-only";
#else
			"SystemBackdrop/AppWindow: unavailable on this platform";
#endif

		// ---- Label -----------------------------------------------------------------
		// "with SHA"    -> v{semver} | {shortSha} | {renderer}
		// "without SHA" -> {semver} | local | {renderer}
		// Avoid a trailing empty renderer segment.
		string rendererSegment = string.IsNullOrEmpty(Renderer) ? string.Empty : " | " + Renderer;

#if VISUAL_REGRESSION
		Label = "visual-regression | Skia-WASM";
#else
		Label = string.IsNullOrEmpty(ShortSha)
			? SemVer + " | local" + rendererSegment
			: "v" + SemVer + " | " + ShortSha + rendererSegment;
#endif
	}

	private static bool IsHexChar(char c)
		=> (c >= '0' && c <= '9')
		|| (c >= 'a' && c <= 'f')
		|| (c >= 'A' && c <= 'F');

	/// <summary>AssemblyConfiguration value (e.g. "Release", "Debug").</summary>
	public static string Configuration { get; }

	/// <summary>Full SemVer without the commit metadata suffix.</summary>
	public static string SemVer    { get; }

	/// <summary>Up-to-40-character lowercase commit SHA, or empty when the metadata is absent or malformed.</summary>
	public static string CommitSha { get; }

	/// <summary>First 7 characters of <see cref="CommitSha"/>, or empty when fewer than 7 hex chars are available.</summary>
	public static string ShortSha  { get; }

	/// <summary>Runtime renderer resolved from AssemblyMetadataAttribute or compile-time fallback.</summary>
	public static string Renderer  { get; }

	/// <summary>Target framework recorded by the compiler.</summary>
	public static string TargetFramework { get; }

	/// <summary>Compile-time target platform.</summary>
	public static string Platform { get; }

	/// <summary>Renderer backend selected for this build.</summary>
	public static string Backend { get; }

	/// <summary>Compile-time execution mode for AOT/interpreter-aware targets.</summary>
	public static string ExecutionMode { get; }

	/// <summary>Compile-time availability summary for Wave C platform features.</summary>
	public static string FeatureAvailability { get; }

	/// <summary>Human-readable build identity label shown in the UI.</summary>
	public static string Label     { get; }
}

using System;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.ViewModels;
using Uno.Themes;
using Windows.UI;
using Command = Uno.Gallery.ViewModels.Command;

namespace Uno.Gallery.Views.Samples
{
	/// <summary>
	/// Material Seed Color Laboratory — demonstrates runtime primary seed color generation
	/// using <see cref="SemanticThemeHelper.PrimarySeed"/> (stable Uno.Themes 7.0.3 API).
	///
	/// Construction-time vs runtime behavior:
	/// • Color roles (PrimaryBrush etc.) are ThemeResources wired at startup.
	///   Changing the seed at runtime regenerates the full tonal palette and the
	///   ThemeResource brushes update automatically via the resource dictionary.
	/// • The original seed is captured on page navigation entry and restored on
	///   navigation exit so this sample never permanently mutates the app theme.
	/// </summary>
	[SamplePage(
		SampleCategory.Theming,
		"Material Seed Color",
		Description = "Runtime primary seed color laboratory: pick any color and M3 algorithmically derives the full tonal palette. Restored on leave.",
		DocumentationLink = "https://m3.material.io/styles/color/the-color-system/key-colors-tones",
		DataType = typeof(MaterialSeedColorSamplePageViewModel),
		Tags = new[] { "material", "color", "seed", "palette", "tokens" },
		Status = SampleStatus.Stable,
		SortOrder = 10,
		RelatedSamples = new[] { "design-tokens" })]
	public sealed partial class MaterialSeedColorSamplePage : Page
	{
		public MaterialSeedColorSamplePage()
		{
			this.InitializeComponent();
			// Wire once: Loaded/Unloaded always fire when NavigationView.Content swaps pages
			// (direct content assignment, not Frame navigation), so these are the correct hooks.
			Loaded += OnPageLoaded;
			Unloaded += OnPageUnloaded;
		}

		private void OnPageLoaded(object sender, RoutedEventArgs e)
		{
			if (DataContext is Sample { Data: MaterialSeedColorSamplePageViewModel vm })
			{
				vm.CaptureOriginalSeed();
			}
		}

		private void OnPageUnloaded(object sender, RoutedEventArgs e)
		{
			if (DataContext is Sample { Data: MaterialSeedColorSamplePageViewModel vm })
			{
				vm.RestoreOriginalSeed();
			}
		}
	}

	[Microsoft.UI.Xaml.Data.Bindable]
	public sealed class MaterialSeedColorSamplePageViewModel : ViewModelBase
	{
		// M3 baseline purple — vivid and recognisable as a demo seed when no app seed is set.
		private static readonly Color DefaultDisplaySeed = Color.FromArgb(0xFF, 0x65, 0x50, 0xF6);

		// ── Static multi-window state (UI thread only) ─────────────────────────
		// Captured once when the FIRST page instance opens; restored when the LAST closes.
		// Null when no page is currently open.
		private static Color? s_globalOriginalSeed;
		private static int s_activePageCount = 0;
		// Fired (on the UI thread) whenever s_activePageCount changes so every subscribed
		// instance can update its IsMultiWindowWarningVisible binding.
		private static event EventHandler? s_pageCountChanged;

		// ── Per-instance state ─────────────────────────────────────────────────
		private bool _isOwner = false;
		// Last value this instance wrote to SemanticThemeHelper. Tracked so that Reset()
		// sets _appliedSeed correctly and RestoreOriginalSeed can still be idempotent.
		private Color? _appliedSeed;

		public Color CurrentSeedColor
		{
			get => GetProperty<Color>();
			set
			{
				SetProperty(value);
				// Opaque seed only — alpha is not meaningful for M3 palette generation.
				var opaque = Color.FromArgb(0xFF, value.R, value.G, value.B);
				SemanticThemeHelper.PrimarySeed = opaque;
				_appliedSeed = opaque;
				CurrentSeedHex = FormatHex(opaque);
			}
		}

		/// <summary>
		/// Current seed color as an uppercase "#RRGGBB" hex string.
		/// Displayed in-page so users (and UITests) can observe the applied seed value.
		/// </summary>
		public string CurrentSeedHex
		{
			get => GetProperty<string>() ?? FormatHex(DefaultDisplaySeed);
			private set => SetProperty(value);
		}

		/// <summary>
		/// Two-way bound to the hex-entry TextBox; consumed by <see cref="ApplySeedHexCommand"/>.
		/// </summary>
		public string SeedHexInput
		{
			get => GetProperty<string>() ?? string.Empty;
			set => SetProperty(value);
		}

		/// <summary>
		/// True when more than one instance of this page is live across windows.
		/// Bound to a warning InfoBar so the user knows changes are global.
		/// </summary>
		public bool IsMultiWindowWarningVisible
		{
			get => GetProperty<bool>();
			private set => SetProperty(value);
		}

		public Command ResetCommand { get; }
		public Command ApplySeedHexCommand { get; }

		public MaterialSeedColorSamplePageViewModel()
		{
			ResetCommand = new Command(Reset);
			ApplySeedHexCommand = new Command(_ => ApplySeedHex());
			// Initialise the picker display without writing to the global seed.
			// CaptureOriginalSeed() (called from OnPageLoaded) is the authoritative
			// capture point; doing it here would corrupt the value we want to restore.
			SyncPickerDisplay(SemanticThemeHelper.PrimarySeed ?? DefaultDisplaySeed);
		}

		/// <summary>
		/// Captures the global original seed (once, on first open) and registers this
		/// instance as an active owner.  Called from <c>OnPageLoaded</c>.
		/// </summary>
		public void CaptureOriginalSeed()
		{
			if (!_isOwner)
			{
				if (s_activePageCount == 0)
				{
					// First page opening: record the true pre-visit app seed.
					s_globalOriginalSeed = SemanticThemeHelper.PrimarySeed;
				}
				s_activePageCount++;
				_isOwner = true;
				s_pageCountChanged += OnPageCountChanged;
			}

			// Reset write-tracking for this visit.
			_appliedSeed = null;

			// Sync picker display without triggering a global write.
			SyncPickerDisplay(SemanticThemeHelper.PrimarySeed ?? DefaultDisplaySeed);

			s_pageCountChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Restores the original app seed when the LAST active page closes.
		/// Idempotent — safe to call from <c>OnPageUnloaded</c>; the <c>_isOwner</c> guard
		/// makes every call after the first a no-op.
		/// </summary>
		public void RestoreOriginalSeed()
		{
			if (!_isOwner) return;

			_isOwner = false;
			s_pageCountChanged -= OnPageCountChanged;
			s_activePageCount--;

			if (s_activePageCount == 0)
			{
				// Last page closing: restore the original app seed captured on first open.
				SemanticThemeHelper.PrimarySeed = s_globalOriginalSeed;
				s_globalOriginalSeed = null;
			}
			// If other pages are still open they are managing the seed; leave it alone.

			s_pageCountChanged?.Invoke(this, EventArgs.Empty);
		}

		private void OnPageCountChanged(object? sender, EventArgs e)
		{
			IsMultiWindowWarningVisible = s_activePageCount > 1;
		}

		private void Reset()
		{
			// Restore to the original app seed (pre any-page-visit).
			var target = s_globalOriginalSeed;
			SemanticThemeHelper.PrimarySeed = target;
			_appliedSeed = target;
			SyncPickerDisplay(target ?? DefaultDisplaySeed);
		}

		private void ApplySeedHex()
		{
			var hex = (SeedHexInput ?? string.Empty).Trim().TrimStart('#').ToUpperInvariant();
			if (hex.Length != 6) return;
			try
			{
				var r = Convert.ToByte(hex.Substring(0, 2), 16);
				var g = Convert.ToByte(hex.Substring(2, 2), 16);
				var b = Convert.ToByte(hex.Substring(4, 2), 16);
				CurrentSeedColor = Color.FromArgb(0xFF, r, g, b);
			}
			catch { /* invalid hex — ignore */ }
		}

		/// <summary>
		/// Updates <see cref="CurrentSeedColor"/> and <see cref="CurrentSeedHex"/> together
		/// without writing to <see cref="SemanticThemeHelper.PrimarySeed"/>.
		/// </summary>
		private void SyncPickerDisplay(Color seed)
		{
			SetProperty(seed, nameof(CurrentSeedColor));
			CurrentSeedHex = FormatHex(seed);
		}

		private static string FormatHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
	}
}

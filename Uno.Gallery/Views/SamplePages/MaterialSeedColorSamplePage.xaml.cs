using System;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.ViewModels;
using Uno.Themes;
using Windows.UI;

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
		}

		protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (DataContext is Sample { Data: MaterialSeedColorSamplePageViewModel vm })
			{
				vm.CaptureOriginalSeed();
			}
			Unloaded += OnPageUnloaded;
		}

		protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
			Unloaded -= OnPageUnloaded;
			if (DataContext is Sample { Data: MaterialSeedColorSamplePageViewModel vm })
			{
				vm.RestoreOriginalSeed();
			}
		}

		private void OnPageUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			Unloaded -= OnPageUnloaded;
			// Fallback for force-close (window killed without navigation). RestoreOriginalSeed
			// is idempotent — the _isOwner guard makes a second call a no-op.
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
			}
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

		public MaterialSeedColorSamplePageViewModel()
		{
			ResetCommand = new Command(Reset);
			// Initialise the picker display without writing to the global seed.
			// CaptureOriginalSeed() (called from OnNavigatedTo) is the authoritative
			// capture point; doing it here would corrupt the value we want to restore.
			SetProperty(SemanticThemeHelper.PrimarySeed ?? DefaultDisplaySeed, nameof(CurrentSeedColor));
		}

		/// <summary>
		/// Captures the global original seed (once, on first open) and registers this
		/// instance as an active owner.  Called from <c>OnNavigatedTo</c>.
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
			SetProperty(SemanticThemeHelper.PrimarySeed ?? DefaultDisplaySeed, nameof(CurrentSeedColor));

			s_pageCountChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Restores the original app seed when the LAST active page closes.
		/// Idempotent — safe to call from both <c>OnNavigatedFrom</c> and <c>Unloaded</c>.
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
			SetProperty(target ?? DefaultDisplaySeed, nameof(CurrentSeedColor));
		}
	}
}


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
		}

		protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
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

		// Multi-window ownership tracking. All accesses are on the UI thread only.
		private static int s_activePageCount = 0;
		// Fired (on the UI thread) whenever s_activePageCount changes so every subscribed
		// instance can update its IsMultiWindowWarningVisible binding.
		private static event EventHandler? s_pageCountChanged;

		private bool _isOwner = false;
		private Color? _originalSeed;
		// Last value this instance wrote to SemanticThemeHelper. Used on navigation exit
		// to detect whether another window changed the seed after us; if so, we skip
		// the restore to avoid clobbering the other window's active seed.
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
		/// Captures the live global seed and registers this instance as an active owner.
		/// Called from <c>OnNavigatedTo</c> once per navigation visit.
		/// </summary>
		public void CaptureOriginalSeed()
		{
			if (!_isOwner)
			{
				s_activePageCount++;
				_isOwner = true;
				s_pageCountChanged += OnPageCountChanged;
			}

			// Reset write-tracking for this visit — no write has been made yet.
			_appliedSeed = null;
			// Capture the true pre-visit global (constructor never touches it).
			_originalSeed = SemanticThemeHelper.PrimarySeed;

			// Notify all subscribed instances (other open windows) of the new count.
			s_pageCountChanged?.Invoke(this, EventArgs.Empty);

			// Sync picker display without triggering a global write.
			SetProperty(_originalSeed ?? DefaultDisplaySeed, nameof(CurrentSeedColor));
		}

		/// <summary>
		/// Restores the seed that was active before this page was opened.
		/// Skips the write if another window changed the seed after us so that
		/// its seed is not silently overwritten.
		/// </summary>
		public void RestoreOriginalSeed()
		{
			if (!_isOwner) return;

			_isOwner = false;
			s_pageCountChanged -= OnPageCountChanged;
			s_activePageCount--;

			// Only restore when no other page has written a different seed since we did.
			// _appliedSeed == null means we never wrote, so current == original and restore is safe.
			var current = SemanticThemeHelper.PrimarySeed;
			if (_appliedSeed is null || current == _appliedSeed)
			{
				SemanticThemeHelper.PrimarySeed = _originalSeed;
			}

			s_pageCountChanged?.Invoke(this, EventArgs.Empty);
		}

		private void OnPageCountChanged(object? sender, EventArgs e)
		{
			IsMultiWindowWarningVisible = s_activePageCount > 1;
		}

		private void Reset()
		{
			// Restore the pre-navigation app seed (user-requested).
			SemanticThemeHelper.PrimarySeed = _originalSeed;
			// Track this write so RestoreOriginalSeed can detect later overwrites.
			_appliedSeed = _originalSeed;
			// Sync picker without re-triggering a global write.
			SetProperty(_originalSeed ?? DefaultDisplaySeed, nameof(CurrentSeedColor));
		}
	}
}


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
	/// • <c>BaseTheme.UseHighFidelityColors</c> is not publicly accessible from
	///   application code (protected member) — only <see cref="SemanticThemeHelper"/>
	///   static properties are used here.
	/// </summary>
	[SamplePage(
		SampleCategory.Theming,
		"Material Seed Color",
		Description = "Runtime primary seed color laboratory: pick any color and M3 algorithmically derives the full tonal palette. Restored on leave.",
		DocumentationLink = "https://m3.material.io/styles/color/the-color-system/key-colors-tones",
		DataType = typeof(MaterialSeedColorSamplePageViewModel),
		Tags = new[] { "material", "color", "seed", "palette", "tokens" },
		Status = SampleStatus.Stable,
		SortOrder = 10)]
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
		// M3 baseline purple, vivid and recognisable as a "default Material seed"
		private static readonly Color DefaultDisplaySeed = Color.FromArgb(0xFF, 0x65, 0x50, 0xF6);

		private Color? _originalSeed;

		public Color CurrentSeedColor
		{
			get => GetProperty<Color>();
			set
			{
				SetProperty(value);
				// Opaque seed only — alpha not meaningful for M3 palette generation
				SemanticThemeHelper.PrimarySeed = Color.FromArgb(0xFF, value.R, value.G, value.B);
			}
		}

		public Command ResetCommand { get; }

		public MaterialSeedColorSamplePageViewModel()
		{
			ResetCommand = new Command(Reset);
			// Initialise picker to current seed or baseline
			var current = SemanticThemeHelper.PrimarySeed;
			CurrentSeedColor = current ?? DefaultDisplaySeed;
		}

		/// <summary>Saves the current live seed before the lab modifies it.</summary>
		public void CaptureOriginalSeed()
		{
			_originalSeed = SemanticThemeHelper.PrimarySeed;
			// Sync picker without triggering a seed write back
			var current = _originalSeed ?? DefaultDisplaySeed;
			if (CurrentSeedColor != current)
			{
				// Set backing store directly to avoid double-setting the ThemeHelper
				SetProperty(current, nameof(CurrentSeedColor));
			}
		}

		/// <summary>Restores the seed that was active before this page was opened.</summary>
		public void RestoreOriginalSeed()
		{
			SemanticThemeHelper.PrimarySeed = _originalSeed;
		}

		private void Reset()
		{
			RestoreOriginalSeed();
			// Sync picker back without retriggering seed write
			var restored = _originalSeed ?? DefaultDisplaySeed;
			SetProperty(restored, nameof(CurrentSeedColor));
		}
	}
}


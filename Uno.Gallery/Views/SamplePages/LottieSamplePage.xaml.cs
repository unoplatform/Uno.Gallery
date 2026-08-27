using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIFeatures, "Lottie",
		Description = "Loads a packaged offline Lottie asset and exposes initialization status.",
		DocumentationLink = "https://platform.uno/docs/articles/features/lottie.html",
		Slug = "lottie",
		Tags = new[] { "animation", "lottie", "offline", "rendering" },
		Status = SampleStatus.Stable,
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Agnostic,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { "The Lottie JSON asset is bundled with the Gallery and requires no network access." },
		AccessibilityNotes = new[] { "Initialization state is exposed as text; the animation is decorative and conveys no unique information." },
		ResetBehavior = "Reload the sample to recreate the visual source and restart playback.",
		Variants = new[] { "Bundled Lottie source", "Automatic playback", "Renderer initialization status" },
		KnownLimitations = new[] { "Rendering fidelity and frame timing can vary by renderer and device performance." },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-27")]
	public sealed partial class LottieSamplePage : Page
	{
		public LottieSamplePage()
		{
			this.InitializeComponent();
		}

		private void LottiePlayer_Loaded(object sender, RoutedEventArgs e)
		{
			var player = (AnimatedVisualPlayer)sender;
			UpdateStatus(player.Source is null
				? "Lottie unavailable: the packaged visual source was not created."
				: "Packaged Lottie source initialized; playback uses the current renderer.");
		}

		private void UpdateStatus(string message)
		{
			if (LocalSamplePageLayout.GetSampleChild<TextBlock>(Design.Agnostic, "LottieStatus") is { } status)
			{
				AccessibilityHelper.Announce(status, message);
			}
		}
	}
}

using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIFeatures, "Composition Visuals",
	Description = "Creates a real compositor SpriteVisual and applies deterministic translation, opacity, and scale state.",
	DocumentationLink = "https://learn.microsoft.com/windows/apps/windows-app-sdk/composition",
	Slug = "composition-visuals",
	Tags = new[] { "rendering", "composition", "visual", "transform", "offline" },
	Status = SampleStatus.Stable,
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "skia-canvas", "diagnostics" })]
public sealed partial class CompositionVisualsSamplePage : Page
{
	private SpriteVisual? _visual;
	private int _stateChanges;

	public CompositionVisualsSamplePage()
	{
		InitializeComponent();
	}

	private void VisualHost_Loaded(object sender, RoutedEventArgs e)
	{
		var host = (UIElement)sender;
		var compositor = ElementCompositionPreview.GetElementVisual(host).Compositor;
		_visual = compositor.CreateSpriteVisual();
		_visual.Size = new Vector2(96, 96);
		_visual.Offset = new Vector3(20, 32, 0);
		_visual.Brush = compositor.CreateColorBrush(Microsoft.UI.Colors.DodgerBlue);
		ElementCompositionPreview.SetElementChildVisual(host, _visual);
		UpdateStatus("SpriteVisual initialized; offset: 20,32; opacity: 1.00; scale: 1.00.");
	}

	private void ApplyVisualState_Click(object sender, RoutedEventArgs e)
	{
		if (_visual is null)
		{
			UpdateStatus("Composition visual is not initialized.");
			return;
		}

		_stateChanges++;
		var alternate = _stateChanges % 2 == 1;
		_visual.Offset = new Vector3(alternate ? 116 : 20, 32, 0);
		_visual.Opacity = alternate ? 0.55f : 1f;
		_visual.Scale = alternate ? new Vector3(0.8f, 0.8f, 1f) : Vector3.One;
		UpdateStatus(
			$"State {_stateChanges}; offset: {(alternate ? "116,32" : "20,32")}; " +
			$"opacity: {(alternate ? "0.55" : "1.00")}; scale: {(alternate ? "0.80" : "1.00")}.");
	}

	private void UpdateStatus(string message)
	{
		if (SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "Composition_Status") is { } status)
		{
			AccessibilityHelper.Announce(status, message);
		}
	}
}

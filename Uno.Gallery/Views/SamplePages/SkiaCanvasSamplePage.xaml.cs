using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Helpers;

#if HAS_SKIA_RENDERER && (__WASM__ || __DESKTOP__)
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;
#endif

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIFeatures, "Skia Canvas",
	Description = "Draws deterministic shapes and text through Uno.WinUI.Graphics2DSK.SKCanvasElement and exposes completed render state.",
	DocumentationLink = "https://aka.platform.uno/skcanvaselement",
	Slug = "skia-canvas",
	Tags = new[] { "rendering", "skia", "canvas", "graphics", "offline" },
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Skia,
	Requirements = new[] { "Requires the Skia desktop or Skia WebAssembly renderer; all drawing is local and deterministic." },
	AccessibilityNotes = new[] { "Redraw is keyboard-focusable, the canvas has an accessible name, and completed render state is announced as text." },
	ResetBehavior = "Reload the sample to restore drawing state zero and reset the completed-render count.",
	Variants = new[] { "Initial blue drawing", "Alternate purple drawing", "RenderOverride completion status" },
	KnownLimitations = new[] { "Automation verifies page-owned render completion and state rather than comparing canvas pixels." },
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "composition-visuals", "diagnostics" })]
[SampleConditional(SampleConditionals.SkiaBased | SampleConditionals.SkiaRenderer,
	Reason = "SKCanvasElement requires the Skia desktop or Skia WebAssembly renderer; native renderer targets are excluded.")]
public sealed partial class SkiaCanvasSamplePage : Page
{
	private int _requestedState;
	private int _completedRenders;
#if HAS_SKIA_RENDERER && (__WASM__ || __DESKTOP__)
	private GalleryCanvas? _canvas;
#endif

	public SkiaCanvasSamplePage()
	{
		InitializeComponent();
	}

	private void CanvasHost_Loaded(object sender, RoutedEventArgs e)
	{
#if HAS_SKIA_RENDERER && (__WASM__ || __DESKTOP__)
		if (!SKCanvasElement.IsSupportedOnCurrentPlatform())
		{
			UpdateStatus("SKCanvasElement is unavailable in this Skia host.");
			return;
		}

		_canvas = new GalleryCanvas(OnRenderCompleted);
		((Border)sender).Child = _canvas;
#else
		UpdateStatus("SKCanvasElement is unavailable on this renderer.");
#endif
	}

	private void Redraw_Click(object sender, RoutedEventArgs e)
	{
#if HAS_SKIA_RENDERER && (__WASM__ || __DESKTOP__)
		if (_canvas is null)
		{
			UpdateStatus("SKCanvasElement is not initialized.");
			return;
		}

		_requestedState++;
		_canvas.DrawingState = _requestedState;
		_canvas.Invalidate();
		UpdateStatus($"Redraw requested for state {_requestedState}; waiting for RenderOverride.");
#else
		UpdateStatus("SKCanvasElement is unavailable on this renderer.");
#endif
	}

	private void OnRenderCompleted(int state)
	{
		_ = DispatcherQueue.TryEnqueue(() =>
		{
			_completedRenders++;
			UpdateStatus($"RenderOverride completed: {_completedRenders}; state: {state}; shapes: rectangle, circle, text.");
		});
	}

	private void UpdateStatus(string message)
	{
		if (SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "SkiaCanvas_Status") is { } status)
		{
			AccessibilityHelper.Announce(status, message);
		}
	}

#if HAS_SKIA_RENDERER && (__WASM__ || __DESKTOP__)
	private sealed class GalleryCanvas : SKCanvasElement
	{
		private readonly Action<int> _rendered;
		private int _lastReportedState = -1;

		public GalleryCanvas(Action<int> rendered)
		{
			_rendered = rendered;
		}

		public int DrawingState { get; set; }

		protected override void RenderOverride(SKCanvas canvas, Size area)
		{
			canvas.Clear(new SKColor(248, 250, 252));
			var accent = DrawingState % 2 == 0
				? new SKColor(0, 99, 177)
				: new SKColor(146, 76, 157);

			using var fill = new SKPaint { Color = accent, IsAntialias = true, Style = SKPaintStyle.Fill };
			using var outline = new SKPaint
			{
				Color = new SKColor(25, 35, 45),
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 4
			};
			using var text = new SKPaint
			{
				Color = new SKColor(25, 35, 45),
				IsAntialias = true,
				TextSize = 24
			};

			var width = Math.Max(180f, (float)area.Width);
			canvas.DrawRoundRect(new SKRect(24, 28, width - 24, 132), 18, 18, fill);
			canvas.DrawCircle(width - 68, 80, 30, outline);
			canvas.DrawText($"Uno + Skia - state {DrawingState}", 28, 190, text);
			if (_lastReportedState != DrawingState)
			{
				_lastReportedState = DrawingState;
				_rendered(DrawingState);
			}
		}
	}
#endif
}

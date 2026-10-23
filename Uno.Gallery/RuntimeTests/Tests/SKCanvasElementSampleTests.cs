#if HAS_UNO_SKIA
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using Uno.Gallery.Helpers;
using Uno.Gallery.Helpers.Canvas;
using Uno.Gallery.Views.SamplePages;
using Uno.UI.RuntimeTests;
using Windows.Foundation;

namespace Uno.Gallery.RuntimeTests.Tests;

[TestClass]
public class SKCanvasElementSampleTests
{
	[TestMethod]
	[RunsOnUIThread]
	public async Task When_Reloaded_Keeps_A_Single_Canvas()
	{
		var page = new SKCanvasElementPage();

		// The first load, then two reloads: taking the page out of the tree and back raises Loaded again, like navigating away and back
		for (var reloads = 0; reloads < 3; reloads++)
		{
			UnitTestsUIContentHelper.Content = page;
			await UnitTestsUIContentHelper.WaitForLoaded(page);
			await UnitTestsUIContentHelper.WaitForIdle();

			var canvases = VisualTreeHelperEx.GetDescendants(page).OfType<SampleSKCanvasElement>().Count();
			Assert.AreEqual(1, canvases, $"Canvases in the page after {reloads} reload(s)");

			UnitTestsUIContentHelper.Content = new Border();
			await TestHelper.WaitFor(() => !page.IsLoaded);
		}
	}

	[TestMethod]
	[RunsOnUIThread]
	public void When_Rendered_Each_Sample_Draws()
	{
		for (var sample = 0; sample < SampleSKCanvasElement.SampleCount; sample++)
		{
			var canvasElement = new RenderableCanvasElement { Sample = sample };
			using var surface = SKSurface.Create(new SKImageInfo(260, 260));
			surface.Canvas.Clear(SKColors.Transparent);
			canvasElement.Render(surface.Canvas, new Size(260, 260));

			using var image = surface.Snapshot();
			using var pixmap = image.PeekPixels();
			var drawnPixels = 0;
			for (var y = 0; y < pixmap.Height; y++)
			{
				for (var x = 0; x < pixmap.Width; x++)
				{
					if (pixmap.GetPixelColor(x, y).Alpha != 0)
					{
						drawnPixels++;
					}
				}
			}

			Assert.IsTrue(drawnPixels > 1000, $"Sample {sample} drew only {drawnPixels} pixels");
		}
	}

	// Renders a sample on any canvas, without a window
	private sealed class RenderableCanvasElement : SampleSKCanvasElement
	{
		public void Render(SKCanvas canvas, Size area) => RenderOverride(canvas, area);
	}
}
#endif

#if HAS_UNO_SKIA
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Gallery.Helpers;
using Uno.Gallery.Helpers.Canvas;
using Uno.Gallery.Views.SamplePages;
using Uno.UI.RuntimeTests;

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
}
#endif

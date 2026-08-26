using System;
using NUnit.Framework;
using Uno.UITest;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_ItemsRepeater : TestBase
{
	[Test]
	public void When_ItemsRepeater_Loads()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		TakeScreenshot("Loaded");
		App.WaitForElement("ItemsRepeater_Stack");
		App.WaitForElement("ItemsRepeater_Grid");
	}

	[Test]
	public void When_ItemsRepeater_Stack_CanScroll()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		App.WaitForElement("ItemsRepeater_Stack_ScrollDown");

		App.WaitThenTap("ItemsRepeater_Stack_ScrollDown");
		TakeScreenshot("After scroll button");
		App.Wait(TimeSpan.FromMilliseconds(400)); // allow disableAnimation ChangeView to settle

		var sv = new QueryEx(x => x.All().Marked("ItemsRepeater_Stack_ScrollViewer"));
		var offsetAfter = sv.GetDependencyPropertyValue<double>("VerticalOffset");
		Assert.Greater(offsetAfter, 0.0, "Scrolling down must produce a positive VerticalOffset");
	}

	[Test]
	public void When_ItemsRepeater_Grid_IsVisible()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		App.ScrollDownTo("ItemsRepeater_Grid");
		App.WaitForElement("ItemsRepeater_Grid_The Great Gatsby");
		TakeScreenshot("Grid in view");

		var realizedTiles = App.Query(x => x.All().Marked("ItemsRepeater_Grid_The Great Gatsby"));
		Assert.AreEqual(1, realizedTiles.Length, "The first grid tile must be realized exactly once.");
		Assert.Greater(realizedTiles[0].Rect.Width, 0, "The realized grid tile must have a rendered width.");
		Assert.Greater(realizedTiles[0].Rect.Height, 0, "The realized grid tile must have a rendered height.");
	}
}

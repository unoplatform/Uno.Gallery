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
		App.WaitForElement("ItemsRepeater_Grid");
		// Scroll the outer page to bring the grid section into the viewport before measuring.
		App.ScrollDownTo("ItemsRepeater_Grid");
		TakeScreenshot("Grid in view");
		var repeater = new QueryEx(x => x.All().Marked("ItemsRepeater_Grid"));
		Assert.IsTrue(repeater.GetDependencyPropertyValue<double>("ActualWidth") > 0);
	}
}

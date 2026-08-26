using NUnit.Framework;
using Uno.UITest;
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
		App.WaitForElement("ItemsRepeater_Stack_ScrollViewer");

		var sv = new QueryEx(x => x.All().Marked("ItemsRepeater_Stack_ScrollViewer"));
		var offsetBefore = sv.GetDependencyPropertyValue<double>("VerticalOffset");

		// Scroll the Stack ScrollViewer directly (not a sibling element).
		App.ScrollDown("ItemsRepeater_Stack_ScrollViewer", ScrollStrategy.Gesture);
		TakeScreenshot("After scroll");

		var offsetAfter = sv.GetDependencyPropertyValue<double>("VerticalOffset");
		Assert.Greater(offsetAfter, offsetBefore, "Scrolling down must increase VerticalOffset");
	}

	[Test]
	public void When_ItemsRepeater_Grid_IsVisible()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		App.WaitForElement("ItemsRepeater_Grid");
		TakeScreenshot("Grid visible");
		var repeater = new QueryEx(x => x.All().Marked("ItemsRepeater_Grid"));
		Assert.IsTrue(repeater.GetDependencyPropertyValue<double>("ActualWidth") > 0);
	}
}

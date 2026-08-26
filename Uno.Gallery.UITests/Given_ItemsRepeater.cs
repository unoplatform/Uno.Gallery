using NUnit.Framework;
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
		App.ScrollDownTo("ItemsRepeater_Grid_ScrollViewer", withinMarked: "ItemsRepeater_Stack_ScrollViewer");
		TakeScreenshot("Scrolled");
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

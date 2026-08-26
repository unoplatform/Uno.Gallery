using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_ItemsView : TestBase
{
	[Test]
	public void When_ItemsView_Loads()
	{
		NavigateToSample("ItemsView", "Fluent");
		TakeScreenshot("Loaded");
		App.WaitForElement("ItemsView_Single");
		App.WaitForElement("ItemsView_Multiple");
	}

	[Test]
	public void When_ItemsView_Single_SelectionUpdatesDisplay()
	{
		NavigateToSample("ItemsView", "Fluent");
		App.WaitForElement("ItemsView_SelectFirst");

		App.WaitThenTap("ItemsView_SelectFirst");
		TakeScreenshot("After select first");

		var display = new QueryEx(x => x.All().Marked("ItemsView_SelectedTitle"));
		var text = display.GetDependencyPropertyValue<string>("Text");
		StringAssert.Contains("The Great Gatsby", text,
			$"Selection display must contain 'The Great Gatsby'; actual: '{text}'");
	}

	[Test]
	public void When_ItemsView_Multiple_SelectsItems()
	{
		NavigateToSample("ItemsView", "Fluent");
		App.WaitForElement("ItemsView_SelectFirstTwo");

		App.WaitThenTap("ItemsView_SelectFirstTwo");
		TakeScreenshot("After select first two");

		var countDisplay = new QueryEx(x => x.All().Marked("ItemsView_MultiSelectionCount"));
		var text = countDisplay.GetDependencyPropertyValue<string>("Text");
		StringAssert.Contains("2", text, $"Count display should show 2 selected items; actual: '{text}'");
	}
}

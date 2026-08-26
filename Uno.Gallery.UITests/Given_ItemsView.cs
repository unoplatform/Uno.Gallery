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
		App.WaitForElement("ItemsView_Single");

		// Tap first item (The Great Gatsby)
		App.WaitThenTap("The Great Gatsby");
		TakeScreenshot("After first tap");

		var display = new QueryEx(x => x.All().Marked("ItemsView_SelectedTitle"));
		var text = display.GetDependencyPropertyValue<string>("Text");
		StringAssert.Contains("The Great Gatsby", text);
	}

	[Test]
	public void When_ItemsView_Multiple_SelectsItems()
	{
		NavigateToSample("ItemsView", "Fluent");
		App.WaitForElement("ItemsView_Multiple");

		// Tap two items (prefixed to avoid ambiguity with the Single-section items)
		App.WaitThenTap("ItemsView_Multiple_The Great Gatsby");
		App.WaitThenTap("ItemsView_Multiple_1984");
		TakeScreenshot("After two selections");

		var countDisplay = new QueryEx(x => x.All().Marked("ItemsView_MultiSelectionCount"));
		var text = countDisplay.GetDependencyPropertyValue<string>("Text");
		StringAssert.Contains("2", text, $"Count display should show 2 selected items; actual: '{text}'");
	}
}

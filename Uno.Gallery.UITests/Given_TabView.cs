using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_TabView : TestBase
{
	[Test]
	public void When_TabView_Loads()
	{
		NavigateToSample("TabView", "Fluent");
		TakeScreenshot("Loaded");
		App.WaitForElement("TabView_Basic");
		App.WaitForElement("TabView_Dynamic");
	}

	[Test]
	public void When_TabView_Basic_TabsAreSelectable()
	{
		NavigateToSample("TabView", "Fluent");
		App.WaitForElement("TabView_Tab_Home");
		App.WaitThenTap("TabView_Tab_Documents");
		TakeScreenshot("After tap Documents");

		var tabView = new QueryEx(x => x.All().Marked("TabView_Basic"));
		Assert.AreEqual(1, tabView.GetDependencyPropertyValue<int>("SelectedIndex"));
	}

	[Test]
	public void When_TabView_Dynamic_AddTab()
	{
		NavigateToSample("TabView", "Fluent");
		App.WaitForElement("TabView_AddTabButton");

		var countDisplay = new QueryEx(x => x.All().Marked("TabView_Dynamic_TabCount"));
		var countBefore = int.Parse(countDisplay.GetDependencyPropertyValue<string>("Text"));

		App.WaitThenTap("TabView_AddTabButton");
		TakeScreenshot("After add tab");

		var countAfter = int.Parse(countDisplay.GetDependencyPropertyValue<string>("Text"));
		Assert.AreEqual(countBefore + 1, countAfter, "Adding a tab must increment tab count");
		App.WaitForElement("TabView_Dynamic_Tab4");
	}

	[Test]
	public void When_TabView_Dynamic_CloseTab()
	{
		NavigateToSample("TabView", "Fluent");
		App.WaitForElement("TabView_Dynamic_Tab2_Close");

		var countDisplay = new QueryEx(x => x.All().Marked("TabView_Dynamic_TabCount"));
		var countBefore = int.Parse(countDisplay.GetDependencyPropertyValue<string>("Text"));

		App.WaitThenTap("TabView_Dynamic_Tab2_Close");
		TakeScreenshot("After close tab");

		var countAfter = int.Parse(countDisplay.GetDependencyPropertyValue<string>("Text"));
		Assert.AreEqual(countBefore - 1, countAfter, "Closing a tab must decrement tab count");
		App.WaitForNoElement("TabView_Dynamic_Tab2");
	}
}

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
		var tabView = new QueryEx(x => x.All().Marked("TabView_Dynamic"));
		var initialCount = tabView.GetDependencyPropertyValue<int>("TabItems.Count");

		// Tap the add tab (+) button
		App.WaitThenTap("Add tab button");
		TakeScreenshot("After add tab");
	}
}

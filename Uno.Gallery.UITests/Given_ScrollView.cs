using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_ScrollView : TestBase
{
	[Test]
	public void When_ScrollView_Loads()
	{
		NavigateToSample("ScrollView", "Fluent");
		TakeScreenshot("Loaded");
		App.WaitForElement("ScrollView_Vertical");
		App.WaitForElement("ScrollView_Horizontal");
	}

	[Test]
	public void When_ScrollView_Vertical_CanScroll()
	{
		NavigateToSample("ScrollView", "Fluent");
		App.WaitForElement("ScrollView_Vertical");
		App.ScrollDown("ScrollView_Vertical", 200);
		TakeScreenshot("After scroll down");
	}

	[Test]
	public void When_ScrollView_ReadOffset_Button_Works()
	{
		NavigateToSample("ScrollView", "Fluent");
		App.WaitForElement("ScrollView_ReadOffset");
		App.WaitThenTap("ScrollView_ReadOffset");
		TakeScreenshot("After read offset");
		App.WaitForElement("ScrollView_OffsetDisplay");
	}
}

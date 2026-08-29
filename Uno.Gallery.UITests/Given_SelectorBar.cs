using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_SelectorBar : TestBase
	{
		[Test]
		public void WhenSelectorBarPage_Loads()
		{
			NavigateToSample("SelectorBar", "Fluent");
			TakeScreenshot("PageLoaded");

			App.WaitForElement("SelectorBar_Default");
			App.WaitForElement("SelectorBar_Icons");
		}

		[Test]
		public void WhenSelectorBarItem_Selected_UpdatesResult()
		{
			NavigateToSample("SelectorBar", "Fluent");
			App.WaitForElement("SelectorBar_Default");

			TakeScreenshot("Before selection");
			// Use the programmatic button to select Unread deterministically on all platforms.
			App.WaitThenTap("SelectorBar_SelectUnread");
			TakeScreenshot("After Unread selected");

			var result = new QueryEx(q => q.All().Marked("SelectorBar_Result"));
			StringAssert.Contains("Unread", result.GetDependencyPropertyValue<string>("Text"));
		}
	}
}

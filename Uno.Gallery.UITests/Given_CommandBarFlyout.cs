using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_CommandBarFlyout : TestBase
	{
		[Test]
		public void WhenCommandBarFlyoutPage_Loads()
		{
			NavigateToSample("CommandBarFlyout", "Fluent");
			TakeScreenshot("PageLoaded");

			App.WaitForElement("CommandBarFlyout_Trigger");
			App.WaitForElement("CommandBarFlyout_AlwaysExpanded_Trigger");
			App.WaitForElement("CommandBarFlyout_TextBox");
		}

		[Test]
		public void WhenCommandBarFlyoutTriggerTapped_ShowsFlyout()
		{
			NavigateToSample("CommandBarFlyout", "Fluent");
			App.WaitForElement("CommandBarFlyout_Trigger");

			TakeScreenshot("Before flyout");
			App.Tap("CommandBarFlyout_Trigger");
			TakeScreenshot("After flyout opened");

			App.WaitForElement("CommandBarFlyout_Share");
		}
	}
}

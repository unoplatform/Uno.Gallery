using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_SplitButton : TestBase
	{
		[Test]
		public void WhenSplitButtonPage_Loads()
		{
			NavigateToSample("SplitButton", "Fluent");
			TakeScreenshot("PageLoaded");

			App.WaitForElement("SplitButton_Default");
			App.WaitForElement("ToggleSplitButton_Default");
		}

		[Test]
		public void WhenSplitButtonPrimaryClick_UpdatesResult()
		{
			NavigateToSample("SplitButton", "Fluent");
			App.WaitForElement("SplitButton_Default");

			TakeScreenshot("Before click");
			// Tap the primary content area (left side) to avoid the dropdown chevron
			App.Tap("SplitButton_PrimaryContent");
			TakeScreenshot("After primary click");

			var result = new QueryEx(q => q.All().Marked("SplitButton_Result"));
			StringAssert.Contains("New document", result.GetDependencyPropertyValue<string>("Text"));
		}

		[Test]
		public void WhenToggleSplitButtonChecked_UpdatesState()
		{
			NavigateToSample("SplitButton", "Fluent");
			App.WaitForElement("ToggleSplitButton_Default");

			TakeScreenshot("Before toggle");
			// Tap the primary content area (left side) to avoid the dropdown chevron
			App.Tap("ToggleSplitButton_PrimaryContent");
			TakeScreenshot("After toggle On");

			var result = new QueryEx(q => q.All().Marked("ToggleSplitButton_Result"));
			StringAssert.Contains("On", result.GetDependencyPropertyValue<string>("Text"));

			App.Tap("ToggleSplitButton_PrimaryContent");
			TakeScreenshot("After toggle Off");
			StringAssert.Contains("Off", result.GetDependencyPropertyValue<string>("Text"));
		}
	}
}

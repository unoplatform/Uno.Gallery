using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_DropDownButton : TestBase
	{
		[Test]
		public void WhenDropDownButtonPage_Loads()
		{
			NavigateToSample("DropDownButton", "Fluent");
			TakeScreenshot("PageLoaded");

			App.WaitForElement("DropDownButton_Default");
			App.WaitForElement("DropDownButton_Disabled");
		}

		[Test]
		public void WhenDropDownButtonDisabled_FlyoutDoesNotOpen()
		{
			NavigateToSample("DropDownButton", "Fluent");
			App.WaitForElement("DropDownButton_Disabled");

			TakeScreenshot("Disabled state");
			var disabledBtn = new QueryEx(q => q.All().Marked("DropDownButton_Disabled"));
			Assert.IsFalse(disabledBtn.GetDependencyPropertyValue<bool>("IsEnabled"));

			// Tap the disabled button — flyout must not open
			App.Tap("DropDownButton_Disabled");
			TakeScreenshot("After tapping disabled button");
			var flyoutItems = App.Query(q => q.All().Marked("DropDownButton_Disabled_FlyoutItem_PDF"));
			Assert.That(flyoutItems, Is.Empty, "Disabled DropDownButton flyout must not open");
		}
	}
}

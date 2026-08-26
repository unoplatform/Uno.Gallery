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
		public void WhenDropDownButtonDisabled_CannotInteract()
		{
			NavigateToSample("DropDownButton", "Fluent");
			App.WaitForElement("DropDownButton_Disabled");

			TakeScreenshot("Disabled state");
			var disabledBtn = new QueryEx(q => q.All().Marked("DropDownButton_Disabled"));
			Assert.IsFalse(disabledBtn.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
	}
}

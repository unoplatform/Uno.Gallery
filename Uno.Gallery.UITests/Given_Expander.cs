using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_Expander : TestBase
	{
		[Test]
		public void WhenExpanderPage_Loads()
		{
			NavigateToSample("Expander", "Fluent");
			TakeScreenshot("PageLoaded");

			App.WaitForElement("Expander_Default");
			App.WaitForElement("Expander_Collapsed");
			App.WaitForElement("Expander_Disabled");
		}

		[Test]
		public void WhenExpanderToggled_ChangesExpandedState()
		{
			NavigateToSample("Expander", "Fluent");
			App.WaitForElement("Expander_Default");

			// Verify the default expander starts expanded
			var expander = new QueryEx(q => q.All().Marked("Expander_Default"));
			TakeScreenshot("IsExpanded = True");
			Assert.IsTrue(expander.GetDependencyPropertyValue<bool>("IsExpanded"));

			// Collapse it
			App.Tap("Expander_Default");
			TakeScreenshot("After collapse");
			Assert.IsFalse(expander.GetDependencyPropertyValue<bool>("IsExpanded"));

			// Re-expand
			App.Tap("Expander_Default");
			TakeScreenshot("After re-expand");
			Assert.IsTrue(expander.GetDependencyPropertyValue<bool>("IsExpanded"));
		}

		[Test]
		public void WhenExpanderDisabled_CannotToggle()
		{
			NavigateToSample("Expander", "Fluent");
			App.ScrollDownTo("Expander_Disabled");
			App.WaitForElement("Expander_Disabled");

			var disabledExpander = new QueryEx(q => q.All().Marked("Expander_Disabled"));
			TakeScreenshot("Disabled expander");
			Assert.IsFalse(disabledExpander.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
	}
}

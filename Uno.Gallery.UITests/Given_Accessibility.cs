using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_Accessibility : TestBase
{
	[Test]
	public void When_Accessibility_Page_Loads()
	{
		NavigateToSample("Accessibility");
		App.WaitForElement("Accessibility_Save");
		App.WaitForElement("Accessibility_RendererStatus");

		var renderer = new QueryEx(query => query.All().Marked("Accessibility_RendererStatus"));
		StringAssert.Contains(
			"Consult the accessibility renderer matrix",
			renderer.GetDependencyPropertyValue<string>("Text"));
	}

	[Test]
	public void When_Save_Is_Invoked_Live_Status_Changes()
	{
		NavigateToSample("Accessibility");
		var status = new QueryEx(query => query.All().Marked("Accessibility_Announcement"));
		var before = status.GetDependencyPropertyValue<string>("Text");
		App.WaitThenTap("Accessibility_Save");

		var after = status.GetDependencyPropertyValue<string>("Text");
		Assert.AreNotEqual(before, after);
		StringAssert.Contains("1", after);
	}

	[Test]
	public void When_Reduced_Motion_Is_Enabled_Update_Is_Immediate()
	{
		NavigateToSample("Accessibility");
		var status = new QueryEx(query => query.All().Marked("Accessibility_MotionStatus"));
		var before = status.GetDependencyPropertyValue<string>("Text");
		App.WaitThenTap("Accessibility_ReduceMotion");
		App.WaitThenTap("Accessibility_MoveTarget");

		var after = status.GetDependencyPropertyValue<string>("Text");
		Assert.AreNotEqual(before, after);
		StringAssert.Contains("120", after);
	}
}

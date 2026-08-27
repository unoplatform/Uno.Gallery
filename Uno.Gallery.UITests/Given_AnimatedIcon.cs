using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_AnimatedIcon : TestBase
{
	[Test]
	public void When_Page_Loads_SourceAndFallbackAreConfigured()
	{
		NavigateToSample("AnimatedIcon");

		App.WaitForElement("AnimatedIcon_Accept");
		App.WaitThenTap("AnimatedIcon_ResetButton");
		var icon = new QueryEx(q => q.All().Marked("AnimatedIcon_Accept"));
		Assert.IsNotNull(icon.GetDependencyPropertyValue("Source"));
		Assert.IsNotNull(icon.GetDependencyPropertyValue("FallbackIconSource"));
		Assert.AreEqual("State: NormalOff; transitions: 0", GetStatus());
	}

	[Test]
	public void When_AcceptAndReset_AreInvoked_StateAndCountChange()
	{
		NavigateToSample("AnimatedIcon");

		App.WaitThenTap("AnimatedIcon_ResetButton");
		App.WaitThenTap("AnimatedIcon_AcceptButton");
		Assert.AreEqual("State: NormalOn; transitions: 1", GetStatus());

		App.WaitThenTap("AnimatedIcon_AcceptButton");
		Assert.AreEqual("State: NormalOn; transitions: 1", GetStatus());

		App.WaitThenTap("AnimatedIcon_ResetButton");
		Assert.AreEqual("State: NormalOff; transitions: 0", GetStatus());

		App.WaitThenTap("AnimatedIcon_ResetButton");
		Assert.AreEqual("State: NormalOff; transitions: 0", GetStatus());
	}

	private static string GetStatus()
		=> new QueryEx(q => q.All().Marked("AnimatedIcon_Status"))
			.GetDependencyPropertyValue<string>("Text");
}

using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_Localization : TestBase
{
	[Test]
	public void When_Rtl_Is_Toggled_State_Is_Preserved()
	{
		NavigateToSample("Localization and RTL");
		App.WaitForElement("Localization_StatefulInput");
		var direction = new QueryEx(query => query.All().Marked("Localization_DirectionStatus"));
		var initialDirection = direction.GetDependencyPropertyValue<string>("Text");
		var input = new QueryEx(query => query.All().Marked("Localization_StatefulInput"));
		var initialInput = input.GetDependencyPropertyValue<string>("Text");
		App.WaitThenTap("Localization_ToggleRtl");

		Assert.AreNotEqual(initialDirection, direction.GetDependencyPropertyValue<string>("Text"));
		Assert.AreEqual(initialInput, input.GetDependencyPropertyValue<string>("Text"));
	}

	[Test]
	public void When_Pseudo_Preview_Is_Enabled_Text_Expands()
	{
		NavigateToSample("Localization and RTL");
		var original = new QueryEx(query => query.All().Marked("Localization_ResolvedResource"))
			.GetDependencyPropertyValue<string>("Text");
		var previewQuery = new QueryEx(query => query.All().Marked("Localization_PseudoPreview"));
		var offText = previewQuery.GetDependencyPropertyValue<string>("Text");

		App.WaitThenTap("Localization_TogglePseudo");

		var pseudo = previewQuery.GetDependencyPropertyValue<string>("Text");
		StringAssert.StartsWith("[!! ", pseudo);
		Assert.AreNotEqual(offText, pseudo);
		Assert.GreaterOrEqual(pseudo.Length, original.Length);
	}

	[Test]
	public void When_Localization_Build_Mode_Is_Declared_It_Matches_The_Shell()
	{
		App.WaitForElement("BuildIdentityLabel");
		App.WaitForElement("SamplesSearchBox");
		App.WaitForElement("AppShell");

		var buildIdentity = new QueryEx(query => query.All().Marked("BuildIdentityLabel"));
		var mode = buildIdentity.GetDependencyPropertyValue<string>("Tag");
		var search = new QueryEx(query => query.All().Marked("SamplesSearchBox"));
		var placeholder = search.GetDependencyPropertyValue<string>("PlaceholderText");
		var shell = new QueryEx(query => query.All().Marked("AppShell"));
		var direction = shell.GetDependencyPropertyValue<string>("FlowDirection");

		if (mode.Contains("Pseudo=True"))
			StringAssert.StartsWith("[!! ", placeholder);
		else
			Assert.AreEqual("Search", placeholder);

		Assert.AreEqual(
			mode.Contains("Rtl=True") ? "RightToLeft" : "LeftToRight",
			direction);
	}
}

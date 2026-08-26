using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_ToolkitExtensions : TestBase
{
	[Test]
	public void When_PageLoads_RealExtensionTargetsArePresent()
	{
		NavigateToSample("Toolkit Extensions");

		App.WaitForElement("ToolkitExtensions_InputNext");
		App.WaitForElement("ToolkitExtensions_StateButton");
		App.WaitForElement("ToolkitExtensions_FlipView");
	}

	[Test]
	public void When_InputSettingsAreVerified_AttachedValuesAreReported()
	{
		NavigateToSample("Toolkit Extensions");

		App.WaitThenTap("ToolkitExtensions_VerifyInput");
		Assert.AreEqual(
			"First: Next, focus next: True; Submit: Done, dismiss: True",
			GetText("ToolkitExtensions_InputStatus"));
	}

	[Test]
	public void When_EnterIsPressed_InputExtensionMovesFocusToConfiguredElement()
	{
		NavigateToSample("Toolkit Extensions");

		App.Tap("ToolkitExtensions_InputNext");
		App.PressEnter();

		var submit = new QueryEx(q => q.All().Marked("ToolkitExtensions_InputSubmit"));
		Assert.AreNotEqual(
			"Unfocused",
			submit.GetDependencyPropertyValue("FocusState")?.ToString(),
			"AutoFocusNextElement must move focus to the configured submit field.");
	}

	[Test]
	public void When_SubmitFieldPressesEnter_CommandReceivesText()
	{
		NavigateToSample("Toolkit Extensions");

		App.EnterText("ToolkitExtensions_InputSubmit", "offline value");
		App.PressEnter();

		Assert.AreEqual("Submitted: offline value", GetText("ToolkitExtensions_SubmitStatus"));
	}

	[Test]
	public void When_CommandSourceChanges_CommandReceivesControlValue()
	{
		NavigateToSample("Toolkit Extensions");

		App.WaitThenTap("ToolkitExtensions_ToggleProgrammatically");
		Assert.AreEqual("Toggle command value: True", GetText("ToolkitExtensions_CommandStatus"));
	}

	[Test]
	public void When_ResourcesAndStateAreInvoked_RealAttachedValuesChange()
	{
		NavigateToSample("Toolkit Extensions");

		App.ScrollDownTo("ToolkitExtensions_VerifyResources");
		App.WaitThenTap("ToolkitExtensions_VerifyResources");
		Assert.AreEqual("Scoped resource ButtonBackground: #FF0063B1", GetText("ToolkitExtensions_ResourceStatus"));

		App.WaitThenTap("ToolkitExtensions_ToggleState");
		Assert.AreEqual("State: Accent", GetText("ToolkitExtensions_StateStatus"));
	}

	[Test]
	public void When_SelectionAdvances_AllLinkedControlsSynchronize()
	{
		NavigateToSample("Toolkit Extensions");

		App.ScrollDownTo("ToolkitExtensions_NextSelection");
		App.WaitThenTap("ToolkitExtensions_NextSelection");
		Assert.AreEqual(
			"FlipView: 1; TabBar: 1; PipsPager: 1; offset: 0.00",
			GetText("ToolkitExtensions_SelectionStatus"));
	}

	private static string GetText(string automationId)
		=> new QueryEx(q => q.All().Marked(automationId)).GetDependencyPropertyValue<string>("Text");
}

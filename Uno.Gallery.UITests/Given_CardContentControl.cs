using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_CardContentControl : TestBase
{
	[Test]
	public void When_Page_Loads_ToolkitPropertiesAreApplied()
	{
		NavigateToSample("CardContentControl", "Material");

		App.WaitForElement("CardContentControl_Card");
		App.WaitThenTap("CardContentControl_Reset");
		var card = new QueryEx(q => q.All().Marked("CardContentControl_Card"));
		Assert.AreEqual(8d, card.GetDependencyPropertyValue<double>("Elevation"));
		Assert.IsTrue(card.GetDependencyPropertyValue<bool>("IsClickable"));
		Assert.AreEqual("Elevation: 8; Shadow: #FF483D8B; Clickable: True; activations: 0", GetStatus());
	}

	[Test]
	public void When_ConfigurationAndActivationChange_StatusMatchesBehavior()
	{
		NavigateToSample("CardContentControl", "Material");

		App.WaitThenTap("CardContentControl_Reset");
		App.WaitThenTap("CardContentControl_Activate");
		Assert.AreEqual("Elevation: 8; Shadow: #FF483D8B; Clickable: True; activations: 1", GetStatus());

		App.WaitThenTap("CardContentControl_ToggleConfiguration");
		var card = new QueryEx(q => q.All().Marked("CardContentControl_Card"));
		Assert.AreEqual(2d, card.GetDependencyPropertyValue<double>("Elevation"));
		Assert.IsFalse(card.GetDependencyPropertyValue<bool>("IsClickable"));
		Assert.AreEqual("Elevation: 2; Shadow: #FF008080; Clickable: False; activations: 1", GetStatus());

		App.WaitThenTap("CardContentControl_Activate");
		Assert.AreEqual("Elevation: 2; Shadow: #FF008080; Clickable: False; activations: 1", GetStatus());
	}

	private static string GetStatus()
		=> new QueryEx(q => q.All().Marked("CardContentControl_Status"))
			.GetDependencyPropertyValue<string>("Text");
}

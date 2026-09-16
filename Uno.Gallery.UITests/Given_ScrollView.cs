using System;
using System.Globalization;
using NUnit.Framework;
using Uno.UITest;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_ScrollView : TestBase
{
	[Test]
	public void When_ScrollView_Loads()
	{
		NavigateToSample("ScrollView", "Fluent");
		TakeScreenshot("Loaded");
		App.WaitForElement("ScrollView_Vertical");
		App.WaitForElement("ScrollView_Horizontal");
	}

	[Test]
	public void When_ScrollView_Vertical_CanScroll()
	{
		NavigateToSample("ScrollView", "Fluent");
		App.WaitForElement("ScrollView_Vertical");
		App.ScrollDown("ScrollView_Vertical", ScrollStrategy.Gesture);
		TakeScreenshot("After scroll down");
	}

	[Test]
	public void When_ScrollView_ReadOffset_Button_Works()
	{
		NavigateToSample("ScrollView", "Fluent");
		App.WaitForElement("ScrollView_ScrollDown");

		// Use the programmatic scroll button for determinism across all platforms.
		App.WaitThenTap("ScrollView_ScrollDown");
		TakeScreenshot("After programmatic scroll");
		App.Wait(TimeSpan.FromSeconds(1)); // allow ScrollView.ScrollTo animation to settle

		// Read the offset.
		App.WaitThenTap("ScrollView_ReadOffset");
		TakeScreenshot("After read offset");
		App.WaitForElement("ScrollView_OffsetDisplay");

		var display = new QueryEx(x => x.All().Marked("ScrollView_OffsetDisplay"));
		var text = display.GetDependencyPropertyValue<string>("Text");
		Assert.IsNotNull(text, "ScrollView_OffsetDisplay Text must not be null after button tap");
		StringAssert.StartsWith("Vertical offset:", text,
			$"Expected text starting with 'Vertical offset:'; actual: '{text}'");

		var valueStr = text["Vertical offset:".Length..].Trim();
		Assert.IsTrue(
			double.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var offset),
			$"Could not parse '{valueStr}' as a double (invariant culture)");
		Assert.Greater(offset, 0.0, $"Scroll offset must be > 0 after programmatic scroll; actual: '{valueStr}'");
	}
}

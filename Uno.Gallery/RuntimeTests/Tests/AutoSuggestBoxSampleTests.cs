using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Gallery.Helpers;
using Uno.Gallery.Views.Samples;
using Uno.UI.RuntimeTests;

namespace Uno.Gallery.RuntimeTests.Tests;

[TestClass]
public class AutoSuggestBoxSampleTests
{
	[TestMethod]
	[RunsOnUIThread]
	public async Task When_Loaded_SuggestionsContainer_Gets_XamlControlsResources()
	{
		var page = new AutoSuggestBoxSamplePage();
		UnitTestsUIContentHelper.Content = page;

		await UnitTestsUIContentHelper.WaitForLoaded(page);
		await UnitTestsUIContentHelper.WaitForIdle();

		var boxes = VisualTreeHelperEx.GetDescendants(page).OfType<AutoSuggestBox>().ToArray();
		Assert.IsTrue(boxes.Length > 0, "No AutoSuggestBox found in the sample page");

		foreach (var box in boxes)
		{
			var popup = VisualTreeHelperEx.GetDescendants(box).OfType<Popup>().FirstOrDefault(p => p.Name == "SuggestionsPopup");
			var border = popup?.Child as Border;
			Assert.IsNotNull(border, $"SuggestionsContainer not found for '{box.PlaceholderText}'");

			var merged = border.Resources?.MergedDictionaries.OfType<XamlControlsResources>().Any() ?? false;
			Assert.IsTrue(merged, $"SuggestionsContainer of '{box.PlaceholderText}' has no XamlControlsResources");
		}
	}
}

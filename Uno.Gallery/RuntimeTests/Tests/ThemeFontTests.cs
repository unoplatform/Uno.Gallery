using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Uno.Gallery.RuntimeTests.Tests;

/// <summary>
/// Material and Cupertino share the DefaultFontFamily root key that every type scale aliases, so the last merged theme
/// wins it for both; App.xaml pins it to Material's Roboto (unoplatform/Uno.Themes#1762).
/// </summary>
[TestClass]
public class ThemeFontTests
{
	private static string GetFontSource(string key) =>
		Application.Current.Resources.TryGetValue(key, out var value) ? (value as FontFamily)?.Source ?? $"<{value}>" : "<missing>";

	[TestMethod]
	[DataRow("DefaultFontFamily")]
	[DataRow("TitleLargeFontFamily")]
	[DataRow("BodyLargeFontFamily")]
	[DataRow("LabelLargeFontFamily")]
	public void When_Material_TypeScale_Then_Roboto(string key)
	{
		var source = GetFontSource(key);
		Assert.IsTrue(source.Contains("Uno.Fonts.Roboto/Fonts/Roboto.ttf"), $"{key}={source}");
	}

	[TestMethod]
	public void When_Cupertino_Then_Bundled_SFPro()
	{
		var source = GetFontSource("CupertinoFontFamily");
		Assert.IsTrue(source.Contains("SF-Pro-Text-Regular.otf"), $"CupertinoFontFamily={source}");
	}
}

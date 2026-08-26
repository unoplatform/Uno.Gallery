using Microsoft.UI.Xaml.Controls;
using Uno.Gallery;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "SafeArea",
		SourceSdk.UnoToolkit,
		Description = "Attached properties that protect content from device notches, rounded corners, home indicators, and soft keyboard overlaps.",
		DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls/SafeArea.html",
		Tags = new[] { "layout", "platform", "insets", "notch", "keyboard" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "drawer" },
		SortOrder = 50)]
	public sealed partial class SafeAreaSamplePage : Page
	{
		public SafeAreaSamplePage()
		{
			this.InitializeComponent();
		}
	}
}

using Microsoft.UI.Xaml.Controls;
using Uno.Gallery;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "Drawer",
		SourceSdk.UnoToolkit,
		Description = "DrawerControl and DrawerFlyoutPresenter: off-canvas panels that can be revealed by swipe gesture or programmatically.",
		DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls/DrawerControl.html",
		Tags = new[] { "navigation", "drawer", "gesture", "flyout" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "SafeArea" },
		SortOrder = 20)]
	public sealed partial class DrawerSamplePage : Page
	{
		public DrawerSamplePage()
		{
			this.InitializeComponent();
		}
	}
}

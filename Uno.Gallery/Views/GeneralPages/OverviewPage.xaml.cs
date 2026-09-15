using Microsoft.UI.Xaml.Controls;
using static Uno.Gallery.SamplePageLayout;

namespace Uno.Gallery.Views.GeneralPages
{
	[SamplePage(SampleCategory.None, "Overview", glyph: "\uE10F")]
	public sealed partial class OverviewPage : Page
	{
		public OverviewPage()
		{
			this.InitializeComponent();
		}
	}
}

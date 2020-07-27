using Windows.UI.Xaml.Controls;

namespace Uno.Gallery
{
	[SamplePage(SampleCategory.Colors, "Palette for Fluent", SortOrder = 2, Description = Description, OverviewCtaText = OverviewCtaText)]
	public sealed partial class FluentPalettePage : Page
	{
		private const string Description = "View the Uno palette applied to Fluent's styles.";
		private const string OverviewCtaText = "View palette for Fluent";

		public FluentPalettePage()
		{
			this.InitializeComponent();
		}
	}
}

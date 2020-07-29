using Windows.UI.Xaml.Controls;

namespace Uno.Gallery
{
	[SamplePage(SampleCategory.Colors, "Palette for Fluent", SortOrder = 2, Description = Description)]
	public sealed partial class FluentPalettePage : Page
	{
		private const string Description = "View the Uno palette applied to Fluent's styles.";

		public FluentPalettePage()
		{
			this.InitializeComponent();
		}
	}
}

using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{

	[SamplePage(SampleCategory.UIFeatures, "Acrylic", Description = "AcrylicBrush is a translucent brush that can be used as background.", DocumentationLink = "https://docs.microsoft.com/en-us/windows/uwp/design/style/acrylic")]
	[SampleConditional(SampleConditionals.Always ^ SampleConditionals.SkiaGtk, Reason = "Acrylic is not supported")]
	public sealed partial class AcrylicSamplePage : Page
	{
		public AcrylicSamplePage()
		{
			this.InitializeComponent();
		}
	}
}

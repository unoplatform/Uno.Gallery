using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "Expander",
		Description = Description,
		DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/expander",
		Slug = "expander",
		Tags = new[] { "layout", "disclosure", "toggle", "container" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "TeachingTip", "InfoBar" })]
	public sealed partial class ExpanderSamplePage : Page
	{
		private const string Description =
			"Expander is a control that shows or hides supplementary content associated with a header. " +
			"It supports custom headers, up/down expand direction, and disabled state.";

		public ExpanderSamplePage()
		{
			this.InitializeComponent();
		}
	}
}

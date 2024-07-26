using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "PipsPager", Description = "Represents a control that enables navigation within linearly paginated content using a configurable collection of glyphs, each of which represents a single \"page\" within a limitless range.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.pipspager")]
	public sealed partial class PipsPagerSamplePage : Page
	{
		public PipsPagerSamplePage()
		{
			InitializeComponent();
		}
	}
	public class FlipViewItemCollection : ObservableCollection<FlipViewItem>
	{
		public FlipViewItemCollection()
		{
		}
	}
}

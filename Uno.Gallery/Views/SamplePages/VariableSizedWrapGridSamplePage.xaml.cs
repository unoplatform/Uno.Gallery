using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "VariableSizedWrapGrid", Description = "Provides a grid-style layout panel where each tile/cell can be variable size based on content.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.variablesizedwrapgrid")]
	public sealed partial class VariableSizedWrapGridSamplePage : Page
	{
		public VariableSizedWrapGridSamplePage()
		{
			InitializeComponent();
			DataContext = new VariableSizedWrapGridViewModel();
		}

		public class VariableSizedWrapGridViewModel : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			private int _width;
			public int Width
			{
				get => _width;
				set
				{
					_width = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Width)));
				}
			}
		}
	}
}

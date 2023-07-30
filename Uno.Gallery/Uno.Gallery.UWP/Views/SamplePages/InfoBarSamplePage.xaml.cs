using Uno.Gallery.ViewModels;
using Windows.UI.Xaml.Controls;


namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.UIComponents, "InfoBar",
		Description = "This control is an inline notification for essential app-wide messages.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.infobar",
		DataType = typeof(InfoBarSamplePageViewModel))]
	public sealed partial class InfoBarSamplePage : Page
	{
		public InfoBarSamplePage()
		{
			this.InitializeComponent();
		}
	}

	public class InfoBarSamplePageViewModel : ViewModelBase
	{
		public bool IsOpen { get => GetProperty<bool>(); set => SetProperty(value); }
		public string Severity { get => GetProperty<string>(); set => SetProperty(value); }
		public bool IsIconVisible { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool IsClosable { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool LongIsOpen { get => GetProperty<bool>(); set => SetProperty(value); }

		public InfoBarSamplePageViewModel()
		{
			IsOpen = true;
			Severity = "Informational";
			IsIconVisible = true;
			IsClosable = true;
			LongIsOpen = true;
		}
	}
}

using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
<<<<<<< HEAD:Uno.Gallery/Uno.Gallery.UWP/Views/SamplePages/TextBlockSamplePage.xaml.cs
	[SamplePage(SampleCategory.UIComponents, "TextBlock", Description = "This control is used to display text.", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.textblock")]
=======
	[SamplePage(SampleCategory.UIComponents, "TextBlock",
		Description = "A lightweight control for displaying text.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.textblock")]
>>>>>>> daa59c9 (chore: move to WinUI/WinAppSDK):Uno.Gallery/Uno.Gallery.Shared/Views/SamplePages/TextBlockSamplePage.xaml.cs
	public sealed partial class TextBlockSamplePage : Page
	{
		public TextBlockSamplePage()
		{
			this.InitializeComponent();
		}
	}
}

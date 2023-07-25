using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Uno.Gallery.ViewModels;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace Uno.Gallery.Views.SamplePages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	[SamplePage(SampleCategory.Theming, "Lightweight Styling", Description = "Lightweight styling enables the customization of a control's appearance by adjusting individual keys rather than completely redefining its style.", DocumentationLink = "https://material.io/components/cards")]
	public sealed partial class LightweightStylingSamplePage : Page
	{
		public LightweightStylingSamplePage()
		{
			this.InitializeComponent();
		}
	}
}

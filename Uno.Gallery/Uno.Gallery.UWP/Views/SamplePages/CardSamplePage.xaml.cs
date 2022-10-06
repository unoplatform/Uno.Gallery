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

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.UIComponents, "Card", Description = "This control is used to display content and actions about a single item.", DocumentationLink = "https://material.io/components/cards")]
	public sealed partial class CardSamplePage : Page
	{
		public CardSamplePage()
		{
			this.InitializeComponent();
		}
	}
}

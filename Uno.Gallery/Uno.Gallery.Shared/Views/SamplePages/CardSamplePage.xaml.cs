using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

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

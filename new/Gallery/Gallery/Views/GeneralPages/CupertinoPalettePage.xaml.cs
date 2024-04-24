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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.GeneralPages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	[SamplePage(SampleCategory.Theming, "Cupertino Palette",
		SortOrder = 3,
		Description = "Uno.Cupertino comes with a built-in set of named Color and Brush resources. They are used in most control styles provided by Uno.Cupertino, meaning that you can easily style most controls just by changing a few colors.")]
	public sealed partial class CupertinoPalettePage : Page
	{
		public CupertinoPalettePage()
		{
			this.InitializeComponent();
		}
	}
}

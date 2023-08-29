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

namespace Uno.Gallery
{
	[SamplePage(SampleCategory.Theming, "Material Palette",
		SortOrder = 1,
		Description = "Uno.Material comes with a built-in set of named Color and Brush resources. They are used in most control styles provided by Uno.Material, meaning that you can easily style most controls just by changing a few colors.")]
	public sealed partial class MaterialPalettePage : Page
	{
		public MaterialPalettePage()
		{
			this.InitializeComponent();
		}
	}
}

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

namespace Uno.Gallery
{
	[SamplePage(SampleCategory.Colors, "Palette for Material", SortOrder = 1)]
	public sealed partial class MaterialPalettePage : Page
	{
		public MaterialPalettePage()
		{
			this.InitializeComponent();
		}
	}
}

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
	[SamplePage(SampleCategory.Theming, "Material Palette", SortOrder = 1, Description = Description)]
	public sealed partial class MaterialPalettePage : Page
	{
		private const string Description = "View the Uno palette adapted to Material's styles.";

		public MaterialPalettePage()
		{
			this.InitializeComponent();
		}
	}
}

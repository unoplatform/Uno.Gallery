﻿using System;
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

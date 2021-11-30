using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Gallery.Entities.Data;
using Uno.Toolkit.UI;
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
	[SamplePage(SampleCategory.Toolkit, "ChipGroup",
		SourceSdk.UnoToolkit,
		Description = "ChipGroup is used to hold multiple Chips which are compact elements that represent an input, attribute, or action.",
		DocumentationLink = "https://material.io/components/chips",
		DataType = typeof(TestCollections))]
	public sealed partial class ChipGroupSamplePage : Page
	{
		public ChipGroupSamplePage()
		{
			this.InitializeComponent();
		}


		private void RemoveChipItem(object sender, ChipItemEventArgs e)
		{
			if (DataContext is Sample sample)
			{
				if (sample.Data is TestCollections test)
				{
					test.RemoveChipItem(e.Item as TestCollections.SelectableData);
				}
			}
		}

		private void ResetChipItems(object sender, RoutedEventArgs e)
		{
			if (DataContext is Sample sample)
			{
				if (sample.Data is TestCollections test)
				{
					test.ResetChipItems();
				}
			}
		}
	}
}

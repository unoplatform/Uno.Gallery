using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Uno.Gallery.ViewModels;
using Uno.Gallery.Views.NestedPages;
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

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "TabBar",
		SourceSdk.UnoToolkit,
		Description = "A control to display a set of TabBarItems horizontally with the ability to display a custom view to denote selected state",
		DataType = typeof(TabBarViewModel))]
	public sealed partial class TabBarSamplePage : Page
	{
		public TabBarSamplePage()
		{
			this.InitializeComponent();
		}

		private void ShowMaterialTopBarSampleInNestedFrame(object sender, RoutedEventArgs e)
		{
			Shell.GetForCurrentView()?.ShowNestedSample<MaterialTopBarSampleNestedPage>(clearStack: true);
		}

		//private void ShowMaterialBottomBarSampleInNestedFrame(object sender, RoutedEventArgs e)
		//{
		//	Shell.GetForCurrentView()?.ShowNestedSample<MaterialBottomBarSampleNestedPage>(clearStack: true);
		//}

		private void ShowCupertinoBottomBarSampleInNestedFrame(object sender, RoutedEventArgs e)
		{
			Shell.GetForCurrentView()?.ShowNestedSample<CupertinoBottomBarSampleNestedPage>(clearStack: true);
		}
	}

	public class TabBarViewModel : ViewModelBase
	{
		public List<string> Items { get; }

		public int HitCounter1 { get => GetProperty<int>(); set => SetProperty(value); }
		public int HitCounter2 { get => GetProperty<int>(); set => SetProperty(value); }
		public int HitCounter3 { get => GetProperty<int>(); set => SetProperty(value); }

		public ICommand IncrementCounterCommand => new Command(IncrementCounter);
		public ICommand ResetAllCounterCommand => new Command(ResetAllCounter);

		public TabBarViewModel()
		{
			Items = Enumerable.Range(1, 3)
				.Select(x => $"Item #{x}")
				.ToList();
		}

		private void IncrementCounter(object parameter)
		{
			if (parameter is string p && int.TryParse(p, out var index))
			{
				switch (index)
				{
					case 1: HitCounter1++; return;
					case 2: HitCounter2++; return;
					case 3: HitCounter3++; return;

					default: break;
				}
			}

#if DEBUG
			throw new ArgumentOutOfRangeException("CommandParameter", parameter, "Invalid parameter");
#endif
		}

		private void ResetAllCounter()
		{
			HitCounter1 =
			HitCounter2 =
			HitCounter3 = 0;
		}
	}
}

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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

        private void ShowMaterialBottomBarSampleInNestedFrame(object sender, RoutedEventArgs e)
        {
            Shell.GetForCurrentView()?.ShowNestedSample<MaterialBottomBarSampleNestedPage>(clearStack: true);
        }

        private void ShowCupertinoBottomBarSampleInNestedFrame(object sender, RoutedEventArgs e)
        {
            Shell.GetForCurrentView()?.ShowNestedSample<CupertinoBottomBarSampleNestedPage>(clearStack: true);
        }
    }

    public class TabBarViewModel : ViewModelBase
    {
        public int Tab1Count { get => GetProperty<int>(); set => SetProperty(value); }
        public int Tab2Count { get => GetProperty<int>(); set => SetProperty(value); }
        public int Tab3Count { get => GetProperty<int>(); set => SetProperty(value); }

        public int MaterialBottomTab1Count { get => GetProperty<int>(); set => SetProperty(value); }
        public int MaterialBottomTab2Count { get => GetProperty<int>(); set => SetProperty(value); }
        public int MaterialBottomTab3Count { get => GetProperty<int>(); set => SetProperty(value); }

        public int CupertinoBottomTab1Count { get => GetProperty<int>(); set => SetProperty(value); }
        public int CupertinoBottomTab2Count { get => GetProperty<int>(); set => SetProperty(value); }
        public int CupertinoBottomTab3Count { get => GetProperty<int>(); set => SetProperty(value); }

        public List<string> Items { get => GetProperty<List<string>>(); set => SetProperty(value); }

        public ICommand Tab1CountCommand => new Command(_ => Tab1Count++);
        public ICommand Tab2CountCommand => new Command(_ => Tab2Count++);
        public ICommand Tab3CountCommand => new Command(_ => Tab3Count++);

        public ICommand MaterialBottomTab1CountCommand => new Command(_ => MaterialBottomTab1Count++);
        public ICommand MaterialBottomTab2CountCommand => new Command(_ => MaterialBottomTab2Count++);
        public ICommand MaterialBottomTab3CountCommand => new Command(_ => MaterialBottomTab3Count++);

        public ICommand CupertinoBottomTab1CountCommand => new Command(_ => CupertinoBottomTab1Count++);
        public ICommand CupertinoBottomTab2CountCommand => new Command(_ => CupertinoBottomTab2Count++);
        public ICommand CupertinoBottomTab3CountCommand => new Command(_ => CupertinoBottomTab3Count++);

        public TabBarViewModel()
        {
            Items = new List<string> { "Tab 1", "Tab 2", "Tab 3" };
        }
    }
}

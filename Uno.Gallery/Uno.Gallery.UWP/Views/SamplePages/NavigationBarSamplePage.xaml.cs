using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    [SamplePage(SampleCategory.Toolkit, "NavigationBar",
        SourceSdk.UnoToolkit,
        Description = "Represents a specialized app bar that provides layout for AppBarButton and navigation logic.")]
    public sealed partial class NavigationBarSamplePage : Page
    {
        public NavigationBarSamplePage()
        {
            this.InitializeComponent();
        }

        private void LaunchFullScreenSample(object sender, RoutedEventArgs e)
        {
            Shell.GetForCurrentView().ShowNestedSample<NavigationBarSample_NestedPage1>(clearStack: true);
        }
    }
}

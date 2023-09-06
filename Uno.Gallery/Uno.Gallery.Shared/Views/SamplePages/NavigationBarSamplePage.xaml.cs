using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
            Shell.GetForCurrentView().ShowNestedSample<ShadowContainerSamplePage>(clearStack: true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshVisualizer = Microsoft.UI.Xaml.Controls.RefreshVisualizer;
using RefreshVisualizerState = Microsoft.UI.Xaml.Controls.RefreshVisualizerState;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;
using RefreshInteractionRatioChangedEventArgs = Microsoft.UI.Xaml.Controls.RefreshInteractionRatioChangedEventArgs;
using RefreshStateChangedEventArgs = Microsoft.UI.Xaml.Controls.RefreshStateChangedEventArgs;
using RefreshPullDirection = Microsoft.UI.Xaml.Controls.RefreshPullDirection;

namespace Uno.Gallery.Views.Samples
{

    [SamplePage(
        SampleCategory.Components,
        "RefreshContainer",
        Description = "RefreshContainer is a control that enables pull to refresh experiences on mobile platforms.",
        DocumentationLink = "https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.refreshcontainer")]
    public sealed partial class RefreshContainerPage : Page
    {
		private readonly Random _randomizer = new Random();
        private RefreshContainer _refreshContainer = null;
        
        public RefreshContainerPage()
        {
            this.InitializeComponent();
            this.Loaded += RefreshContainerPage_Loaded;
        }

        private void RefreshContainerPage_Loaded(object sender, RoutedEventArgs e)
        {
            _refreshContainer = FindRefreshContainer(this);
            _refreshContainer.RefreshRequested += RefreshContainer_RefreshRequested;
        }

        internal RefreshContainer FindRefreshContainer(DependencyObject source)
        {
            int count = VisualTreeHelper.GetChildrenCount(source);
            for (int i = 0; i < count; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(source, i);
                if (!(current is RefreshContainer refresh))
                {
                    refresh = FindRefreshContainer(current);
                }
                
                if (refresh != null)
                {
                    return refresh;
                }
            }
            return null;
        }

        private void ChangeColors_Click(object sender, RoutedEventArgs args)
        {
            var foregroundColor = Color.FromArgb(255, (byte)_randomizer.Next(0, 256), (byte)_randomizer.Next(0, 256), (byte)_randomizer.Next(0, 256));
            var backgroundColor = Color.FromArgb(255, (byte)_randomizer.Next(0, 256), (byte)_randomizer.Next(0, 256), (byte)_randomizer.Next(0, 256));

            _refreshContainer.Visualizer.Foreground = new SolidColorBrush(foregroundColor);
            _refreshContainer.Visualizer.Background = new SolidColorBrush(backgroundColor);
        }

        private void RequestRefresh_Click(object sender, RoutedEventArgs args)
        {
            _refreshContainer.RequestRefresh();
        }

        private async void RefreshContainer_RefreshRequested(object sender, RefreshRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();
            await Task.Delay(2000);
            deferral.Complete();
        }
    }
}

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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.NestedPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MaterialTopBarSampleNestedPage : Page
    {
        public MaterialTopBarSampleNestedPage()
        {
            this.InitializeComponent();
        }

        private void NavigateBack(object sender, RoutedEventArgs e)
        {
            // Normally we would've just called `Frame.GoBack();` if we only have a single frame.
            // However, a nested frame is used to show-case fullscreen sample, so we need some
            // custom handling to hide the nested frame on back navigation when the stack is empty.
            Shell.GetForCurrentView()?.BackNavigateFromNestedSample();
        }
    }
}

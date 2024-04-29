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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.SamplePages
{
    [SamplePage(SampleCategory.Toolkit, "SegmentedControl",
        SourceSdk.UnoToolkit,
        Description = "A segmented control is a Cupertino-only linear set of two or more segments, each of which functions as a mutually exclusive button.")]
    public sealed partial class SegmentedControlSamplePage : Page
    {
        public SegmentedControlSamplePage()
        {
            this.InitializeComponent();
        }
    }
}

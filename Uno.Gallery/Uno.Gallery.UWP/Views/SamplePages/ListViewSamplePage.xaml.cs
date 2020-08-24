using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.Samples
{

    [SamplePage(SampleCategory.Features, "ListView", Description = "Represents a control that displays data items in a vertical stack.", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.listview")]
	public sealed partial class ListViewSamplePage : Page
	{
        public ListViewSamplePage()
		{
			this.InitializeComponent();
		}

        public List<Record> Records = new List<Record>() {
            new Record(compositionName: "Mass in B minor", artistName: "Johann Sebastian Bach", color: "#159bff"),
            new Record(compositionName: "Third Symphony", artistName: "Ludwig van Beethoven", color: "#7a67f8"),
            new Record(compositionName: "Serse", artistName: "George Frideric Handel", color: "#67e5ad"),
            new Record(compositionName: "Idomeneo", artistName: "Wolfgang Amadeus Mozart", color: "#f85977")
        };

        public class Record
        {
            public Record(string compositionName, string artistName, string color)
            {
                CompositionName = compositionName;
                ArtistName = artistName;
                Color = color;
            }

            public string CompositionName;
            public string ArtistName;
            public string Color;
        }
    }
}

/*
{
  "Recordings": [
    {
      "CompositionName": "Mass in B minor",
      "ArtistName": "Johann Sebastian Bach",
      "Color": "#159bff"
    }, {
      "CompositionName": "Third Symphony",
      "ArtistName": "Ludwig van Beethoven",
      "Color": "#7a67f8"
    }, {
      "CompositionName": "Serse",
      "ArtistName": "George Frideric Handel",
      "Color": "#67e5ad"
    }, {
      "CompositionName": "Idomeneo",
      "ArtistName": "Wolfgang Amadeus Mozart",
      "Color": "#f85977"
    }
  ]
}
*/
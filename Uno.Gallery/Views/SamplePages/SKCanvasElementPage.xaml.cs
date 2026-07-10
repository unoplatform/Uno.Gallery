using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
#if HAS_UNO_SKIA
using Uno.Gallery.Helpers.Canvas;
#endif

namespace Uno.Gallery.Views.SamplePages;

#if HAS_UNO_SKIA
[SamplePage(SampleCategory.UIComponents, "SKCanvasElement", Description = "Represents a 2D graphics canvas.", DocumentationLink = "https://platform.uno/docs/articles/controls/SKCanvasElement.html")]
#endif
public sealed partial class SKCanvasElementPage : Page
{
#if HAS_UNO_SKIA
	public int MaxSampleIndex => SampleSKCanvasElement.SampleCount - 1;

	private SampleSKCanvasElement _canvasElement;
#endif

	public SKCanvasElementPage()
	{
		this.InitializeComponent();

		this.Loaded += SKCanvasElementPage_Loaded;
	}

	private void SKCanvasElementPage_Loaded(object sender, RoutedEventArgs e)
	{
#if HAS_UNO_SKIA
		var container = (Grid)FindName("SKContainer");
		container.Children.Add(_canvasElement = new SampleSKCanvasElement());
#endif
	}

	private void NextSample_Click(object sender, RoutedEventArgs e)
	{
#if HAS_UNO_SKIA
		_canvasElement.Sample = (_canvasElement.Sample + 1) % SampleSKCanvasElement.SampleCount;
#endif
	}
}

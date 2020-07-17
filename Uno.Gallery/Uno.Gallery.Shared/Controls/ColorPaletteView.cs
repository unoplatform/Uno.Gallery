using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Uno.Gallery.Controls
{
	/// <summary>
	/// This controls is used to display a color.
	/// </summary>
	public partial class ColorPaletteView : Control
	{
		public ColorPaletteView()
		{
			DefaultStyleKey = typeof(ColorPaletteView);
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register("Title", typeof(string), typeof(ColorPaletteView), new PropertyMetadata(string.Empty));

		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}

		public static readonly DependencyProperty DescriptionProperty =
			DependencyProperty.Register("Description", typeof(string), typeof(ColorPaletteView), new PropertyMetadata(string.Empty));

		public string ColorName
		{
			get { return (string)GetValue(ColorNameProperty); }
			set { SetValue(ColorNameProperty, value); }
		}

		public static readonly DependencyProperty ColorNameProperty =
			DependencyProperty.Register("ColorName", typeof(string), typeof(ColorPaletteView), new PropertyMetadata(string.Empty));

		public string ColorSecondaryName
		{
			get { return (string)GetValue(ColorSecondaryNameProperty); }
			set { SetValue(ColorSecondaryNameProperty, value); }
		}

		public static readonly DependencyProperty ColorSecondaryNameProperty =
			DependencyProperty.Register("ColorSecondaryName", typeof(string), typeof(ColorPaletteView), new PropertyMetadata(string.Empty));

		public Brush ColorBrush
		{
			get { return (Brush)GetValue(ColorBrushProperty); }
			set { SetValue(ColorBrushProperty, value); }
		}

		public static readonly DependencyProperty ColorBrushProperty =
			DependencyProperty.Register("ColorBrush", typeof(Brush), typeof(ColorPaletteView), new PropertyMetadata(null));

		public double ColorHeight
		{
			get { return (double)GetValue(ColorHeightProperty); }
			set { SetValue(ColorHeightProperty, value); }
		}

		public static readonly DependencyProperty ColorHeightProperty =
			DependencyProperty.Register("ColorHeight", typeof(double), typeof(ColorPaletteView), new PropertyMetadata(0f));
	}
}

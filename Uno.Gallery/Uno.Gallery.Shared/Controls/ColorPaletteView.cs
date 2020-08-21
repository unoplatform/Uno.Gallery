using System;
using System.Collections.Generic;
using System.Text;
using Uno.Gallery.Converters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Uno.Gallery
{
	/// <summary>
	/// This controls is used to display a color.
	/// </summary>
	public partial class ColorPaletteView : Control
	{
		public ColorPaletteView()
		{
			DefaultStyleKey = typeof(ColorPaletteView);

			ActualThemeChanged += OnThemeChanged;
		}

		private void OnThemeChanged(FrameworkElement sender, object args)
		{
			ColorHex = FromColorBrushToHexConverter.GetHexName(ColorBrush);
			OnColorHex = FromColorBrushToHexConverter.GetHexName(OnColorBrush);
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

		public Brush ColorBrush
		{
			get { return (Brush)GetValue(ColorBrushProperty); }
			set { SetValue(ColorBrushProperty, value); }
		}

		public static readonly DependencyProperty ColorBrushProperty =
			DependencyProperty.Register("ColorBrush", typeof(Brush), typeof(ColorPaletteView), new PropertyMetadata(null, OnColorBrushChanged));

		private static void OnColorBrushChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			var that = (ColorPaletteView)dependencyObject;
			that.ColorHex = FromColorBrushToHexConverter.GetHexName(args.NewValue);
		}

		public Brush OnColorBrush
		{
			get { return (Brush)GetValue(OnColorBrushProperty); }
			set { SetValue(OnColorBrushProperty, value); }
		}

		public static readonly DependencyProperty OnColorBrushProperty =
			DependencyProperty.Register("OnColorBrush", typeof(Brush), typeof(ColorPaletteView), new PropertyMetadata(null, OnOnColorBrushChanged));

		private static void OnOnColorBrushChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			var that = (ColorPaletteView)dependencyObject;
			that.OnColorHex = FromColorBrushToHexConverter.GetHexName(args.NewValue);
		}

		public double ColorHeight
		{
			get { return (double)GetValue(ColorHeightProperty); }
			set { SetValue(ColorHeightProperty, value); }
		}

		public static readonly DependencyProperty ColorHeightProperty =
			DependencyProperty.Register("ColorHeight", typeof(double), typeof(ColorPaletteView), new PropertyMetadata(0f));

		public string ColorHex
		{
			get { return (string)GetValue(ColorHexProperty); }
			set { SetValue(ColorHexProperty, value); }
		}

		public static readonly DependencyProperty ColorHexProperty =
			DependencyProperty.Register("ColorHex", typeof(string), typeof(ColorPaletteView), new PropertyMetadata(null));

		public string OnColorHex
		{
			get { return (string)GetValue(OnColorHexProperty); }
			set { SetValue(OnColorHexProperty, value); }
		}

		public static readonly DependencyProperty OnColorHexProperty =
			DependencyProperty.Register("OnColorHex", typeof(string), typeof(ColorPaletteView), new PropertyMetadata(null));
	}
}

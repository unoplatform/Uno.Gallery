using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery
{
	/// <summary>
	/// A control used for displaying information about TextBlock Styles in TypographySample.
	/// </summary>
	public partial class TypographyControl : ContentControl
	{
		public string ExampleColText
		{
			get => (string)GetValue(ExampleColTextProperty);
			set => SetValue(ExampleColTextProperty, value);
		}

		public static readonly DependencyProperty ExampleColTextProperty =
			DependencyProperty.Register(nameof(ExampleColText), typeof(string), typeof(TypographyControl), new PropertyMetadata(null));

		public string VariableFontColText
		{
			get => (string)GetValue(VariableFontColTextProperty);
			set => SetValue(VariableFontColTextProperty, value);
		}

		public static readonly DependencyProperty VariableFontColTextProperty =
			DependencyProperty.Register(nameof(VariableFontColText), typeof(string), typeof(TypographyControl), new PropertyMetadata(null));

		public string FontSizeColText
		{
			get => (string)GetValue(FontSizeColTextProperty);
			set => SetValue(FontSizeColTextProperty, value);
		}

		public static readonly DependencyProperty FontSizeColTextProperty =
			DependencyProperty.Register(nameof(FontSizeColText), typeof(string), typeof(TypographyControl), new PropertyMetadata(null));

		public string StyleColText
		{
			get => (string)GetValue(StyleColTextProperty);
			set => SetValue(StyleColTextProperty, value);
		}

		public static readonly DependencyProperty StyleColTextProperty =
			DependencyProperty.Register(nameof(StyleColText), typeof(string), typeof(TypographyControl), new PropertyMetadata(null));

		public bool IsCopyEnabled
		{
			get => (bool)GetValue(IsCopyEnabledProperty);
			set => SetValue(IsCopyEnabledProperty, value);
		}

		public static readonly DependencyProperty IsCopyEnabledProperty =
			DependencyProperty.Register(nameof(IsCopyEnabled), typeof(bool), typeof(TypographyControl), new PropertyMetadata(true));

		public bool IsHeader
		{
			get => (bool)GetValue(IsHeaderProperty);
			set => SetValue(IsHeaderProperty, value);
		}

		public static readonly DependencyProperty IsHeaderProperty =
			DependencyProperty.Register(nameof(IsHeaderProperty), typeof(bool), typeof(TypographyControl), new PropertyMetadata(false));

		public Style TypographyStyle
		{
			get => (Style)GetValue(TypographyStyleProperty);
			set => SetValue(TypographyStyleProperty, value);
		}

		public static readonly DependencyProperty TypographyStyleProperty =
			DependencyProperty.Register(nameof(TypographyStyle), typeof(Style), typeof(TypographyControl), new PropertyMetadata(null));

		public string DetailsText
		{
			get => $"{StyleColText} • {VariableFontColText} • {FontSizeColText}";
		}
	}
}

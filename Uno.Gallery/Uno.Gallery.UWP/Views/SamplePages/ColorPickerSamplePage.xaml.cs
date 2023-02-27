using System;
using Uno.Gallery.ViewModels;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "ColorPicker", Description = "This control allows users to choose a desired color.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.colorpicker", DataType = typeof(ColorPickerSamplePageViewModel))]
	public sealed partial class ColorPickerSamplePage : Page
	{
		public ColorPickerSamplePage()
		{
			this.InitializeComponent();
		}

		public void ColorSpectrumShapeRadio_Checked(object sender, RoutedEventArgs e)
		{
			if (((Sample)DataContext).Data is ColorPickerSamplePageViewModel viewModel)
			{
				if (Enum.TryParse<Microsoft.UI.Xaml.Controls.ColorSpectrumShape>(((RadioButton)sender).Content?.ToString(), out var value))
				{
					viewModel.ColorSpectrumShape = value;
				}
			}
		}
	}

	public class ColorPickerSamplePageViewModel : ViewModelBase
	{
		public bool IsColorSliderVisible { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool IsColorChannelTextInputVisible { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool IsHexInputVisible { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool IsMoreButtonVisible { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool AlphaEnabled { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool IsAlphaSliderVisible { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool IsAlphaTextInputVisible { get => GetProperty<bool>(); set => SetProperty(value); }
		public Microsoft.UI.Xaml.Controls.ColorSpectrumShape ColorSpectrumShape { get => GetProperty<Microsoft.UI.Xaml.Controls.ColorSpectrumShape>(); set => SetProperty(value); }

		public Color CurrentColor 
		{
			get => GetProperty<Color>();
			set => SetProperty(value); 
	    }

		public ColorPickerSamplePageViewModel()
		{
			Random r = new Random();

			byte red = (byte)r.Next(0, 256);
			byte green = (byte)r.Next(0, 256);
			byte blue = (byte)r.Next(0, 256);

			CurrentColor = Color.FromArgb(255, red, green, blue);

			IsColorSliderVisible = true;
			IsColorChannelTextInputVisible = true;
			IsHexInputVisible = true;
			IsMoreButtonVisible = false;
			AlphaEnabled = false;
			IsAlphaSliderVisible = true;
			IsAlphaTextInputVisible = true;
			ColorSpectrumShape = Microsoft.UI.Xaml.Controls.ColorSpectrumShape.Box;
		}
	}
}

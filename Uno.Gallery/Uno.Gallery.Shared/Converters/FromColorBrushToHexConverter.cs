using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Uno.Gallery.Converters
{
	public class FromColorBrushToHexConverter : IValueConverter
	{
		/// <summary>
		/// This is used to format bytes into Hexadecimal number with 2 uppercase characters.
		/// For example 13 => 0D
		/// </summary>
		private const string HexadecimalStringFormat = "X2";

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return GetHexName(value);
		}		

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
		
		public static string GetHexName(object value)
		{
			if (value is SolidColorBrush brush)
			{
				var colorHex = "#" + brush.Color.R.ToString(HexadecimalStringFormat) + brush.Color.G.ToString(HexadecimalStringFormat) + brush.Color.B.ToString(HexadecimalStringFormat);
				if(brush.Opacity != 1)
				{
					// Shows " 50%"
					colorHex += $" {brush.Opacity:P0}";
				}

				return colorHex;
			}

			return string.Empty;
		}
	}
}

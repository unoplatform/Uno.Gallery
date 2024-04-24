using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using Windows.UI;

namespace Uno.Gallery.Converters
{
	public class HexToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value?.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (value is not string { Length: 9 } hex || !hex.StartsWith("#"))
			{
				return value;
			}

			return XamlBindingHelper.ConvertValue(typeof(Color), hex);
		}
	}
}

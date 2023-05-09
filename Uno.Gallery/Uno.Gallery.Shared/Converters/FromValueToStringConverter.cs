using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml.Data;
using Windows.UI;

namespace Uno.Gallery.Converters
{
	public class FromValueToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is Color color)
			{
				// Use reflection to get the name of the color
				PropertyInfo colorProperty = typeof(Colors).GetRuntimeProperties().FirstOrDefault(p => ((Color)p.GetValue(null)).Equals(color));
				if (colorProperty != null)
				{
					return colorProperty.Name;
				}
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException("Only one-way conversion is supported.");
	}
}


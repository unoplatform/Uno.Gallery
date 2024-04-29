using System;
using Microsoft.UI.Xaml.Data;

namespace Uno.Gallery.Converters
{
	public class FromObjectToValueConverter : IValueConverter
	{
		public object NullValue { get; set; }

		public object NotNullValue { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language) => value is null ? NullValue : NotNullValue;

		public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException("Only one-way conversion is supported.");
	}
}

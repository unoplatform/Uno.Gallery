using Microsoft.UI.Xaml.Data;
using System;

namespace Uno.Gallery.Converters;

public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		DateTime date = (DateTime)value;
		return new DateTimeOffset(date);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		DateTimeOffset dateTimeOffset = (DateTimeOffset)value;
		return dateTimeOffset.DateTime;
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace Uno.Gallery.Converters
{
	public abstract class EnumConverterBase<TEnum> : IValueConverter
		where TEnum : struct, IConvertible
	{
		public object Convert(object value, Type targetType, object parameter, string language) => (value is TEnum @enum)
								? ConvertEnum(@enum, targetType, parameter, language)
								: throw new InvalidOperationException($"An value of type {typeof(TEnum).Name} is expected. Got '{value?.GetType().Name ?? "(null)"}' instead.");

		public abstract object ConvertEnum(TEnum value, Type targetType, object parameter, string language);

		public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException("Only one-way conversion is supported.");
	}
}

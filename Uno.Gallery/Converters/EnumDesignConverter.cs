using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.Gallery.Converters
{
	public class EnumDesignConverter : EnumConverterBase<Design>
	{
		public object MaterialValue { get; set; }

		public object FluentValue { get; set; }

		public override object ConvertEnum(Design value, Type targetType, object parameter, string language)
		{
			switch (value)
			{
				case Design.Material: return MaterialValue;
				case Design.Fluent: return FluentValue;

				default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unexpected value");
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Uno.Gallery.Converters
{
	public class RandomColorConverter : IValueConverter
	{
		public enum RngProvider { HashCode, Random }

		public enum OutputType { Color, Brush }

		public RngProvider Provider { get; set; } = RngProvider.Random;

		public OutputType Output { get; set; } = OutputType.Color;

		public static readonly Random _random = new Random(Guid.NewGuid().GetHashCode());
		private static readonly Color[] _knownColors = GetKnownColors().ToArray();


		private static IEnumerable<Color> GetKnownColors()
		{
			return typeof(Colors)
#if WINDOWS_UWP
				.GetProperties(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.PropertyType == typeof(Color) && x.GetMethod != null)
#else
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.FieldType == typeof(Color))
#endif
				.Select(x => (Color)x.GetValue(null));
		}

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var rng = Provider == RngProvider.HashCode
				? value?.GetHashCode() ?? 0
				: _random.Next();
			var color = _knownColors[rng % _knownColors.Length];

			return Output == OutputType.Color ? (object)color : new SolidColorBrush(color);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException("Only one-way conversion is supported.");
	}
}

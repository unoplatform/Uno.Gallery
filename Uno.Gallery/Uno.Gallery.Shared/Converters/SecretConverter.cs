using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Uno.Gallery.Converters
{
	public class SecretConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language) => (value as bool?) == true
			? GetAnswer()
			: default;

		private string GetAnswer()
		{
			var keys = Application.Current.Resources.MergedDictionaries
				.SelectMany(x => x.Keys)
				.OfType<string>()
				.ToArray();
			var a = keys
				.GroupBy(x => Regex.Replace(x, "([A-Z][a-z]+)([A-Z][a-z]+){2}Brush", "$1"))
				.OrderBy(x => x.Count())
				.Select(x => x.Key)
				.LastOrDefault();
			var b = keys
				.Where(x => Regex.Replace(x, "[a-z]", string.Empty) == "GVIRBSACS")
				.Select(x => string.Concat(new[] { 1, 24, 20, 21, 32 }.Select(y => x[y])))
				.FirstOrDefault();
			var answer = Regex.Replace(string.Join(" ", a, b), @"\b[a-z]", x => x.Value.ToUpperInvariant());

			return answer;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException("Only one-way conversion is supported.");
	}
}

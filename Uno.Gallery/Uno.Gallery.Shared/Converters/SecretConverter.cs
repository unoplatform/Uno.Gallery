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
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var date = (value as DateTimeOffset?)?.Date == new DateTime(2021, 11, 30).Date;

			if (date && (string)parameter == "HiddenSecret")
			{
				return GetAnswer();
			}
			else if (date && (string)parameter == "SecretDate")
			{
				return Visibility.Visible;
			}
			else
			{
				return Visibility.Collapsed;
			}
		}

		private string GetAnswer()
		{
			var keys = Application.Current.Resources.MergedDictionaries
				.SelectMany(x => x.Keys)
				.OfType<string>()
				.ToArray();

			var a = keys
				.GroupBy(x => Regex.Replace(x, "([A-Z][a-z]+)([A-Z][a-z]+){2}", "$2"))
				.OrderBy(x => x.Count())
				.Select(x => x.Key)
				.LastOrDefault();

			var answer = a[3].ToString().ToUpper();

			return answer;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException("Only one-way conversion is supported.");
	}
}
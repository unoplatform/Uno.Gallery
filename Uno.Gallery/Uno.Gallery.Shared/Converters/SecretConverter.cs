using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Uno.Toolkit.UI;
using Windows.UI;

namespace Uno.Gallery.Converters
{
	public class SecretConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var shadowContainerSource = value as ShadowCollection;
			var thirdShadow = shadowContainerSource?.ElementAtOrDefault(2);

			if (shadowContainerSource != null && thirdShadow != null)
			{
				var isThirdShadowSuccess = thirdShadow.Color.Equals(Color.FromArgb(255, 34, 157, 252)) && thirdShadow.IsInner == true;

				if (isThirdShadowSuccess && (string)parameter == "HiddenSecret")
				{
					return GetAnswer();
				}
				else if (isThirdShadowSuccess && (string)parameter == "SecretShadowCombination")
				{
					return Visibility.Visible;
				}
				else
				{
					return Visibility.Collapsed;
				}
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

			var answer = a[0].ToString().ToLower();

			return answer;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException("Only one-way conversion is supported.");
	}
}

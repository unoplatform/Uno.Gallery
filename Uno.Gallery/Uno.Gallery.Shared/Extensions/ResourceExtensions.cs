using Microsoft.UI.Xaml;
using System;

namespace Uno.Gallery
{
	/// <summary>
	/// Helper class for Resources Extensions.
	/// </summary>
	public static class ResourceExtensions
	{
		public static readonly DependencyProperty OverridePathProperty =
		DependencyProperty.RegisterAttached(
			"OverridePath",
			typeof(string),
			typeof(ResourceExtensions),
			new PropertyMetadata(null, OnOverridePathChanged));

		public static void SetOverridePath(FrameworkElement element, string value)
		{
			element.SetValue(OverridePathProperty, value);
		}

		public static string GetOverridePath(FrameworkElement element)
		{
			return (string)element.GetValue(OverridePathProperty);
		}

		private static void OnOverridePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FrameworkElement fe)
			{
				fe.Resources = e.NewValue is string path
					? new ResourceDictionary() { Source = new Uri(path, UriKind.RelativeOrAbsolute) }
					: default;
			}
		}
	}
}

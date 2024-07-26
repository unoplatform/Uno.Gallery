using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Uno.Gallery.Helpers
{
	public static class VisualTreeHelperEx
	{
		public static T GetFirstDescendant<T>(DependencyObject reference) => GetDescendants(reference)
			.OfType<T>()
			.FirstOrDefault();

		public static T GetFirstDescendant<T>(DependencyObject reference, Func<T, bool> predicate) => GetDescendants(reference)
			.OfType<T>()
			.FirstOrDefault(predicate);

		public static IEnumerable<DependencyObject> GetDescendants(DependencyObject reference)
		{
			foreach (var child in GetChildren(reference))
			{
				yield return child;
				foreach (var grandchild in GetDescendants(child))
				{
					yield return grandchild;
				}
			}
		}

		public static IEnumerable<DependencyObject> GetChildren(DependencyObject reference)
		{
			return Enumerable
				.Range(0, VisualTreeHelper.GetChildrenCount(reference))
				.Select(x => VisualTreeHelper.GetChild(reference, x));
		}

		public static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
		{
			while (current != null)
			{
				if (current is T)
				{
					return (T)current;
				}
				current = VisualTreeHelper.GetParent(current);
			}

			throw new InvalidOperationException($"Ancestor of type {typeof(T).Name} not found.");
		}
	}
}

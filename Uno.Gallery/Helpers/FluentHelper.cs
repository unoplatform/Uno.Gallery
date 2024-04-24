using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace Uno.Gallery.Helpers;

internal static class FluentHelper
{
	public static void EnsureXamlControlsResources(this FrameworkElement fe, bool ensureTopPrecedence = false)
	{
		if (fe.Resources is XamlControlsResources) return;
		if (fe.Resources is null)
		{
			fe.Resources = new ResourceDictionary();
		}

		// check only 1st level for performance
		if (ensureTopPrecedence
			? fe.Resources.MergedDictionaries.LastOrDefault() is not XamlControlsResources
			: !fe.Resources.MergedDictionaries.Any(x => x is XamlControlsResources))
		{
			if (ensureTopPrecedence)
			{
				fe.Resources.MergedDictionaries.Insert(0, new XamlControlsResources());
			}
			else
			{
				fe.Resources.MergedDictionaries.Add(new XamlControlsResources());
			}
		}
	}
}

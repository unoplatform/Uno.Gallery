using Windows.ApplicationModel.Resources;

namespace Uno.Gallery.Helpers;

internal static class LocalizationHelper
{
	private static readonly ResourceLoader _resources = ResourceLoader.GetForViewIndependentUse();

	public static string GetString(string resourceKey, string fallback)
	{
		var value = _resources.GetString(resourceKey);
		return string.IsNullOrEmpty(value) ? fallback : value;
	}
}

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Views;

namespace Uno.Gallery
{
	[Activity(
			MainLauncher = true,
			ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
			WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden
		)]
	[IntentFilter(new[] { Android.Content.Intent.ActionView },
		Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
		DataScheme = "https",
		DataHost = "unogallery.app.link",
		AutoVerify = true)]
	public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
	{
	}
}


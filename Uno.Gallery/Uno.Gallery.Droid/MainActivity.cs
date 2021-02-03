using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Views;

namespace Uno.Gallery.Droid
{
	[Activity(
			MainLauncher = true,
			ConfigurationChanges = Uno.UI.ActivityHelper.AllConfigChanges,
			WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden,
			ScreenOrientation = ScreenOrientation.Portrait
		)]
	public class MainActivity : Windows.UI.Xaml.ApplicationActivity
	{
	}
}


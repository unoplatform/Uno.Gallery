using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Views;
using BranchXamarinSDK;
using Android.Content;
using Uno.Gallery.Deeplinking;

namespace Uno.Gallery.Droid
{
	[Activity(
			MainLauncher = true,
			ConfigurationChanges = Uno.UI.ActivityHelper.AllConfigChanges,
			WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden
		)]
	[IntentFilter(new[] { Android.Content.Intent.ActionView },
		Categories = new[] { Android.Content.Intent.CategoryBrowsable, Android.Content.Intent.CategoryDefault },
		DataScheme = "https",
		DataHost = "unogallery.app.link",
		AutoVerify = true)]
	public class MainActivity : Windows.UI.Xaml.ApplicationActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			BranchAndroid.Init(this, BranchService.BranchKey, BranchService.Instance);
		}

		// Ensure we get the updated link identifier when the app becomes active
		// due to a Branch link click after having been in the background
		protected override void OnNewIntent(Intent intent)
		{
			this.Intent = intent;
		}
	}
}


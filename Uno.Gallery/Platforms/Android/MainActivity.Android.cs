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
	public class MainActivity : Uno.UI.Runtime.Android.ApplicationActivity
	{
		// Android can't set environment variables for an app started with `adb shell am start`, so the runtime tests script
		// passes them as intent extras (see build/scripts/android-runtime-tests.sh). Only these keys are copied.
		private static readonly string[] RuntimeTestVariables =
		{
			"UITEST_RUNTIME_AUTOSTART_RESULT_FILE",
			"UITEST_RUNTIME_TESTS_FILTER",
		};

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			// Before base.OnCreate, which starts the app that reads them.
			if (Intent?.Extras is { } extras)
			{
				foreach (var key in RuntimeTestVariables)
				{
					if (extras.GetString(key) is { } value)
					{
						System.Environment.SetEnvironmentVariable(key, value);
					}
				}

				// The results go to the app's external files dir, and only the platform API can create that directory.
				if (extras.ContainsKey("UITEST_RUNTIME_AUTOSTART_RESULT_FILE"))
				{
					_ = GetExternalFilesDir(null);
				}
			}

			base.OnCreate(savedInstanceState);
		}
	}
}

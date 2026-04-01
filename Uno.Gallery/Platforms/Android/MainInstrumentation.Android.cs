using Android.App;
using Android.OS;
using Android.Runtime;

namespace Uno.Gallery
{
	[Instrumentation(Name = "my.MainInstrumentation")]
	public class MainInstrumentation : Android.App.Instrumentation
	{
		public MainInstrumentation(IntPtr handle, JniHandleOwnership ownership)
		    : base(handle, ownership)
		{
		}

		public override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Finish(Result.Ok, null);
		}
	}
}

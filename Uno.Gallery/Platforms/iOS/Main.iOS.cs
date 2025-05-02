using UIKit;
using Uno.UI.Hosting;

namespace Uno.Gallery
{
	public class EntryPoint
{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
#if !IS_UIKIT_SKIA
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, typeof(App));
#else
			var host = UnoPlatformHostBuilder.Create()
				.App(() => new App())
				.UseAppleUIKit()
				.Build();

			host.Run();
#endif
		}
	}
}

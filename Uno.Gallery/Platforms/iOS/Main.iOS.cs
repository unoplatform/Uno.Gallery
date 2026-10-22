using UIKit;
using Uno.UI.Hosting;

namespace Uno.Gallery
{
	public class EntryPoint
{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
			var host = UnoPlatformHostBuilder.Create()
				.App(() => new App())
				.UseAppleUIKit()
				.Build();

			host.Run();
		}
	}
}

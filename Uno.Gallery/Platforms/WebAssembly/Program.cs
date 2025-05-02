using Microsoft.UI.Text;
using System;
using Uno.UI.Hosting;
using Uno.UI.Xaml.Media;

namespace Uno.Gallery.Wasm
{
	public class Program
	{
		private static App _app;

#if IS_WASM_SKIA
		static async Task Main(string[] args)
#else
		static int Main(string[] args)
#endif
		{
			// Ask the browser to preload these fonts to avoid relayouting content
#if IS_WASM_SKIA
			// Disabled https://github.com/unoplatform/uno-private/issues/777
			// FontFamilyHelper.PreloadAsync("Symbols", FontWeights.Normal, Windows.UI.Text.FontStretch.Normal, Windows.UI.Text.FontStyle.Normal);
#else
			FontFamilyHelper.PreloadAsync("Symbols");
#endif

#if IS_WASM_SKIA
			var host = UnoPlatformHostBuilder.Create()
				.App(() => new App())
				.UseWebAssembly()
				.Build();

			await host.RunAsync();
#else
            Microsoft.UI.Xaml.Application.Start(_ => _app = new App());
			return 0;
#endif
		}
	}
}

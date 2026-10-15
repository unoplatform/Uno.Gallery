using Microsoft.UI.Text;
using System;
using Uno.UI.Hosting;
using Uno.UI.Xaml.Media;

namespace Uno.Gallery.Wasm
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			// Ask the browser to preload these fonts to avoid relayouting content
			// Disabled https://github.com/unoplatform/uno-private/issues/777
			// FontFamilyHelper.PreloadAsync("Symbols", FontWeights.Normal, Windows.UI.Text.FontStretch.Normal, Windows.UI.Text.FontStyle.Normal);

			var host = UnoPlatformHostBuilder.Create()
				.App(() => new App())
				.UseWebAssembly()
				.Build();

			await host.RunAsync();
		}
	}
}

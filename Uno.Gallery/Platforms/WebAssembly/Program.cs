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
#if GALLERY_WEBGPU
				// WebGPU canary: WebGPU draws the UI instead of Skia, over the managed geometry engine, as in Uno's SamplesApp
				.GraphicsBackend(global::Uno.UI.Composition.WebGpu.WebGpuBackend.CreateGraphicsProvider())
				.GeometryFactory(global::Uno.UI.Composition.Managed.ManagedBackend.CreateGeometryFactory())
#endif
				.Build();

			await host.RunAsync();
		}
	}
}

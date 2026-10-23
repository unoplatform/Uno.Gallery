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
#if GALLERY_WEBGPU
				// WebGPU canary: WebGPU draws the UI instead of Skia, over the managed geometry engine, as in Uno's SamplesApp
				.GraphicsBackend(global::Uno.UI.Composition.WebGpu.WebGpuBackend.CreateGraphicsProvider())
				.GeometryFactory(global::Uno.UI.Composition.Managed.ManagedBackend.CreateGeometryFactory())
#endif
				.Build();

			host.Run();
		}
	}
}

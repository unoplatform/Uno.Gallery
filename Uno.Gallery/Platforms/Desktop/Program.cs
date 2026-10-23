using Uno.UI.Hosting;

namespace Uno.Gallery;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var exit = args.Any(a => a == "--exit");

        App.InitializeLogging();

        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App(exit))
            .UseX11()
            .UseLinuxFrameBuffer()
            .UseMacOS()
            .UseWin32()
#if GALLERY_WEBGPU
            // WebGPU canary: WebGPU draws the UI instead of Skia, over the managed geometry engine, as in Uno's SamplesApp
            .GraphicsBackend(global::Uno.UI.Composition.WebGpu.WebGpuBackend.CreateGraphicsProvider())
            .GeometryFactory(global::Uno.UI.Composition.Managed.ManagedBackend.CreateGeometryFactory())
#endif
            .Build();

        host.Run();
    }
}

using Uno.UI.Runtime.Skia;

namespace Uno.Gallery;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = SkiaHostBuilder.Create()
            .App(() => new App())
            .UseX11()
            .UseLinuxFrameBuffer()
            .UseMacOS()
#if HAS_SKIA_RENDERER || true
            .UseWin32()
#else
            .UseWindows()
#endif
            .Build();

        host.Run();
    }
}

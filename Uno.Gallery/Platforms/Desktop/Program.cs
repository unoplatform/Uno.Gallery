using Uno.UI.Hosting;
using Uno.UI.Runtime.Skia;

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
            .Build();

        host.Run();
    }
}

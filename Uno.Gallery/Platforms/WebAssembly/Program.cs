namespace Uno.Gallery;
using Uno.UI.Xaml.Media;

public class Program
{
    private static App? _app;

    public static int Main(string[] args)
    {
        // Ask the browser to preload these fonts to avoid relayouting content
		FontFamilyHelper.PreloadAsync("Symbols");

        Microsoft.UI.Xaml.Application.Start(_ => _app = new App());

        return 0;
    }
}

using System;
using Uno.UI.Xaml.Media;

namespace Uno.Gallery.Wasm
{
	public class Program
	{
		private static App _app;

		static int Main(string[] args)
		{
			// Ask the browser to preload these fonts to avoid relayouting content
            FontFamilyHelper.PreloadAsync("ms-appx:///Assets/Fonts/Material/Roboto-Light.ttf#Roboto");
            FontFamilyHelper.PreloadAsync("ms-appx:///Assets/Fonts/Material/Roboto-Medium.ttf#Roboto");
            FontFamilyHelper.PreloadAsync("ms-appx:///Assets/Fonts/Material/Roboto-Regular.ttf#Roboto");
            FontFamilyHelper.PreloadAsync("Symbols");

            Windows.UI.Xaml.Application.Start(_ => _app = new App());

			return 0;
		}
	}
}

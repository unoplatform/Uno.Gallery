using System.Runtime.InteropServices.JavaScript;

namespace Uno.Gallery.Wasm
{
	internal sealed partial class LocationHrefNavigationHandler
	{
		internal static partial class NativeMethods
		{
			private const string JsType = "globalThis.Uno.Gallery.Wasm.LocationHrefNavigation";

			[JSImport($"{JsType}.getCurrentLocationHref")]
			internal static partial string GetCurrentLocationHref();

			[JSImport($"{JsType}.setCurrentLocationHref")]
			internal static partial string SetCurrentLocationHref(string locationHref);
		}
	}
}

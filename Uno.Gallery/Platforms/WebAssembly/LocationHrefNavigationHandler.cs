using Uno.Foundation;

namespace Uno.Gallery.Wasm
{
	internal partial class LocationHrefNavigationHandler
	{
		public static string CurrentLocationHref
		{
			get
			{
				return NativeMethods.GetCurrentLocationHref();
			}

			set
			{
				var escaped = WebAssemblyRuntime.EscapeJs(value);
				NativeMethods.SetCurrentLocationHref(escaped);
			}
		}
	}
}

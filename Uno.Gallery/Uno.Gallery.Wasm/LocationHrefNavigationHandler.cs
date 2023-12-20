using Uno.Foundation;

namespace Uno.Gallery.Wasm
{
	public class LocationHrefNavigationHandler
	{
		public static string CurrentLocationHref
		{
			get
			{
				const string command = "Uno.Gallery.Wasm.LocationHrefNavigationHandler.getCurrentLocationHref()";
				var locationHref = WebAssemblyRuntime.InvokeJS(command);
				return locationHref;
			}

			set
			{
				var escaped = WebAssemblyRuntime.EscapeJs(value);
				var command =
					"Uno.Gallery.Wasm.LocationHrefNavigationHandler.setCurrentLocationHref(\"" + escaped + "\")";

				WebAssemblyRuntime.InvokeJS(command);
			}
		}
	}
}

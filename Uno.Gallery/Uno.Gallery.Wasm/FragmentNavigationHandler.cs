using Uno.Foundation;

namespace Uno.Gallery.Wasm
{
	public class FragmentNavigationHandler
	{
		public static string CurrentFragment
		{
			get
			{
				const string command = "Uno.Gallery.Wasm.FragmentNavigationHandler.getCurrentFragment()";
				var fragment = WebAssemblyRuntime.InvokeJS(command);
				return fragment;
			}

			set
			{
				var escaped = WebAssemblyRuntime.EscapeJs(value);
				var command =
					"Uno.Gallery.Wasm.FragmentNavigationHandler.setCurrentFragment(\"" + escaped + "\")";

				WebAssemblyRuntime.InvokeJS(command);
			}
		}
	}
}

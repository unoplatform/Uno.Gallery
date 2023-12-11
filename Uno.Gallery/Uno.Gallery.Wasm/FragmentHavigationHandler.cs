using Uno.Foundation;

namespace Uno.Gallery.Wasm
{
	public class FragmentHavigationHandler
	{
		public static string CurrentFragment
		{
			get
			{
				const string command = "Uno.UI.FragmentNavigationHandler.getCurrentFragment()";
				var fragment = WebAssemblyRuntime.InvokeJS(command);
				return fragment;
			}

			set
			{
				var escaped = WebAssemblyRuntime.EscapeJs(value);
				var command =
					"Uno.UI.FragmentNavigationHandler.setCurrentFragment(\"" + escaped + "\")";

				WebAssemblyRuntime.InvokeJS(command);
			}
		}
	}
}

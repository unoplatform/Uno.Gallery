using Uno.Foundation;

namespace Uno.Gallery.Wasm
{
	internal partial class FragmentNavigationHandler
	{
		public static string CurrentFragment
		{
			get
			{
				return NativeMethods.GetCurrentFragment();
			}

			set
			{
				var escaped = WebAssemblyRuntime.EscapeJs(value);
				NativeMethods.SetCurrentFragment(escaped);
			}
		}
	}
}

using System.Runtime.InteropServices.JavaScript;

namespace Uno.Gallery.Wasm
{
	internal sealed partial class FragmentNavigationHandler
	{
		internal static partial class NativeMethods
		{
			private const string JsType = "globalThis.Uno.Gallery.Wasm.FragmentNavigation";

			[JSImport($"{JsType}.getCurrentFragment")]
			internal static partial string GetCurrentFragment();

			[JSImport($"{JsType}.setCurrentFragment")]
			internal static partial string SetCurrentFragment(string fragment);
		}
	}
}

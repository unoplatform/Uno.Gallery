using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.Gallery
{
	[Flags]
	public enum SampleConditionals
	{
		Windows = 1 << 0,
		Wasm = 1 << 1,
		SkiaGtk = 1 << 2,
		Droid = 1 << 3,
		iOS = 1 << 4,
		macOS = 1 << 5,

		Desktop = Windows | Wasm | SkiaGtk | macOS,
		Mobile = Droid | iOS,
		SkiaBased = Wasm | SkiaGtk,
		
		Disabled = 1 << 31,
		Always = int.MaxValue ^ Disabled,
	}
}

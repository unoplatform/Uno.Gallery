using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.Gallery
{
	[Flags]
	public enum SampleConditionals : uint
	{
		Android = 1 << 0,
		iOS = 1 << 1,
		Windows = 1 << 2,
		Desktop = 1 << 3,
		BrowserWasm = 1 << 4,

		Mobile = Android | iOS,

		Disabled = 1U << 31,
		Always = uint.MaxValue ^ Disabled,
	}
}

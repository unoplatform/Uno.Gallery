using System;

namespace Uno.Gallery
{
	/// <summary>
	/// Rendering backends on which a sample is intentionally supported.
	/// Values are stable manifest identifiers; do not reorder or renumber.
	/// </summary>
	[Flags]
	public enum SampleRenderers
	{
		None = 0,
		Native = 1 << 0,
		Skia = 1 << 1,
		DOM = 1 << 2,
	}
}

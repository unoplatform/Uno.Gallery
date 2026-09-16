using System;

namespace Uno.Gallery
{
	/// <summary>
	/// Design systems intentionally demonstrated by a sample.
	/// Values are stable manifest identifiers; do not reorder or renumber.
	/// </summary>
	[Flags]
	public enum SampleDesigns
	{
		None = 0,
		Material = 1 << 0,
		Fluent = 1 << 1,
		Cupertino = 1 << 2,
		Native = 1 << 3,
		Agnostic = 1 << 4,
	}
}

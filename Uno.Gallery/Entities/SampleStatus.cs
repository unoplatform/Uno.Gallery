namespace Uno.Gallery
{
	/// <summary>
	/// Describes the production-readiness of a sample.
	/// Values are stable numeric identifiers; do not reorder or renumber.
	/// </summary>
	public enum SampleStatus
	{
		/// <summary>Feature is available and supported on all listed platforms.</summary>
		Stable = 0,

		/// <summary>Feature is usable but the API or behavior may still change.</summary>
		Preview = 1,

		/// <summary>Feature is under active investigation; use with caution.</summary>
		Experimental = 2,

		/// <summary>Feature is superseded or removed and kept only for reference.</summary>
		Deprecated = 3,

		/// <summary>Sample is a work-in-progress; coverage or functionality is incomplete.</summary>
		Incomplete = 4,
	}
}

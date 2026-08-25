namespace Uno.Gallery;

/// <summary>Event data for <see cref="IGalleryNavigator.Navigated"/>.</summary>
internal sealed class SampleNavigatedEventArgs : EventArgs
{
	/// <param name="previous">The sample active before this navigation, or null on the first navigation.</param>
	/// <param name="current">
	/// The sample just navigated to.
	/// Normally non-null after navigation; null only during initial shell setup before the
	/// first navigation completes.
	/// </param>
	public SampleNavigatedEventArgs(Sample? previous, Sample? current)
	{
		Previous = previous;
		Current = current;
	}

	/// <summary>The sample active before this navigation, or null on the first navigation.</summary>
	public Sample? Previous { get; }

	/// <summary>
	/// The sample just navigated to.
	/// </summary>
	/// <remarks>
	/// Normally non-null after navigation; null only during initial shell setup before the
	/// first navigation completes.
	/// </remarks>
	public Sample? Current { get; }
}

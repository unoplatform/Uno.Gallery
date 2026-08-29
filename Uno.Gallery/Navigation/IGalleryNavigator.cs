namespace Uno.Gallery;

/// <summary>
/// Typed navigation contract for a single <see cref="Shell"/> instance.
/// All members must be called from the UI thread.
/// </summary>
internal interface IGalleryNavigator
{
	/// <summary>The most recently navigated-to sample, or null before the first navigation completes.</summary>
	Sample? Current { get; }

	/// <summary>Raised after each successful navigation. Not raised for same-page no-ops.</summary>
	event EventHandler<SampleNavigatedEventArgs> Navigated;

	/// <summary>
	/// Navigate to <paramref name="sample"/>.
	/// No-op when <paramref name="sample"/>'s page type is already displayed.
	/// </summary>
	void NavigateTo(Sample sample, NavigationOptions options = NavigationOptions.None);

	/// <summary>
	/// Navigate to the sample whose <see cref="Sample.Slug"/> matches <paramref name="slug"/>
	/// (case-insensitive). Returns <see langword="false"/> if no matching sample exists.
	/// </summary>
	bool NavigateToSlug(string slug, NavigationOptions options = NavigationOptions.None);

	/// <summary>Navigate to the Overview landing page.</summary>
	void NavigateToOverview(NavigationOptions options = NavigationOptions.None);

	/// <summary>
	/// Return the catalog sample whose <see cref="Sample.Slug"/> matches <paramref name="slug"/>
	/// (case-insensitive), or <see langword="null"/> if none is found.
	/// </summary>
	Sample? FindBySlug(string slug);

	/// <summary>
	/// Return the catalog sample whose <see cref="Sample.Title"/> matches <paramref name="title"/>
	/// (case-insensitive), or <see langword="null"/> if none is found.
	/// </summary>
	Sample? FindByTitle(string title);
}

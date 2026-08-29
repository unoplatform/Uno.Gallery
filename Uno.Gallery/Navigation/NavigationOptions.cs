namespace Uno.Gallery;

/// <summary>Options that control navigation behavior in <see cref="IGalleryNavigator"/>.</summary>
[Flags]
internal enum NavigationOptions
{
	/// <summary>Default: sync NavigationView selection without expanding categories.</summary>
	None = 0,

	/// <summary>
	/// Skip NavigationView selection synchronization.
	/// Use when the item was already invoked (e.g. <c>ItemInvoked</c> handler) or during startup navigation.
	/// </summary>
	SkipNavSync = 1 << 0,

	/// <summary>
	/// Expand the parent category and call <c>UpdateLayout</c> before syncing selection.
	/// Required on the search path so collapsed nested items are materialized before selection.
	/// </summary>
	ExpandCategory = 1 << 1,

	/// <summary>
	/// Skip browser history push on WASM.
	/// Use for startup navigation and browser back/forward callbacks to avoid double-recording entries.
	/// No-op on non-WASM targets.
	/// </summary>
	SkipHistory = 1 << 2,
}

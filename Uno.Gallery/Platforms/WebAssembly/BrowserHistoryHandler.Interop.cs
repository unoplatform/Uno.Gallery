using System.Runtime.InteropServices.JavaScript;

namespace Uno.Gallery.Wasm;

/// <summary>
/// Managed proxy for <c>Uno.Gallery.Wasm.BrowserHistory</c> (BrowserHistory.ts).
/// Wraps push/replace/subscribe history operations for WASM URL synchronization.
/// <para>
/// All public members delegate to <see cref="NativeMethods"/>; <see cref="Subscribe"/>
/// also stores the delegate in a static GC root so the JS listener cannot outlive it.
/// </para>
/// </summary>
internal sealed partial class BrowserHistoryHandler
{
	// Static GC root: prevents the delegate (and any managed objects it closes over)
	// from being collected while the JS event listener is active.
	private static Action<string>? _subscribedCallback;

	/// <summary>Returns the current URL hash without the leading '#', or empty string.</summary>
	internal static string GetHash() => NativeMethods.GetHash();

	/// <summary>Returns the current <c>design</c> query-string value, or empty string.</summary>
	internal static string GetDesign() => NativeMethods.GetDesign();

	/// <summary>
	/// Registers <paramref name="callback"/> for popstate/hashchange events.
	/// Pass <see langword="null"/> to unsubscribe. The delegate is rooted until the next call.
	/// The callback receives <c>"slug\ndesign"</c> where <c>slug</c> is the decoded hash
	/// (without '#') and <c>design</c> is the <c>?design=</c> query value.
	/// </summary>
	internal static void Subscribe(Action<string>? callback)
	{
		_subscribedCallback = callback; // root before crossing the JS boundary
		NativeMethods.Subscribe(callback);
	}

	/// <summary>
	/// Pushes a new browser history entry with canonical URL <c>?design=&lt;design&gt;#&lt;slug&gt;</c>.
	/// </summary>
	internal static void PushState(string slug, string design) =>
		NativeMethods.PushState(slug, design);

	/// <summary>
	/// Replaces the current browser history entry without emitting a history event.
	/// </summary>
	internal static void ReplaceState(string slug, string design) =>
		NativeMethods.ReplaceState(slug, design);

	/// <summary>
	/// Updates only the <c>?design=</c> query parameter without adding a history entry.
	/// </summary>
	internal static void ReplaceDesign(string design) =>
		NativeMethods.ReplaceDesign(design);

	private static partial class NativeMethods
	{
		private const string JsType = "globalThis.Uno.Gallery.Wasm.BrowserHistory";

		[JSImport($"{JsType}.getHash")]
		internal static partial string GetHash();

		[JSImport($"{JsType}.getDesign")]
		internal static partial string GetDesign();

		[JSImport($"{JsType}.subscribe")]
		internal static partial void Subscribe(
			[JSMarshalAs<JSType.Function<JSType.String>>] Action<string>? callback);

		[JSImport($"{JsType}.pushState")]
		internal static partial void PushState(string slug, string design);

		[JSImport($"{JsType}.replaceState")]
		internal static partial void ReplaceState(string slug, string design);

		[JSImport($"{JsType}.replaceDesign")]
		internal static partial void ReplaceDesign(string design);
	}
}

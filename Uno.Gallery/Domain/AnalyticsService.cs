namespace Uno.Gallery
{
	public static class AnalyticsService
	{
		public static void Initialize()
		{
#if !DEBUG && __WASM__
			Interop.ReportPageView("main");
#endif
		}

		public static void TrackView(string viewName)
		{
#if !DEBUG && __WASM__
			Interop.ReportPageView(viewName);
#endif
		}
	}

#if !DEBUG && __WASM__
	static partial class Interop
	{
		[System.Runtime.InteropServices.JavaScript.JSImport("globalThis.Uno.UI.Demo.Analytics.reportPageView")]
		internal static partial void ReportPageView(string page);
	}
#endif
}

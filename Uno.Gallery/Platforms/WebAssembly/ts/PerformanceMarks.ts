namespace Uno.Gallery.Wasm {
	export class PerformanceMarks {

		/**
		 * Records a browser Performance API mark for the given name.
		 * Uses the modern performance.mark() API; no-op when the API is unavailable.
		 * Only called from managed code in PERF_MEASUREMENTS-enabled builds.
		 *
		 * All recorded marks are available externally via:
		 *   performance.getEntriesByType('mark').filter(e => e.name.startsWith('app.'))
		 */
		public static mark(name: string): void {
			if (typeof performance !== "undefined" && typeof performance.mark === "function") {
				performance.mark(name);
			}
		}
	}
}

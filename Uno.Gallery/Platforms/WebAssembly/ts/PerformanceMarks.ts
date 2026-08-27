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

		/**
		 * Adds an app-owned duration ending at the current browser timestamp.
		 */
		public static measure(name: string, durationMs: number): void {
			if (typeof performance !== "undefined" && typeof performance.measure === "function") {
				const duration = Math.max(0, durationMs);
				performance.measure(name, {
					start: Math.max(0, performance.now() - duration),
					duration
				});
			}
		}
	}
}

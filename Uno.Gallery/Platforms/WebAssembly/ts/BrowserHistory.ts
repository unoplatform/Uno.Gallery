namespace Uno.Gallery.Wasm {
	export class BrowserHistory {

		// Tracks the full href to deduplicate popstate + hashchange events for the same navigation.
		private static _currentHref: string = "";
		private static _callback: ((state: string) => void) | null = null;
		private static _boundListener: (() => void) | null = null;

		/**
		 * Returns the current URL hash without the leading '#', or empty string when there is no hash.
		 */
		public static getHash(): string {
			const h = window.location.hash;
			return h.startsWith("#") ? h.substring(1) : h;
		}

		/**
		 * Returns the current 'design' query-parameter value, or empty string when absent.
		 */
		public static getDesign(): string {
			return new URLSearchParams(window.location.search).get("design") ?? "";
		}

		/**
		 * Subscribes to both popstate and hashchange events.
		 * Only one callback is stored at a time; pass null to unsubscribe.
		 * The callback receives "slug\ndesign" where slug is the hash (without '#')
		 * and design is the value of the '?design=' query parameter.
		 */
		public static subscribe(callback: ((state: string) => void) | null): void {
			if (this._boundListener !== null) {
				window.removeEventListener("popstate", this._boundListener);
				window.removeEventListener("hashchange", this._boundListener);
				this._boundListener = null;
			}

			this._callback = callback;

			if (callback !== null) {
				this._currentHref = window.location.href;
				this._boundListener = () => this._onStateChange();
				window.addEventListener("popstate", this._boundListener);
				window.addEventListener("hashchange", this._boundListener);
			}
		}

		private static _onStateChange(): void {
			const newHref = window.location.href;
			if (newHref === this._currentHref) {
				return; // deduplicate: popstate + hashchange can both fire for the same navigation
			}
			this._currentHref = newHref;

			if (this._callback !== null) {
				const hash = this.getHash();
				const design = this.getDesign();
				this._callback(hash + "\n" + design);
			}
		}

		/**
		 * Pushes a new browser history entry with canonical URL: ?design=<design>#<slug>.
		 * Overview is represented as slug "overview".
		 */
		public static pushState(slug: string, design: string): void {
			const effectiveSlug = slug || "overview";
			const url = this._buildUrl(effectiveSlug, design);
			history.pushState({ slug: effectiveSlug, design }, "", url);
			this._currentHref = window.location.href;
		}

		/**
		 * Replaces the current browser history entry without emitting a history event.
		 * Used for URL canonicalization on startup and design-only updates on navigation.
		 */
		public static replaceState(slug: string, design: string): void {
			const effectiveSlug = slug || "overview";
			const url = this._buildUrl(effectiveSlug, design);
			history.replaceState({ slug: effectiveSlug, design }, "", url);
			this._currentHref = window.location.href;
		}

		/**
		 * Updates only the ?design= query parameter in the current URL, keeping the hash unchanged.
		 * Does not add a history entry. Safe to call before subscription is established.
		 */
		public static replaceDesign(design: string): void {
			const slug = this.getHash() || "overview";
			this.replaceState(slug, design);
		}

		private static _buildUrl(slug: string, design: string): string {
			return "?design=" + encodeURIComponent(design) + "#" + encodeURIComponent(slug);
		}
	}
}

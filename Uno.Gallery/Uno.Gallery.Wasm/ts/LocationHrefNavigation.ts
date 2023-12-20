namespace Uno.Gallery.Wasm {
	export class LocationHrefNavigationHandler {

		private static currentLocationHref: string;

		public static getCurrentLocationHref(): string {
			return window.location.href;
		}

		public static setCurrentLocationHref(locationHref: string): string {
			window.location.href = locationHref;
			this.currentLocationHref = window.location.href;

			return "ok";
		}

		private static subscribed: boolean = false;

		public static subscribeToLocationHrefChanged(): string {

			if (this.subscribed) {
				return "already subscribed";
			}

			this.subscribed = true;

			this.currentLocationHref = this.getCurrentLocationHref();

			window.addEventListener(
				"hashchange",
				_ => this.notifyLocationHrefChanged(),
				false);

			return "ok";
		}

		private static notifyLocationHrefChangedMethod: any;

		private static notifyLocationHrefChanged() {
			const newLocationHref: string = this.getCurrentLocationHref();
			if (newLocationHref === this.currentLocationHref) {
				return;  // nothing to do
			}

			this.currentLocationHref = newLocationHref;

			this.initializeMethods();
			const newLocationHrefStr = MonoRuntime.mono_string(newLocationHref);
			MonoRuntime.call_method(this.notifyLocationHrefChangedMethod, null, [newLocationHrefStr]);
		}

		private static initializeMethods(): void {
			if (this.notifyLocationHrefChangedMethod) {
				return; // already initialized.
			}

			const asm = MonoRuntime.assembly_load("Uno.Gallery.WASM");
			const handlerClass = MonoRuntime.find_class(asm, "Uno.UI.Wasm", "LocationHrefHavigationHandler");
			this.notifyLocationHrefChangedMethod = MonoRuntime.find_method(handlerClass, "NotifyLocationHrefChanged", -1);
		}
	}

	declare var MonoRuntime: any;
}

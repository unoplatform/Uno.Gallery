var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        class FragmentNavigationHandler {
            static getCurrentFragment() {
                return window.location.hash;
            }
            static setCurrentFragment(fragment) {
                window.location.hash = fragment;
                this.currentFragment = window.location.hash;
                return "ok";
            }
            static subscribeToFragmentChanged() {
                if (this.subscribed) {
                    return "already subscribed";
                }
                this.subscribed = true;
                this.currentFragment = this.getCurrentFragment();
                window.addEventListener("hashchange", _ => this.notifyFragmentChanged(), false);
                return "ok";
            }
            static notifyFragmentChanged() {
                const newFragment = this.getCurrentFragment();
                if (newFragment === this.currentFragment) {
                    return; // nothing to do
                }
                this.currentFragment = newFragment;
                this.initializeMethods();
                const newFragmentStr = MonoRuntime.mono_string(newFragment);
                MonoRuntime.call_method(this.notifyFragmentChangedMethod, null, [newFragmentStr]);
            }
            static initializeMethods() {
                if (this.notifyFragmentChangedMethod) {
                    return; // already initialized.
                }
                const asm = MonoRuntime.assembly_load("Uno.Gallery.WASM");
                const handlerClass = MonoRuntime.find_class(asm, "Uno.UI.Wasm", "FragmentHavigationHandler");
                this.notifyFragmentChangedMethod = MonoRuntime.find_method(handlerClass, "NotifyFragmentChanged", -1);
            }
        }
        FragmentNavigationHandler.subscribed = false;
        UI.FragmentNavigationHandler = FragmentNavigationHandler;
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Demo;
        (function (Demo) {
            class Analytics {
                static reportPageView(screenName, appName = "Uno.Gallery") {
                    if (Analytics.init(screenName, appName)) {
                        return "ok";
                    }
                    const gtag = window.gtag;
                    if (gtag) {
                        gtag("event", "screen_view", {
                            screen_name: screenName,
                            app_name: appName
                        });
                    }
                    else {
                        console.error(`Google Analytics not present, can't report page view for ${screenName}.`);
                    }
                    return "ok";
                }
                static init(screenName, appName) {
                    if (Analytics.isLoaded) {
                        return false;
                    }
                    const script = `
	            window.dataLayer = window.dataLayer || [];
	            function gtag() { dataLayer.push(arguments); }
	            gtag('js', new Date());
	            gtag('config', 'G-GTQK0EEP2J');
	            gtag("event", "screen_view", {screen_name: \"${screenName}\", app_name: \"${appName}\"});`;
                    const script1 = document.createElement("script");
                    script1.type = "text/javascript";
                    script1.src = "https://www.googletagmanager.com/gtag/js?id=G-GTQK0EEP2J";
                    document.body.appendChild(script1);
                    const script2 = document.createElement("script");
                    script2.type = "text/javascript";
                    script2.innerText = script;
                    document.body.appendChild(script2);
                    Analytics.isLoaded = true;
                    return true;
                }
            }
            Analytics.isLoaded = false;
            Demo.Analytics = Analytics;
        })(Demo = UI.Demo || (UI.Demo = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));

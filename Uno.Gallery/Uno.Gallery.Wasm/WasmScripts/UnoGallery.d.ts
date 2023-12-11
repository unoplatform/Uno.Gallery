declare namespace Uno.UI {
    class FragmentNavigationHandler {
        private static currentFragment;
        static getCurrentFragment(): string;
        static setCurrentFragment(fragment: string): string;
        private static subscribed;
        static subscribeToFragmentChanged(): string;
        private static notifyFragmentChangedMethod;
        private static notifyFragmentChanged;
        private static initializeMethods;
    }
}
declare namespace Uno.UI.Demo {
    class Analytics {
        private static isLoaded;
        static reportPageView(screenName: string, appName?: string): string;
        private static init;
    }
}

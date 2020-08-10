using AppKit;
using Uno.Gallery;

namespace Uno.Gallery.macOS
{
	internal static class MenuHelper
	{
		private const string QuitTitle = "Quit";
		private const string QuitCharCode = "q";

		internal static NSMenu GetMenu()
		{
			var menubar = new NSMenu();
			var appMenuItem = new NSMenuItem();
			menubar.AddItem(appMenuItem);

			var appMenu = new NSMenu();

			var quitMenuItem = new NSMenuItem(QuitTitle, QuitCharCode, delegate
			{
				NSApplication.SharedApplication.Terminate(menubar);
			});

			appMenu.AddItem(quitMenuItem);
			appMenuItem.Submenu = appMenu;

			return menubar;
		}
	}
}
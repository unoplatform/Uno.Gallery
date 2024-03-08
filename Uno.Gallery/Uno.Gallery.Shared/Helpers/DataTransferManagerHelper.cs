using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using WinRT.Interop;
using WinRT;

namespace Uno.Gallery.Helpers
{
	public static class DataTransferManagerHelper
	{
		private static readonly Guid _dtm_iid = new Guid("a5caee9b-8708-49d1-8d36-67d25a8da00c");

#if WINDOWS
		static IDataTransferManagerInterop DataTransferManagerInterop => DataTransferManager.As<IDataTransferManagerInterop>();
#endif

		public static DataTransferManager GetForCurrentView()
		{
#if WINDOWS
			IntPtr result;
			var hwnd = WindowNative.GetWindowHandle(App.Instance.MainWindow);
			result = DataTransferManagerInterop.GetForWindow(hwnd, _dtm_iid);
			DataTransferManager dataTransferManager = MarshalInterface<DataTransferManager>.FromAbi(result);
			return (dataTransferManager);
#else
			return DataTransferManager.GetForCurrentView();
#endif
		}

		public static void ShowShareUI(ShareUIOptions options = null)
		{
#if WINDOWS
			var hwnd = WindowNative.GetWindowHandle(App.Instance.MainWindow);
			DataTransferManagerInterop.ShowShareUIForWindow(hwnd, options);
#else
			DataTransferManager.ShowShareUI(options);
#endif
		}

#if WINDOWS
		[ComImport]
		[Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IDataTransferManagerInterop
		{
			IntPtr GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);
			void ShowShareUIForWindow(IntPtr appWindow, ShareUIOptions options);
		}
#endif
	}
}

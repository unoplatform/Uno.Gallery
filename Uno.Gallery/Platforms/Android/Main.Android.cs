using System;
using System.Diagnostics.CodeAnalysis;
using Android.App;
using Android.Runtime;
using Uno.UI.Hosting;

namespace Uno.Gallery.Droid
{
	[global::Android.App.ApplicationAttribute(
		Label = "@string/ApplicationName",
		Icon = "@mipmap/icon",
		LargeHeap = true,
		HardwareAccelerated = true,
		Theme = "@style/AppTheme"
	)]
	public class Application : Uno.UI.Runtime.Android.NativeApplication
	{
		[DynamicDependency(DynamicallyAccessedMemberTypes.All, "Android.Text.ITextWatcherInvoker", "Mono.Android")] // xamarin/xamarin-android#7097
		public Application(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		protected override UnoPlatformHost CreateHost() =>
			UnoPlatformHostBuilder.Create()
				.App(() => new App())
				.UseAndroid()
#if GALLERY_WEBGPU
				// WebGPU canary: WebGPU draws the UI instead of Skia, over the managed geometry engine, as in Uno's SamplesApp
				.GraphicsBackend(global::Uno.UI.Composition.WebGpu.WebGpuBackend.CreateGraphicsProvider())
				.GeometryFactory(global::Uno.UI.Composition.Managed.ManagedBackend.CreateGeometryFactory())
#endif
				.Build();
	}
}

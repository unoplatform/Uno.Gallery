using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUI, "Clipboard", Description = "The clipboard enables copy and paste support in Uno applications.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/uwp/app-to-app/copy-and-paste")]
	public sealed partial class ClipboardSamplePage : Page
	{
		private static readonly Uri _sampleBitmapUri = new Uri("ms-appx:///Assets/UnoLogo.png");
		private const string _sampleHtml = "<!DOCTYPE html><html><body><p>Hello World!</p></body></html>";
		private const string _sampleRtf = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Calibri;}}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1 
\pard\sa200\sl276\slmult1\b\f0\fs22\lang9 Hello World!\par
}";

		public ClipboardSamplePage()
		{
			this.InitializeComponent();
		}

		private void CopyText_Click(object sender, RoutedEventArgs args)
		{
			var package = new DataPackage();
			package.SetText("Hello World!");
			Clipboard.SetContent(package);
		}

		private async void CopyBitmap_Click(object sender, RoutedEventArgs args)
		{
			var package = new DataPackage();
            var sFile = await StorageFile.GetFileFromApplicationUriAsync(_sampleBitmapUri);
            package.SetBitmap(RandomAccessStreamReference.CreateFromFile(sFile));
			Clipboard.SetContent(package);
		}

		private void CopyHtml_Click(object sender, RoutedEventArgs args)
		{
			var package = new DataPackage();
			package.SetHtmlFormat(_sampleHtml);
			Clipboard.SetContent(package);
		}

		private void CopyRtf_Click(object sender, RoutedEventArgs args)
		{
			var package = new DataPackage();
			package.SetRtf(_sampleRtf);
			Clipboard.SetContent(package);
		}

		private async void CopyFile_Click(object sender, RoutedEventArgs args)
		{
#if NETFX_CORE || HAS_UNO_SKIA
			var sFile = await StorageFile.GetFileFromApplicationUriAsync(_sampleBitmapUri);
			var package = new DataPackage();
			package.SetStorageItems(new IStorageItem[] { sFile });
			Clipboard.SetContent(package);
#else
			await System.Threading.Tasks.Task.CompletedTask;
#endif
		}

		private void CopyTextHtmlRtf_Click(object sender, RoutedEventArgs args)
		{
			var package = new DataPackage();
			package.SetText("Hello World!");
			package.SetHtmlFormat(_sampleHtml);
			package.SetRtf(_sampleRtf);
			Clipboard.SetContent(package);
		}

		private async void PasteText_Click(object sender, RoutedEventArgs args)
		{
			var package = Clipboard.GetContent();
			if (package.Contains(StandardDataFormats.Text))
			{
				var dialog = new ContentDialog()
				{
					Title = "Pasted Text",
					Content = new TextBlock() { Text = await package.GetTextAsync() },
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
			else
			{
				var dialog = new ContentDialog()
				{
					Title = "No Text in Clipboard",
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
		}

		private async void PasteBitmap_Click(object sender, RoutedEventArgs args)
		{
			var package = Clipboard.GetContent();
			if (package.Contains(StandardDataFormats.Bitmap))
			{
				var img = new Image();
				var src = new BitmapImage();
				var streamRef = await package.GetBitmapAsync();
				src.SetSource(await streamRef.OpenReadAsync());
				img.Source = src;
				var dialog = new ContentDialog()
				{
					Title = "Pasted Image",
					Content = img,
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
			else
			{
				var dialog = new ContentDialog()
				{
					Title = "No Image in Clipboard",
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
		}

		private async void PasteHtml_Click(object sender, RoutedEventArgs args)
		{
			var package = Clipboard.GetContent();
			if (package.Contains(StandardDataFormats.Html))
			{
				var dialog = new ContentDialog()
				{
					Title = "Pasted HTML",
					Content = new TextBlock() { Text = await package.GetHtmlFormatAsync() },
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
			else
			{
				var dialog = new ContentDialog()
				{
					Title = "No HTML in Clipboard",
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
		}

		private async void PasteRtf_Click(object sender, RoutedEventArgs args)
		{
			var package = Clipboard.GetContent();
			if (package.Contains(StandardDataFormats.Rtf))
			{
#if WINDOWS_UWP
				var content = new RichEditBox();
				content.TextDocument.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, await package.GetRtfAsync());
				content.IsReadOnly = true;
#else
				// TODO: Remove when RichEditBox is supported in Uno.
				var content = new TextBlock() { Text = await package.GetRtfAsync() };
#endif

				var dialog = new ContentDialog()
				{
					Title = "Pasted RTF",
					Content = content,
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
			else
			{
				var dialog = new ContentDialog()
				{
					Title = "No RTF in Clipboard",
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
		}

		private async void PasteFile_Click(object sender, RoutedEventArgs args)
		{
			var package = Clipboard.GetContent();
			if (package.Contains(StandardDataFormats.StorageItems))
			{
				var files = await package.GetStorageItemsAsync();
				var list = string.Join(Environment.NewLine, files.Select(file => file.Path));
				var dialog = new ContentDialog()
				{
					Title = "Pasted Files",
					Content = new TextBlock() { Text = list },
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
			else
			{
				var dialog = new ContentDialog()
				{
					Title = "No File in Clipboard",
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
		}

		private void Flush_Click(object sender, RoutedEventArgs args)
		{
			Clipboard.Flush();
		}

		private void Clear_Click(object sender, RoutedEventArgs args)
		{
			Clipboard.Clear();
		}

		private void Listen_Click(object sender, RoutedEventArgs args)
		{
			if ((sender as Button)?.DataContext is ClipboardSamplePageViewModel viewModel)
			{
				if (viewModel.Listening)
				{
					Clipboard.ContentChanged -= viewModel.Clipboard_ContentChanged;
				}
				else
				{
					Clipboard.ContentChanged += viewModel.Clipboard_ContentChanged;
				}
				viewModel.Listening = !viewModel.Listening;
			}
		}
	}

	public class ClipboardSamplePageViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private int _changeCount = 0;
		public int ChangeCount
		{
			get => _changeCount;
			set
			{
				_changeCount = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChangeCount)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
			}
		}

		private bool _listening;
		public bool Listening
		{
			get => _listening;
			set
			{
				_listening = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Listening)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListenButtonContent)));
			}
		}

		public string Message => $"Clipboard content changed {_changeCount} times.";
		public string ListenButtonContent => $"{(_listening ? "Stop" : "Start")} listening to Clipboard.ContentChanged";

		public void Clipboard_ContentChanged(object sender, object args)
		{
			++ChangeCount;
		}
	}
}

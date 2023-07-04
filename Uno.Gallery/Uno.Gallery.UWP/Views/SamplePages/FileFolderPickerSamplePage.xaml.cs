using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "File and Folder Pickers", Description = "A file picker displays information to orient users and provide a consistent experience when opening or saving files.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/uwp/files/quickstart-using-file-and-folder-pickers")]
	public sealed partial class FileFolderPickerSamplePage : Page
	{
		public FileFolderPickerSamplePage()
		{
			this.InitializeComponent();
		}

		private async void PickFileButton_Click(object sender, RoutedEventArgs e)
		{
			var picker = new FileOpenPicker();
			picker.FileTypeFilter.Add("*");
			var storageFile = await picker.PickSingleFileAsync();

			if (storageFile != null)
			{
				await new MessageDialog(storageFile.Path, "Successfully picked file").ShowAsync();
			}
			else
			{
				await new MessageDialog("You didn't pick any file!", "Failed to pick file").ShowAsync();
			}
		}

		private async void PickMultipleFilesButton_Click(object sender, RoutedEventArgs e)
		{
			var picker = new FileOpenPicker();
			picker.FileTypeFilter.Add("*");
			var storageFiles = await picker.PickMultipleFilesAsync();

			if (storageFiles?.Any() ?? false)
			{
				var fileListString = string.Join(Environment.NewLine, storageFiles.Select(f => f.Path));
				var dialog = new ContentDialog()
				{
					Title = "Successfully picked files",
					Content = new ScrollViewer()
					{
						Content = new TextBlock() { Text = fileListString }
					},
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
			else
			{
				await new MessageDialog("You didn't pick any file!", "Failed to pick files").ShowAsync();
			}
		}

		private async void PickSpecificFilesButton_Click(object sender, RoutedEventArgs e)
		{
			var picker = new FileOpenPicker();
			picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
			picker.FileTypeFilter.Add(".jpg");
			picker.FileTypeFilter.Add(".jpeg");
			picker.FileTypeFilter.Add(".png");
			picker.FileTypeFilter.Add(".bmp");
			var storageFiles = await picker.PickMultipleFilesAsync();

			if (storageFiles?.Any() ?? false)
			{
				var stack = new StackPanel() { Spacing = 10 };
				foreach (var file in storageFiles)
				{
					var bitmap = new BitmapImage();
					using (var stream = await file.OpenReadAsync())
					{
						await bitmap.SetSourceAsync(stream);
					}
					stack.Children.Add(new Image() { Source = bitmap });
				}
				var dialog = new ContentDialog()
				{
					Title = "Successsfully picked images",
					Content = new ScrollViewer() { Content = stack },
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
			else
			{
				await new MessageDialog("You didn't pick any image!", "Failed to pick images").ShowAsync();
			}
		}

		private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
		{
			var folderPicker = new FolderPicker();
			folderPicker.FileTypeFilter.Add("*");

			var storageFolder = await folderPicker.PickSingleFolderAsync();
			if (storageFolder != null)
			{
				var fileList = await storageFolder.GetFilesAsync();
				var folderList = await storageFolder.GetFoldersAsync();

				var contentString = string.Join(Environment.NewLine, 
					fileList.Select(f => f.Path).Concat(folderList.Select(f => f.Path)));
				var dialog = new ContentDialog()
				{
					Title = "Contents of picked folder",
					Content = new ScrollViewer() 
					{ 
						Content = new TextBlock() { Text = contentString } 
					},
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
			else
			{
				await new MessageDialog("You didn't pick any folder!", "Failed to pick folder").ShowAsync();
			}
		}

		private async void SaveFileButton_Click(object sender, RoutedEventArgs e)
		{
			var picker = new FileSavePicker();
			picker.FileTypeChoices.Add("Text files", new[] { ".txt" });
			var storageFile = await picker.PickSaveFileAsync();

			if (storageFile != null)
			{
				var textBox = ((sender as Button).Parent as StackPanel)
					.FindName("ContentTextBox") as TextBox;
				using (var stream = await storageFile.OpenStreamForWriteAsync())
				{
					using (var tw = new StreamWriter(stream))
					{
						tw.WriteLine(textBox?.Text);
					}
				}
				await new MessageDialog(storageFile.Path, "Successfully saved to file").ShowAsync();
			}
			else
			{
				await new MessageDialog("You didn't pick any file!", "Failed to save to file").ShowAsync();
			}
		}

		private async void SaveMultipleFilesButton_Click(object sender, RoutedEventArgs e)
		{
			var folderPicker = new FolderPicker();
			folderPicker.FileTypeFilter.Add("*");

			var storageFolder = await folderPicker.PickSingleFolderAsync();
			if (storageFolder != null)
			{
				var storageFile1 = await storageFolder.CreateFileAsync("file1.txt", CreationCollisionOption.ReplaceExisting);
				var storageFile2 = await storageFolder.CreateFileAsync("file2.txt", CreationCollisionOption.ReplaceExisting);

				var textBox1 = ((sender as Button).Parent as StackPanel)
					.FindName("ContentTextBox1") as TextBox;
				var textBox2 = ((sender as Button).Parent as StackPanel)
					.FindName("ContentTextBox2") as TextBox;

				using (var stream = await storageFile1.OpenStreamForWriteAsync())
				{
					using (var tw = new StreamWriter(stream))
					{
						tw.WriteLine(textBox1?.Text);
					}
				}

				using (var stream = await storageFile2.OpenStreamForWriteAsync())
				{
					using (var tw = new StreamWriter(stream))
					{
						tw.WriteLine(textBox2?.Text);
					}
				}

				await new MessageDialog(storageFolder.Path, "Successfully saved files to folder").ShowAsync();
			}
			else
			{
				await new MessageDialog("You didn't pick any folder!", "Failed to save files").ShowAsync();
			}
		}
	}
}

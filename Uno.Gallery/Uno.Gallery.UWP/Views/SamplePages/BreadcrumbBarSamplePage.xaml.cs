using System.Collections.ObjectModel;
using System.Linq;
using Uno.Gallery.Entities.Data;
using Uno.Gallery.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "BreadcrumbBar",
		Description = "Provides the direct path of pages or folders to the current location.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/apps/design/controls/breadcrumbbar",
		DataType = typeof(BreadcrumbBarSamplePageViewModel))]
	public sealed partial class BreadcrumbBarSamplePage : Page
	{
		public BreadcrumbBarSamplePage()
		{
			this.InitializeComponent();
		}

		private void OnBreadcrumb_ItemClicked(Microsoft.UI.Xaml.Controls.BreadcrumbBar sender, Microsoft.UI.Xaml.Controls.BreadcrumbBarItemClickedEventArgs args)
		{
			if (!(((Sample)DataContext).Data is BreadcrumbBarSamplePageViewModel viewModel))
				return;

			for (int i = viewModel.Folders.Count - 1; i >= args.Index + 1; i--)
			{
				viewModel.Folders.RemoveAt(i);
				viewModel.DisplayItem = viewModel.Folders.Last();
			}
		}

		private void OnResetItems_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			if (!(((Sample)DataContext).Data is BreadcrumbBarSamplePageViewModel viewModel))
				return;

			var items = new FolderCollection();

			viewModel.Folders.Clear();
			foreach (var item in items)
			{
				viewModel.Folders.Add(item);
			}

			viewModel.Folders = items;
			viewModel.DisplayItem = viewModel.Folders.Last();
		}
	}

	public class BreadcrumbBarSamplePageViewModel : ViewModelBase
	{
		public string[] Items { get => GetProperty<string[]>(); set => SetProperty(value); }
		public ObservableCollection<Folder> Folders { get => GetProperty<ObservableCollection<Folder>>(); set => SetProperty(value); }
		public Folder DisplayItem { get => GetProperty<Folder>(); set => SetProperty(value); }

		public BreadcrumbBarSamplePageViewModel()
		{
			Items = new string[] { "Home", "Documents", "Images", "Memories", "2023", "Summer", "London" };
			Folders = new FolderCollection();

			DisplayItem = Folders.Last();
		}
	}
}


using System.Collections.ObjectModel;
using Uno.Gallery.Entities.Data;
using Uno.Gallery.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "TreeView",
		Description = "Represents a hierarchical list with expanding and collapsing nodes that contain nested items.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.treeview",
		DataType = typeof(TreeViewSamplePageViewModel))]
	public sealed partial class TreeViewSamplePage : Page
	{
		public TreeViewSamplePage()
		{
			this.InitializeComponent();
		}
	}

	public class TreeViewSamplePageViewModel : ViewModelBase
	{
		public ObservableCollection<TreeItem> Items { get => GetProperty<ObservableCollection<TreeItem>>(); set => SetProperty(value); }
		public ObservableCollection<TreeItem> Folders { get => GetProperty<ObservableCollection<TreeItem>>(); set => SetProperty(value); }
		public Folder DisplayItem { get => GetProperty<Folder>(); set => SetProperty(value); }
		public string SelectionMode { get => GetProperty<string>(); set => SetProperty(value); }
		public bool CanDragItems { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool AllowDrop { get => GetProperty<bool>(); set => SetProperty(value); }

		public TreeViewSamplePageViewModel()
		{
			Items = new TreeItemCollection();

			Folders = new TreeItemCollection();

			SelectionMode = "Multiple";
			CanDragItems = true;
			AllowDrop = true;

			Folders = new ObservableCollection<TreeItem>()
			{
				new TreeItem("Documents")
				{
					Children = new ObservableCollection<TreeItem>
					{
						new TreeItem("Personal"),
						new TreeItem("Projects"),
						new TreeItem("Work")
					}
				},
				new TreeItem("Pictures")
				{
					Children = new ObservableCollection<TreeItem>
					{
						new TreeItem("2022")
						{
							Children = new ObservableCollection<TreeItem>
							{
								new TreeItem("Spring"),
								new TreeItem("Winter")
							}
						},
						new TreeItem("2023")
						{
							Children = new ObservableCollection<TreeItem>
							{
								new TreeItem("May"),
								new TreeItem("July"),
								new TreeItem("September"),
							}
						},
					}
				}
			};
		}
	}
}


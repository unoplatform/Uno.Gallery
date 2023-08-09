﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Uno.Gallery.Entities.Data
{
	public class TreeItem
	{
		public TreeItem(string name)
		{
			Name = name;
		}

		public string Name { get; }
		public string Glyph => Children.Count > 0 ? "\uE8B7" : "\uE8A5";
		public ObservableCollection<TreeItem> Children { get; set; } = new ObservableCollection<TreeItem>();
	}

	public class TreeItemCollection : ObservableCollection<TreeItem>
	{
		public TreeItemCollection() : base(GetTreeItem())
		{
		}

		private static IEnumerable<TreeItem> GetTreeItem()
		{
			yield return new TreeItem("Documents")
			{
				Children = new ObservableCollection<TreeItem>
				{
					new TreeItem("Personal"),
					new TreeItem("Projects"),
					new TreeItem("Work")
				}
			};

			yield return new TreeItem("Pictures")
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
			};
		}
	}
}

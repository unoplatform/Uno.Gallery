using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Uno.Gallery.Entities.Data
{
	public class Folder
	{
		public Folder(string name, int itemCount, string description)
		{
			Name = name;
			ItemCount = itemCount;
			Description = description;
		}

		public string Name { get; }
		public int ItemCount { get; }
		public string Description { get; }
	}

	public class FolderCollection : ObservableCollection<Folder>
	{
		public FolderCollection() : base(GetFolders())
		{
		}

		private static IEnumerable<Folder> GetFolders()
		{
			yield return new Folder("Desktop", 12, "This folder contains various files and documents related to desktop applications or general computer usage.");
			yield return new Folder("Development", 2, "Dedicated to storing files and resources related to software development.");
			yield return new Folder("Uno Platform", 12, "Holds files and resources specifically related to the Uno Platform.");
			yield return new Folder("Blog posts", 6, "Contains articles, drafts, or research materials related to blog posts.");
			yield return new Folder("2023", 6, "Blog posts released (or to be) in 2023.");
			yield return new Folder("The Power of Uno Platform WebAssembly", 3, "All related content to the article The Power of Uno Platform WebAssembly.");
		}
	}
}

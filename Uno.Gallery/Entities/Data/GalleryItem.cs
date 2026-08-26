using System;
using System.Collections.Generic;

namespace Uno.Gallery.Entities.Data;

[Microsoft.UI.Xaml.Data.Bindable]
public class GalleryItem
{
	public GalleryItem(string title, string subtitle, string accent, bool isEnabled = true)
	{
		Title = title;
		Subtitle = subtitle;
		Accent = accent;
		IsEnabled = isEnabled;
	}

	public string Title { get; }
	public string Subtitle { get; }
	/// <summary>Hex accent color for the cover tile.</summary>
	public string Accent { get; }
	public bool IsEnabled { get; }
}

public class GalleryItemCollection : List<GalleryItem>
{
	public GalleryItemCollection() : base(GetItems()) { }

	private static IEnumerable<GalleryItem> GetItems()
	{
		yield return new GalleryItem("The Great Gatsby", "F. Scott Fitzgerald", "#4A90D9");
		yield return new GalleryItem("To Kill a Mockingbird", "Harper Lee", "#7B5EA7");
		yield return new GalleryItem("1984", "George Orwell", "#D95858");
		yield return new GalleryItem("Pride and Prejudice", "Jane Austen", "#5BAD6F");
		yield return new GalleryItem("The Catcher in the Rye", "J.D. Salinger", "#D9874A");
		yield return new GalleryItem("Brave New World", "Aldous Huxley", "#4AD9C8");
		yield return new GalleryItem("The Hobbit", "J.R.R. Tolkien", "#D9C24A");
		yield return new GalleryItem("Dune", "Frank Herbert", "#AD5B4A", isEnabled: false);
		yield return new GalleryItem("Foundation", "Isaac Asimov", "#7BAD6F");
		yield return new GalleryItem("Neuromancer", "William Gibson", "#5B7BAD");
	}
}

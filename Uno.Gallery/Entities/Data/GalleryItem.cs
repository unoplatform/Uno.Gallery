using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Uno.Gallery.Entities.Data;

[Microsoft.UI.Xaml.Data.Bindable]
public class GalleryItem
{
	public GalleryItem(string title, string subtitle, string accentHex, bool isEnabled = true)
	{
		Title = title;
		Subtitle = subtitle;
		Accent = new SolidColorBrush(ParseHexColor(accentHex));
		IsEnabled = isEnabled;
	}

	public string Title { get; }
	public string Subtitle { get; }
	/// <summary>Accent brush for the cover tile; parsed from hex at construction time.</summary>
	public SolidColorBrush Accent { get; }
	public bool IsEnabled { get; }
	/// <summary>Stable automation name for ItemsView multiple-selection containers: "ItemsView_Multiple_{Title}".</summary>
	public string MultipleAutomationName => $"ItemsView_Multiple_{Title}";
	/// <summary>Stable automation identifier for ItemsRepeater grid tiles.</summary>
	public string ItemsRepeaterGridAutomationId => $"ItemsRepeater_Grid_{Title}";

	private static Color ParseHexColor(string hex)
	{
		var s = hex.TrimStart('#');
		if (s.Length == 6)
			s = "FF" + s;
		var v = Convert.ToUInt32(s, 16);
		return Color.FromArgb((byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v);
	}
}

[Microsoft.UI.Xaml.Data.Bindable]
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

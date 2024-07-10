<Query Kind="Statements">
  <NuGetReference Version="1.1.2">AngleSharp</NuGetReference>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>AngleSharp.Html.Parser</Namespace>
  <Namespace>AngleSharp.Dom</Namespace>
  <Namespace>AngleSharp.Html.Dom</Namespace>
</Query>

// source generator for "platform.uno nav mimic" section (ctrl+f: BANNER_SOURCE_GEN)
#nullable enable

var source = Util.Cache<string>(() => new HttpClient().GetStringAsync("https://platform.uno/").Result, "https://platform.uno/");
var document = new HtmlParser().ParseDocument(source);

var rootMenuItems = document.QuerySelectorAll("nav.elementor-nav-menu--main > ul > li")
	.Select(ParseNavItem)
	.Dump("data", 0);
var root = new XElement("nav-root", rootMenuItems.Select(x => x switch
{
	{ Href: { }, Items: { Length: > 0 } } => new XComment("todo@xy: link with child items is not supported") as XNode,
	{ Href: { } } => new XElement("Button",
		new XAttribute("Content", x.Header?.ToUpper() ?? string.Empty),
		new XAttribute("Click", "OnAppBarButtonClick"),
		new XAttribute("Tag", x.Href),
		new XAttribute("Style", $"{{StaticResource {(x.Type == NavItemType.CTA ? "AppBarCtaButtonStyle" : "AppBarButtonStyle")}}}")
	),
	_ => new XElement("Button",
		new XAttribute("Content", x.Header?.ToUpper() ?? string.Empty),
		new XAttribute("Style", "{StaticResource AppBarDropdownButtonStyle}"),
		new XElement("Button.Flyout",
			new XElement("MenuBarItemFlyout", new XObject[]
			{
				new XAttribute("Placement", "Bottom"),
				new XAttribute("MenuFlyoutPresenterStyle", "{StaticResource AppBarMenuFlyoutPresenterStyle}"),
			}.Concat((x.Items ?? Enumerable.Empty<NavigationItem>()).Select(y => new XElement("MenuFlyoutItem",
				new XAttribute("Text", y.Header?.ToString() ?? string.Empty),
				y.Href is { } ? new XAttribute("Click", "OnAppBarButtonClick") : null,
				y.Href is { } ? new XAttribute("Tag", y.Href) : null,
				y.Href is not { } ? new XAttribute("IsEnabled", "False") : null,
				new XAttribute("Style", $"{{StaticResource {(y.Type == NavItemType.SectionHeader ? "AppBarHeaderMenuFlyoutItemStyle" : "AppBarMenuFlyoutItemStyle")}}}")
			))))
		)
	),
})).Dump("xaml");

NavigationItem ParseNavItem(IElement x) => new
(
	Type: ParseNavItemType(x),
	Header: SanitizeText(x.FirstElementChild?.TextContent),
	Href: (x.FirstElementChild as IHtmlAnchorElement)?.Href is { } href && href != "about:///#" ? href : null,
	Items: x.Children.ElementAtOrDefault(1)?.Children.Select(ParseNavItem).ToArray()
);
string? SanitizeText(string? text) => text?.Replace("\u2013", "-");
NavItemType ParseNavItemType(IElement x)
{
	if (x.ClassName?.Contains("header-menu-category") == true) return NavItemType.SectionHeader; // loose match header-menu-category and header-menu-category--stuff
	if (x.ClassList?.Contains("get-started-btn") == true) return NavItemType.CTA; // exact match

	return default;
}

public record class NavigationItem(NavItemType Type, string? Header, string? Href, NavigationItem[]? Items = null);
public enum NavItemType { Default = 0, SectionHeader, CTA }

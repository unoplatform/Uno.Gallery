using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Toolkit.UI;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.Toolkit, "CardContentControl",
	SourceSdk.UnoToolkit,
	Description = "CardContentControl is a flexible Toolkit content surface with elevation, shadow color, and optional pointer and focus interaction states. It complements rather than replaces the slot-based Card control.",
	DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls/CardAndCardContentControl.html",
	Slug = "card-content-control",
	Tags = new[] { "card", "content", "elevation", "shadow", "clickable" },
	Status = SampleStatus.Stable,
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "card" })]
public sealed partial class CardContentControlSamplePage : Page
{
	private int _activations;

	public CardContentControlSamplePage()
	{
		InitializeComponent();
	}

	private void ToggleConfiguration_Click(object sender, RoutedEventArgs e)
	{
		var card = GetCard();
		if (card is null)
		{
			return;
		}

		if (card.Elevation == 8)
		{
			card.Elevation = 2;
			card.ShadowColor = Colors.Teal;
			card.IsClickable = false;
		}
		else
		{
			card.Elevation = 8;
			card.ShadowColor = Colors.DarkSlateBlue;
			card.IsClickable = true;
		}

		UpdateStatus(card);
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		var card = GetCard();
		if (card is null)
		{
			return;
		}

		card.Elevation = 8;
		card.ShadowColor = Colors.DarkSlateBlue;
		card.IsClickable = true;
		_activations = 0;
		UpdateStatus(card);
	}

	private void Activate_Click(object sender, RoutedEventArgs e)
	{
		var card = GetCard();
		if (card is null || !card.IsClickable)
		{
			return;
		}

		_activations++;
		UpdateStatus(card);
	}

	private void InteractiveCard_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
	{
		if (sender is CardContentControl { IsClickable: true } card)
		{
			_activations++;
			UpdateStatus(card);
		}
	}

	private CardContentControl? GetCard()
		=> SamplePageLayoutRoot.GetSampleChild<CardContentControl>(Design.Material, "InteractiveCard");

	private void UpdateStatus(CardContentControl card)
	{
		var status = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Material, "CardStatus");
		if (status is not null)
		{
			status.Text = $"Elevation: {card.Elevation:0}; Shadow: {card.ShadowColor}; Clickable: {card.IsClickable}; activations: {_activations}";
		}
	}
}

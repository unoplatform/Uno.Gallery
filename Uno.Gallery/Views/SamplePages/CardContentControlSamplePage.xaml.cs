using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Helpers;
using Uno.Toolkit.UI;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.Toolkit, "CardContentControl",
	SourceSdk.UnoToolkit,
	Description = "CardContentControl is a flexible Toolkit content surface with elevation, shadow color, and interaction states. Wrap it in a Button when keyboard and automation Invoke semantics are required.",
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
		var button = GetCardButton();

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

		button.IsEnabled = card.IsClickable;
		UpdateStatus(card);
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		var card = GetCard();
		var button = GetCardButton();

		card.Elevation = 8;
		card.ShadowColor = Colors.DarkSlateBlue;
		card.IsClickable = true;
		button.IsEnabled = true;
		_activations = 0;
		UpdateStatus(card);
	}

	private void InteractiveCard_Click(object sender, RoutedEventArgs e)
	{
		var card = GetCard();
		_activations++;
		UpdateStatus(card);
	}

	private CardContentControl GetCard()
		=> SamplePageLayoutRoot.GetSampleChild<CardContentControl>(Design.Material, "InteractiveCard")
			?? throw new InvalidOperationException("The interactive card is not loaded.");

	private Button GetCardButton()
		=> SamplePageLayoutRoot.GetSampleChild<Button>(Design.Material, "InteractiveCardButton")
			?? throw new InvalidOperationException("The interactive card button is not loaded.");

	private void UpdateStatus(CardContentControl card)
	{
		var status = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Material, "CardStatus")
			?? throw new InvalidOperationException("The card status is not loaded.");
		AccessibilityHelper.Announce(
			status,
			$"Elevation: {card.Elevation:0}; Shadow: {card.ShadowColor}; Clickable: {card.IsClickable}; activations: {_activations}");
	}
}

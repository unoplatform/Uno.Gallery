using System;
using System.Collections.Generic;
using System.Linq;
using Uno.Gallery.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Helpers;
using Microsoft.UI.Xaml.Media;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "AutoSuggestBox", Description = "Represents a text control that makes suggestions to users as they enter text.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.autosuggestbox?view=windows-app-sdk-1.3", DataType = typeof(AutoSuggestBoxSamplePageViewModel))]
	public sealed partial class AutoSuggestBoxSamplePage : Page
	{
		public AutoSuggestBoxSamplePage()
		{
			this.InitializeComponent();
		}

		private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
			{
				return;
			}

			if (string.IsNullOrEmpty(sender.Text))
			{
				return;
			}

			if (((Sample)DataContext).Data is AutoSuggestBoxSamplePageViewModel viewModel)
			{
				EnsureFluentResourcesForPopups(sender);
				sender.ItemsSource = viewModel.GetSuggestedItems(sender.Text).Select(sample => sample.Title).ToList();
			}
		}

		private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			if (((Sample)DataContext).Data is AutoSuggestBoxSamplePageViewModel viewModel)
			{
				viewModel.SelectedString = args.SelectedItem.ToString();
			}

		}

		private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
			{
				return;
			}

			if (((Sample)DataContext).Data is AutoSuggestBoxSamplePageViewModel viewModel)
			{
				EnsureFluentResourcesForPopups(sender);
				sender.ItemsSource = viewModel.GetSuggestedItems(sender.Text);
			}
		}

		private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (args is null)
			{
				return;
			}

			if (((Sample)DataContext).Data is AutoSuggestBoxSamplePageViewModel viewModel)
			{
				viewModel.SearchBoxSelectedItem = (Sample)args.ChosenSuggestion;
			}
		}

		private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			if (((Sample)DataContext).Data is AutoSuggestBoxSamplePageViewModel viewModel)
			{
				viewModel.SearchBoxSelectedItem = (Sample)args.SelectedItem;
			}
		}

		// This workaround is necessary since the app's main theme is Material and we need to ensure it displays with Fluent styles
		private void EnsureFluentResourcesForPopups(AutoSuggestBox sender)
		{
			if (sender.IsSuggestionListOpen)
			{
				var popup = VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot).FirstOrDefault();

				popup.EnsureXamlControlsResources(true);
				(popup.Child as Border).EnsureXamlControlsResources(true);
			}
		}
	}

	public class AutoSuggestBoxSamplePageViewModel : ViewModelBase
	{
		public string SelectedString { get => GetProperty<string>(); set => SetProperty(value); }

		public Sample SearchBoxSelectedItem { get => GetProperty<Sample>(); set => SetProperty(value); }

		public List<Sample> GetSuggestedItems(string searchQuery)
		{
			if (string.IsNullOrEmpty(searchQuery))
			{
				return null;
			}

			var splitText = searchQuery.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);

			var samples = App.GetSamples()
				.OrderByDescending(sample => sample.SortOrder.HasValue)
				.ThenBy(sample => sample.SortOrder)
				.ThenBy(sample => sample.Title)
				.Where(sample => splitText.All(key => sample.Title.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0));

			return samples.ToList();
		}
	}
}

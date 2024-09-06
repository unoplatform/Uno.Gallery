using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "TextBox", Description = "This control allows users to input a textual value.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.textbox")]
	public sealed partial class TextBoxSamplePage : Page
	{
		private TextBox txtFilled;
		private TextBox txtOutlined;

		public TextBoxSamplePage()
		{
			this.InitializeComponent();

			Loaded += (s, e) =>
			{
				txtFilled = SamplePageLayout.GetSampleChild<TextBox>(Design.Material, "txtFilled");
				txtOutlined = SamplePageLayout.GetSampleChild<TextBox>(Design.Material, "txtOutlined");
			};
		}

		private void AddHeader(object sender, RoutedEventArgs e)
		{
			if (txtFilled != null)
			{
				txtFilled.Header = "Header";
			}

			if (txtOutlined != null)
			{
				txtOutlined.Header = "Header";
			}
		}

		private void RemoveHeader(object sender, RoutedEventArgs e)
		{
			if(txtFilled != null)
			{
				txtFilled.Header = null;
			}

			if(txtOutlined != null)
			{
				txtOutlined.Header = null;
			}
		}

		private void AddPlaceholder(object sender, RoutedEventArgs e)
		{
			if(txtFilled != null)
			{
				txtFilled.PlaceholderText = "Placeholder";
			}

			if(txtOutlined != null)
			{
				txtOutlined.PlaceholderText = "Placeholder";
			}
		}

		private void RemovePlaceholder(object sender, RoutedEventArgs e)
		{
			if(txtFilled != null)
			{
				txtFilled.PlaceholderText = null;
			}

			if(txtOutlined != null)
			{
				txtOutlined.PlaceholderText = null;
			}
		}

		private void AddText(object sender, RoutedEventArgs e)
		{
			if(txtFilled != null)
			{
				txtFilled.Text = "Text";
			}

			if(txtOutlined != null)
			{
				txtOutlined.Text = "Text";
			}
		}

		private void RemoveText(object sender, RoutedEventArgs e)
		{
			if(txtFilled != null)
			{
				txtFilled.Text = null;
			}

			if(txtOutlined != null)
			{
				txtOutlined.Text = null;
			}
		}
	}
}

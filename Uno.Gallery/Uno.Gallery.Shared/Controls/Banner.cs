using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uno.Disposables;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Controls
{
	public partial class Banner : Control
	{
		private const string HowItWorkButtonPart = "PART_HowItWorkButton";
		private const string CodeSampleButtonPart = "PART_CodeSampleButton";
		private const string ShowCasesButtonPart = "PART_ShowCasesButton";
		private const string DocsButtonPart = "PART_DocsButton";
		private const string BlogButtonPart = "PART_BlogButton";
		private const string ContactButtonPart = "PART_ContactButton";
		private const string GetStartedButtonPart = "PART_GetStartedButton";

		private static readonly Uri HowItWorkLink = new Uri("https://platform.uno/how-it-works/");
		private static readonly Uri CodeSampleButtonLink = new Uri("https://platform.uno/code-samples/");
		private static readonly Uri ShowCasesButtonLink = new Uri("https://platform.uno/showcases/");
		private static readonly Uri DocsButtonLink = new Uri("https://platform.uno/docs/articles/intro.html");
		private static readonly Uri BlogButtonLink = new Uri("https://platform.uno/blog/");
		private static readonly Uri ContactButtonLink = new Uri("https://platform.uno/contact/#");
		private static readonly Uri GetStartedButtonLink = new Uri("https://platform.uno/docs/articles/getting-started-tutorial-1.html");

		private Button _howItWorksButton;
		private Button _codeSampleButton;
		private Button _showCasesButton;
		private Button _docsButton;
		private Button _blogButton;
		private Button _contactButton;
		private Button _getStartedButton;

		private readonly SerialDisposable _subscriptions = new SerialDisposable();

		private IReadOnlyCollection<BannerButton> BannerButtons => new List<BannerButton>
		{
			new BannerButton(_howItWorksButton, HowItWorkLink),
			new BannerButton(_codeSampleButton, CodeSampleButtonLink),
			new BannerButton(_showCasesButton, ShowCasesButtonLink),
			new BannerButton(_docsButton, DocsButtonLink),
			new BannerButton(_blogButton, BlogButtonLink),
			new BannerButton(_contactButton, ContactButtonLink),
			new BannerButton(_getStartedButton, GetStartedButtonLink),
		};

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_howItWorksButton = (Button)GetTemplateChild(HowItWorkButtonPart);
			_codeSampleButton = (Button)GetTemplateChild(CodeSampleButtonPart);
			_showCasesButton = (Button)GetTemplateChild(ShowCasesButtonPart);
			_docsButton = (Button)GetTemplateChild(DocsButtonPart);
			_blogButton = (Button)GetTemplateChild(BlogButtonPart);
			_contactButton = (Button)GetTemplateChild(ContactButtonPart);
			_getStartedButton = (Button)GetTemplateChild(GetStartedButtonPart);

			var disposables = new CompositeDisposable();
			_subscriptions.Disposable = disposables;

			BindOnClick(_howItWorksButton);
			BindOnClick(_codeSampleButton);
			BindOnClick(_showCasesButton);
			BindOnClick(_docsButton);
			BindOnClick(_blogButton);
			BindOnClick(_contactButton);
			BindOnClick(_getStartedButton);

			void BindOnClick(Button button)
			{
				button.Click += OnBannerButtonClicked;
				Disposable
					.Create(() => button.Click -= OnBannerButtonClicked)
					.DisposeWith(disposables);
			}
		}

		private void OnBannerButtonClicked(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && BannerButtons.FirstOrDefault(x => x.Button == button) is BannerButton buttonBanner)
			{
				Launcher.LaunchUriAsync(buttonBanner.Url);
			}
		}

		private class BannerButton
		{
			public Button Button { get; set; }

			public Uri Url { get; set; }

			public BannerButton(Button button, Uri url)
			{
				Button = button;
				Url = url;
			}
		}
	}
}

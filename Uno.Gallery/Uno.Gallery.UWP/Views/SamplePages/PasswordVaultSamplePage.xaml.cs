using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.Features, "PasswordVault", Description = "Represents a Credential Locker of credentials. Lockers are specific to a user..", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.security.credentials.passwordvault")]
#if __WASM__
	[SampleConditional(SampleConditionals.Disabled, Reason = "API is not yet supported")]
#endif
    public sealed partial class PasswordVaultSamplePage : Page
	{
		private const string _resource = "MyResourceName";
		private const string _userName = "MyUsername";

        public PasswordVaultSamplePage()
		{
			this.InitializeComponent();
		}

		private void Store_Click(object sender, RoutedEventArgs args)
		{
            var vault = new Windows.Security.Credentials.PasswordVault();
            vault.Add(new Windows.Security.Credentials.PasswordCredential(_resource, _userName, DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString()));
		}

		private async void Retrieve_Click(object sender, RoutedEventArgs args)
		{
            var vault = new Windows.Security.Credentials.PasswordVault();
			try
			{
				var credential = vault.Retrieve(_resource, _userName);
				credential.RetrievePassword();
				var password = credential.Password;

				if (!string.IsNullOrEmpty(password))
				{
					var dialog = new ContentDialog()
					{
						Title = "Stored Secret",
						Content = new TextBlock() { Text = password },
						PrimaryButtonText = "OK"
					};
					await dialog.ShowAsync();
				}
				else
				{
					var dialog = new ContentDialog()
					{
						Title = "No Secret in PasswordVault",
						PrimaryButtonText = "OK"
					};
					await dialog.ShowAsync();
				}
			}
			catch
			{
                var dialog = new ContentDialog()
                {
                    Title = "No Secret in PasswordVault",
                    PrimaryButtonText = "OK"
                };
                await dialog.ShowAsync();
            }
		}
	}
}

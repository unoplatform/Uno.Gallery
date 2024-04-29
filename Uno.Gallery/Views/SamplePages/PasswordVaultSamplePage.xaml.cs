using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Security.Credentials;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "PasswordVault", Description = "Represents a Credential Locker of credentials. Lockers are specific to a user.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.security.credentials.passwordvault")]
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
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(_resource, _userName, DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString()));
		}

		private async void Retrieve_Click(object sender, RoutedEventArgs args)
		{
            var vault = new PasswordVault();
			try
			{
				var credential = vault.Retrieve(_resource, _userName);
				credential.RetrievePassword();
				var password = credential.Password;

				if (!string.IsNullOrEmpty(password))
				{
					var dialog = new AppDialog()
					{
						Title = "Stored Secret",
						Content = new TextBlock() { Text = password },
						PrimaryButtonText = "OK"
					};
					await dialog.ShowAsync();
				}
				else
				{
					var dialog = new AppDialog()
					{
						Title = "No Secret in PasswordVault",
						PrimaryButtonText = "OK"
					};
					await dialog.ShowAsync();
				}
			}
			catch
			{
                var dialog = new AppDialog()
                {
                    Title = "No Secret in PasswordVault",
                    PrimaryButtonText = "OK"
                };
                await dialog.ShowAsync();
            }
		}
	}
}

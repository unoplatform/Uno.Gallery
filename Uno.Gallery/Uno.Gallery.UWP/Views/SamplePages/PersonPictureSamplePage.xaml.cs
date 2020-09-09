using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.Components, "PersonPicture", Description = "The person picture control displays the avatar image for a person, if one is available; if not, it displays the person's initials or a generic glyph.", DocumentationLink = "https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/person-picture")]
	public sealed partial class PersonPictureSamplePage : Page
	{
		public PersonPictureSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
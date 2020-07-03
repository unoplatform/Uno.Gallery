using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Controls
{
	/// <summary>
	/// This control is used to display content with its xaml code.
	/// </summary>
	public partial class SnippetView : ContentControl
	{
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register("Title", typeof(string), typeof(SnippetView), new PropertyMetadata(string.Empty));
	}
}

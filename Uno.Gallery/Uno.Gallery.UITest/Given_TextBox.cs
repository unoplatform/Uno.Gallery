using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using Uno.UI.Xaml;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;
using Uno.UITests.Helpers;

namespace Uno.Gallery.UITests
{
	public class Given_TextBox : TestBase
	{
		[Test]
		public void TextBox_fluent()
		{
			NavigateToSample("TextBox", "Fluent");

			//Fluent :- Defautl Enabled 
			var TextBox_Default = new QueryEx(x => x.All().Marked("TextBox_Fluent"));
			TextBox_Default.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_Default.GetDependencyPropertyValue<string>("Text"));

			//Fluent :- Defautl Disabled
			var TextBox_Default_Disabled = new QueryEx(x => x.All().Marked("TextBox_Disabled_Fluent"));
			bool TextBox_Default_Disabled_bool = TextBox_Default_Disabled.GetDependencyPropertyValue<bool>("IsEnabled");
			Assert.AreEqual("", TextBox_Default_Disabled.GetDependencyPropertyValue<string>("Text"));
			Assert.IsFalse(TextBox_Default_Disabled_bool, "Is TextBox Disabled ?");

			//Fluent :- Header Enabled
			var TextBox_Header = new QueryEx(x => x.All().Marked("TextBox_Header_Fluent"));
			TextBox_Header.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_Header.GetDependencyPropertyValue<string>("Text"));

			//Fluent :- Header Disabled
			var TextBox_Header_Disabled = new QueryEx(x => x.All().Marked("TextBox_Header_Disabled_Fluent"));
			bool TextBox_Header_Disabled_bool = TextBox_Header_Disabled.GetDependencyPropertyValue<bool>("IsEnabled");
			Assert.AreEqual("", TextBox_Header_Disabled.GetDependencyPropertyValue<string>("Text"));
			Assert.IsFalse(TextBox_Header_Disabled_bool, "Suggestion list is open");

			//Fluent :- MultiLine Enabled
			var TextBox_MultiLine = new QueryEx(x => x.All().Marked("TextBox_MultiLine_Fluent"));
			TextBox_MultiLine.ClearText();
			TextBox_MultiLine.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_MultiLine.GetDependencyPropertyValue<string>("Text"));

			//Fluent :- MultiLine Disabled
			var TextBox_MultiLine_Disabled = new QueryEx(x => x.All().Marked("TextBox_MultiLine_Disabled_Fluent"));
			bool TextBox_MultiLine_Disabled_bool = TextBox_MultiLine_Disabled.GetDependencyPropertyValue<bool>("IsEnabled");
			Assert.IsFalse(TextBox_MultiLine_Disabled_bool, "Is TextBox Enabled?");
			Assert.AreEqual("Lorem ipsum dolor sit amet consectetur adipiscing, elit aliquam ullamcorper commodo primis ornare himenaeos, inceptos tellus accumsan praesent laoreet. Pharetra semper ullamcorper neque mollis vestibulum luctus gravida facilisi rhoncus, rutrum massa bibendum vitae imperdiet quisque fames dignissim, varius curae erat risus platea orci quis scelerisque. Auctor erat vestibulum enim sodales sapien nam litora rhoncus condimentum praesent, platea dui odio eros integer id gravida turpis semper nisi maecenas, nascetur dictumst sed arcu aenean varius dis leo habitant.", TextBox_MultiLine_Disabled.GetDependencyPropertyValue<string>("Text"));


			//Fluent :- Test once again for header enabled
			TextBox_Header.ClearText();
			TextBox_Header.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_Header.GetDependencyPropertyValue<string>("Text"));

		}

		[Test]
		public void TextBox_Material()
		{
			NavigateToSample("TextBox", "Material");

			//Material :- Default
			var TextBox_Material = new QueryEx(x => x.All().Marked("TextBox_Default_Material"));
			TextBox_Material.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_Material.GetDependencyPropertyValue<string>("Text"));

			//Material :- Default with Icon
			var TextBox_Default_WithIcon = new QueryEx(x => x.All().Marked("TextBox_Default_WithIcon_Material"));
			TextBox_Default_WithIcon.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_Default_WithIcon.GetDependencyPropertyValue<string>("Text"));


			//Material :- Default with Disabled
			var TextBox_Default_Disabled = new QueryEx(x => x.All().Marked("TextBox_Default_Disabled_Material"));
			bool TextBox_Default_Disabled_bool = TextBox_Default_Disabled.GetDependencyPropertyValue<bool>("IsEnabled");
			Assert.AreEqual("", TextBox_Default_Disabled.GetDependencyPropertyValue<string>("Text"));
			Assert.IsFalse(TextBox_Default_Disabled_bool, "Is TextBox Disabled ?");


			//Material :- Outlined 
			var TextBox_Outline = new QueryEx(x => x.All().Marked("TextBox_Outlined_Material"));
			TextBox_Outline.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_Outline.GetDependencyPropertyValue<string>("Text"));

			//Material :- Outlined with Icon
			var TextBox_Outline_WithIcon = new QueryEx(x => x.All().Marked("TextBox_Outlined_WithIcon_Material"));
			TextBox_Outline_WithIcon.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_Outline_WithIcon.GetDependencyPropertyValue<string>("Text"));

			//Material :- Outlined with Disabled
			var TextBox_Outlined_Disabled = new QueryEx(x => x.All().Marked("TextBox_Outlined_Disabled_Material"));
			bool TextBox_Outlined_Disabled_bool = TextBox_Outlined_Disabled.GetDependencyPropertyValue<bool>("IsEnabled");
			Assert.AreEqual("", TextBox_Outlined_Disabled.GetDependencyPropertyValue<string>("Text"));
			Assert.IsFalse(TextBox_Outlined_Disabled_bool, "Is TextBox Disabled ?");

			//Material :- MultiLine
			var TextBox_Mutline = new QueryEx(x => x.All().Marked("TextBox_MultiLine_Material"));
			TextBox_Mutline.ClearText();
			TextBox_Mutline.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_Mutline.GetDependencyPropertyValue<string>("Text"));

			//Material :- MultiLine Disabled
			var TextBox_MultiLine_Disabled = new QueryEx(x => x.All().Marked("TextBox_MultiLine_Disabled_Material"));
			bool TextBox_MultiLine_Disabled_bool = TextBox_MultiLine_Disabled.GetDependencyPropertyValue<bool>("IsEnabled");
			Assert.IsFalse(TextBox_MultiLine_Disabled_bool, "Is TextBox Disabled");
			Assert.AreEqual("Lorem ipsum dolor sit amet consectetur adipiscing, elit aliquam ullamcorper commodo primis ornare himenaeos, inceptos tellus accumsan praesent laoreet. Pharetra semper ullamcorper neque mollis vestibulum luctus gravida facilisi rhoncus, rutrum massa bibendum vitae imperdiet quisque fames dignissim, varius curae erat risus platea orci quis scelerisque. Auctor erat vestibulum enim sodales sapien nam litora rhoncus condimentum praesent, platea dui odio eros integer id gravida turpis semper nisi maecenas, nascetur dictumst sed arcu aenean varius dis leo habitant.", TextBox_MultiLine_Disabled.GetDependencyPropertyValue<string>("Text"));

		}
		[Test]

		public void TextBox_Cupertino()
		{
			NavigateToSample("TextBox", "Cupertino");

			//Cupertino :- Default
			var TextBox_Default = new QueryEx(x => x.All().Marked("TextBox_Default_Cupertino"));
			TextBox_Default.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_Default.GetDependencyPropertyValue<string>("Text"));

			//Cupertino :- MultiLine
			var TextBox_MultiLine = new QueryEx(x => x.All().Marked("TextBox_MultiLine_Cupertino"));
			TextBox_MultiLine.ClearText();
			TextBox_MultiLine.EnterText("Uno platform");
			Assert.AreEqual("Uno platform", TextBox_MultiLine.GetDependencyPropertyValue<string>("Text"));

			//Cupertino :- Disabled
			var TextBox_Disabled = new QueryEx(x => x.All().Marked("TextBox_MultLine_Disabled_Cupertino"));
			bool TextBox_MultiLine_Disabled_bool = TextBox_Disabled.GetDependencyPropertyValue<bool>("IsEnabled");
			Assert.AreEqual("This is my text", TextBox_Disabled.GetDependencyPropertyValue<string>("Text"));
			Assert.IsFalse(TextBox_MultiLine_Disabled_bool, "Is TextBox Disabled ?");



		}


	}
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Uno.Gallery.Selectors
{
	public class NeumorphicTemplateSelector : DataTemplateSelector
	{
		public DataTemplate NoneTemplate { get; set; }
		public DataTemplate TextBoxTemplate { get; set; }
		public DataTemplate ComboBoxTemplate { get; set; }
		public DataTemplate PasswordBoxTemplate { get; set; }
		public DataTemplate ButtonTemplate { get; set; }
		public DataTemplate FabTemplate { get; set; }
		public DataTemplate ToggleButtonTemplate { get; set; }
		public DataTemplate IconButtonTemplate { get; set; }
		public DataTemplate RadioButtonTemplate { get; set; }
		public DataTemplate CheckBoxTemplate { get; set; }
		public DataTemplate ChipTemplate { get; set; }
		public DataTemplate SliderTemplate { get; set; }
		public DataTemplate ToggleSwitchTemplate { get; set; }
		public DataTemplate TabBarTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
			=> (item as string)?.ToLowerInvariant() switch
			{
				"textbox" => TextBoxTemplate,
				"passwordbox" => PasswordBoxTemplate,
				"combobox" => ComboBoxTemplate,
				"button" => ButtonTemplate,
				"fab" => FabTemplate,
				"togglebutton" => ToggleButtonTemplate,
				"iconbutton" => IconButtonTemplate,
				"radiobutton" => RadioButtonTemplate,
				"checkbox" => CheckBoxTemplate,
				"chip" => ChipTemplate,
				"slider" => SliderTemplate,
				"toggleswitch" => ToggleSwitchTemplate,
				"tabbar" => TabBarTemplate,
				_ => NoneTemplate
			};

	}
}

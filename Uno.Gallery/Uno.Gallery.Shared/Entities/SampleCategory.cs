using Uno.Gallery.Entities;

namespace Uno.Gallery
{
	public enum SampleCategory
	{
		/// <summary>
		/// Reserved for samples placed on top with no category, eg: Home, Overview
		/// </summary>
		None,

		[SampleCategoryInfo("\uE790", "Theming")]
        Theme,
        
		[SampleCategoryInfo("\uE81E", "UI components")]
		Components,
        
        [SampleCategoryInfo("\uE8AE", "UI features")]
        Features,

        [SampleCategoryInfo("\uE950", "Non-UI features")]
        NonUI,
        
        [SampleCategoryInfo("\uF0B4", "Toolkit")]
		Toolkit,
    }
}

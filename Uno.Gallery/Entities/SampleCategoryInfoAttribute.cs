using System;

namespace Uno.Gallery.Entities
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class SampleCategoryInfoAttribute : Attribute
    {
        public SampleCategoryInfoAttribute(string glyph, string resourceKey, string caption)
        {
            Glyph = glyph;
            ResourceKey = resourceKey;
            Caption = caption;
        }

        public string Glyph { get; }

        public string ResourceKey { get; }

        public string Caption { get; }
    }
}

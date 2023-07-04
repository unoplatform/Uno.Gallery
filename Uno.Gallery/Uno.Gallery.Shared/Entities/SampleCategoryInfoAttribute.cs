using System;

namespace Uno.Gallery.Entities
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class SampleCategoryInfoAttribute : Attribute
    {
        public SampleCategoryInfoAttribute(string glyph, string caption)
        {
            Glyph = glyph;
            Caption = caption;
        }

        public string Glyph { get; }

        public string Caption { get; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Uno.Gallery
{
	public static class EnumHelper
	{
		/// <summary>
		/// Generic method to extract description attributes from enum
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="Expected"></typeparam>
		/// <param name="enumeration"></param>
		/// <param name="expression"></param>
		/// <returns>The attribute of type T that exists on the enum value</returns>
		public static Expected GetAttributeValue<T, Expected>(this Enum enumeration, Func<T, Expected> expression)
			where T : Attribute
		{
			T attribute =
				enumeration
					.GetType()
					.GetMember(enumeration.ToString())
					.Where(member => member.MemberType == MemberTypes.Field)
					.FirstOrDefault()
					.GetCustomAttributes(typeof(T), false)
					.Cast<T>()
					.SingleOrDefault();

			if (attribute == null)
				return default;

			return expression(attribute);
		}

		/// <summary>
		/// Get the description value from DescriptionAttribute
		/// </summary>
		/// <param name="enumeration"></param>
		/// <returns></returns>
		public static string GetDescription(this Enum enumeration) => enumeration.GetAttributeValue<DescriptionAttribute, string>(x => x.Description);
	}
}

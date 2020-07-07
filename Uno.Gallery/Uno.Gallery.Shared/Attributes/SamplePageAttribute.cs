using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Uno.Gallery
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class SamplePageAttribute : Attribute
	{
		public SamplePageAttribute(string title, string description)
		{
			Title = title;
			Description = description;
		}

		public string Title { get; }

		public string Description { get; }

		public static IReadOnlyList<Sample> GetAllSamples()
		{
			var query = from type in FindDefinedAssemblies(Assembly.GetExecutingAssembly())
						let sampleAttribute = FindSampleAttribute(type)
						where sampleAttribute != null
						select new Sample(sampleAttribute, type.AsType());

			return query.ToArray();
		}

		private static IEnumerable<TypeInfo> FindDefinedAssemblies(Assembly assembly)
		{
			try
			{
				return assembly.DefinedTypes.ToArray();
			}
			catch (Exception)
			{
				return new TypeInfo[0];
			}
		}

		private static SamplePageAttribute FindSampleAttribute(TypeInfo type)
		{
			try
			{
				if (!(type.Namespace?.StartsWith("System.Windows") ?? true))
				{
					return type?.GetCustomAttributes()
						.OfType<SamplePageAttribute>()
						.FirstOrDefault();
				}
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}

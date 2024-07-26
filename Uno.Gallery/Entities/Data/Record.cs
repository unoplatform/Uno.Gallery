using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.Gallery.Entities.Data
{
	public class Record
	{
		public Record(string compositionName, string artistName, string color)
		{
			CompositionName = compositionName;
			ArtistName = artistName;
			Color = color;
		}

		public string CompositionName { get; }
		public string ArtistName { get; }
		public string Color { get; }
	}

	public class RecordCollection : List<Record>
	{
		public RecordCollection() : base(GetRecords())
		{
		}

		private static IEnumerable<Record> GetRecords()
		{
			yield return new Record(compositionName: "Mass in B minor", artistName: "Johann Sebastian Bach", color: "#159bff");
			yield return new Record(compositionName: "Third Symphony", artistName: "Ludwig van Beethoven", color: "#7a67f8");
			yield return new Record(compositionName: "Serse", artistName: "George Frideric Handel", color: "#67e5ad");
			yield return new Record(compositionName: "Idomeneo", artistName: "Wolfgang Amadeus Mozart", color: "#f85977");
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.UITest;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public static class Extensions
	{
		public static Func<IAppQuery, IAppQuery> WaitThenTap(this IApp app, Func<IAppQuery, IAppQuery> query, TimeSpan? timeout = null)
		{
			app.WaitForElement(query, timeout: timeout);
			app.Tap(query);

			return query;
		}

		public static QueryEx ToQueryEx(this Func<IAppQuery, IAppQuery> query) => new QueryEx(query);
	}
}

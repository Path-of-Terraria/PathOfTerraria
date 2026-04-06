using System.Collections.Generic;

namespace PathOfTerraria.Utilities;

internal static class LinqUtils
{
	public static IEnumerable<TResult> SelectExcept<TSource, TResult>(this IEnumerable<TSource> values, TResult exception, Func<TSource, TResult> selector)
	{
		foreach (TSource value in values)
		{
			TResult result = selector(value);

			if (result is not null && !result.Equals(exception)) 
			{ 
				yield return result; 
			}
		}
	}
}

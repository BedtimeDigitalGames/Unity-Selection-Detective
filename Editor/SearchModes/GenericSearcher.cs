using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BedtimeCore.SelectionDetective
{
	internal abstract class GenericSearcher<T> : ISearchMode where T : Object
	{
		public void Filter(Action<string, FilterObject, Type> add, FilterObject fo)
		{
			foreach (var co in fo.Components)
			{
				var result = SearchUtility.Find<T>(co);
				
				foreach (var prop in result)
				{
					add($"{prop.name}", fo, typeof(T));
				}
			}	
		}

		public Texture Icon { get; }

		public string Name => typeof(T).GetNameWithoutGenericArity();
		public int Priority { get; }
	}
}
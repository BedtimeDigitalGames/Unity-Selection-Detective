using System;
using UnityEngine;

namespace BedtimeCore.SelectionDetective
{
	internal class SearchNames : ISearchMode
	{
		public void Filter(Action<string, FilterObject, Type> add, FilterObject fo) => add(fo.Name, fo, null);

		public Texture Icon { get; }

		public string Name => "Names";
	}
}
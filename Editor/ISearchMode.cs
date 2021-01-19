using System;
using UnityEngine;

namespace BedtimeCore.SelectionDetective
{
	public interface ISearchMode
	{
		void Filter(Action<string, FilterObject, Type> add, FilterObject toFilter);
		Texture Icon { get; }
		string Name { get; }
	}
}	
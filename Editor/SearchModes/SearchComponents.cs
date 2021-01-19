using System;
using UnityEditor;
using UnityEngine;

namespace BedtimeCore.SelectionDetective
{
	internal class SearchComponents : ISearchMode
	{
		public void Filter(Action<string, FilterObject, Type> add, FilterObject toFilter)
		{
			foreach (var c in toFilter.Components)
			{
				var type = c != null ? c.GetType() : null;
				var name = type?.Name ?? "NULL"; 
				add(name, toFilter, type);
			}
		}
		public Texture Icon { get; }

		public string Name => "Components";
		
		public SearchComponents()
		{
			Icon = EditorGUIUtility.IconContent("cs Script Icon").image;;
		}
	}
}
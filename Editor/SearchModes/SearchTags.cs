using System;
using UnityEditor;
using UnityEngine;

namespace BedtimeCore.SelectionDetective
{
	internal class SearchTags : ISearchMode
	{
		public void Filter(Action<string, FilterObject, Type> add, FilterObject fo) => add(fo.Tag, fo, null);

		public Texture Icon { get; }

		public string Name => "Tags";
		
		public SearchTags()
		{
			Icon = EditorGUIUtility.IconContent("d_FilterByLabel").image;
		}
	}
}
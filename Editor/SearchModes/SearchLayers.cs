using System;
using UnityEditor;
using UnityEngine;

namespace BedtimeCore.SelectionDetective
{
	internal class SearchLayers : ISearchMode
	{
		public void Filter(Action<string, FilterObject, Type> add, FilterObject fo) => add(fo.Layer, fo, null);
		public Texture Icon { get; }

		public string Name => "Layers";
		
		public SearchLayers()
		{
			Icon = EditorGUIUtility.IconContent("d_SceneViewFx").image;
		}
	}
}
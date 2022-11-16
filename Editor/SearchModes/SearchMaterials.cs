using System;
using UnityEditor;
using UnityEngine;

namespace BedtimeCore.SelectionDetective
{
	internal class SearchMaterials : ISearchMode
	{
		public void Filter(Action<string, FilterObject, Type> add, FilterObject fo)
		{
			foreach (Material material in fo.Materials)
			{
				if (material == null)
				{
					continue;
				}

				add(material.name, fo, typeof(Material));
			}	
		}

		public Texture Icon { get; }

		public string Name => "Materials";
		public int Priority { get; }
	}
}
using System;
using UnityEngine;

namespace BedtimeCore.SelectionDetective
{
	internal class SearchShaders : ISearchMode
	{
		public void Filter(Action<string, FilterObject, Type> add, FilterObject fo)
		{
			foreach (Material material in fo.Materials)
			{
				if (material == null)
				{
					continue;
				}

				add(material.shader.name, fo, typeof(Shader));
			}	
		}

		public Texture Icon { get; }

		public string Name => "Shaders";
		public int Priority { get; }
	}
}
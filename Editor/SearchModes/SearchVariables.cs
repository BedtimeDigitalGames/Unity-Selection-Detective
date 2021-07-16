using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BedtimeCore.SelectionDetective
{
	class SearchVariables : ISearchMode
	{
		public SearchVariables()
		{
			Icon = EditorGUIUtility.IconContent("cs Script Icon").image;
		}

		public void Filter(Action<string, FilterObject, Type> add, FilterObject toFilter)
		{
			foreach (var component in toFilter.Components)
			{
				if (component == null)
				{
					continue;
				}
				
				var type = component.GetType();
				var fields = type.GetFields(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic);
				foreach (var fieldInfo in fields)
				{
					if (fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null)
					{
						add(ObjectNames.NicifyVariableName(fieldInfo.Name), toFilter, type);
					}
				}
			}
		}

		public Texture Icon { get; }
		public string Name => "Public Fields";
	}
}
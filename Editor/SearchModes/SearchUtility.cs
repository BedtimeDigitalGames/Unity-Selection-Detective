using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace BedtimeCore.SelectionDetective
{
	internal static class SearchUtility
	{
		private static readonly HashSet<Type> _toSkip = new()
		{
			typeof(SceneAsset),
			typeof(Scene),
			typeof(GameObject),
			typeof(MonoScript),
			typeof(ScriptableObject),
			typeof(Transform),
		};
		
		public static IEnumerable<Object> Find<T>(Object obj, int depth = 0) where T : class
		{
			if (obj == null)
			{
				yield break;
			}
			
			depth++;
			if(depth > 2)
			{
				yield break;
			}
			
			var serializedObject = new SerializedObject(obj);
			var property = serializedObject.GetIterator();
			while (property.NextVisible(true))
			{
				if (property.propertyType != SerializedPropertyType.ObjectReference)
				{
					continue;
				}
				
				if (property.objectReferenceValue == null)
				{
					continue;
				}

				var type = property.objectReferenceValue.GetType();
				
				if (_toSkip.Contains(type) || typeof(ScriptableObject).IsAssignableFrom(type))
				{
					continue;
				}
				
				if (property.objectReferenceValue is not T)
				{
					if(AssetDatabase.Contains(property.objectReferenceValue))
					{
						foreach (var serializedProperty in Find<T>(property.objectReferenceValue, depth))
						{
							yield return serializedProperty;
						}
					}
					continue;
				}

				if (property.isArray)
				{
					for (int i = 0; i < property.arraySize; i++)
					{
						yield return property.GetArrayElementAtIndex(i).objectReferenceValue;
					}
				}
				else
				{
					yield return property.objectReferenceValue;
				}
			}
		}

		public static string GetNameWithoutGenericArity(this Type t)
		{
			string name = t.Name;
			int index = name.IndexOf('`');
			return index == -1 ? name : name.Substring(0, index);
		}
	}
}

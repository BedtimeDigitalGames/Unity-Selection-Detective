using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace BedtimeCore.SelectionDetective
{
	internal static class SearchUtility
	{
		public static IEnumerable<Object> Find<T>(Object obj) where T : class
		{
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

				if (property.objectReferenceValue is not T)
				{
					foreach (var serializedProperty in Find<T>(property.objectReferenceValue))
					{
						yield return serializedProperty;
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

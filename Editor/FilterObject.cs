using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BedtimeCore.SelectionDetective
{
	public sealed class FilterObject : IEquatable<GameObject>
	{
		private static readonly Dictionary<GameObject, FilterObject> ObjectCache = new Dictionary<GameObject, FilterObject>();

		public static void InvalidateCache()
		{
			ObjectCache.Clear();
		}
			
		public static IEnumerable<FilterObject> Get(IEnumerable<Transform> transforms)
		{
			return transforms.Select(t => Get((GameObject) t.gameObject));
		}
			
		public static IEnumerable<FilterObject> Get(IEnumerable<GameObject> gameObjects) => gameObjects.Select(Get);

		public static FilterObject Get(GameObject gameObject)
		{
			if (!ObjectCache.TryGetValue(gameObject, out var filterObject))
			{
				filterObject = new FilterObject(gameObject);
				ObjectCache[gameObject] = filterObject;
			}

			return filterObject;
		}
			
		private FilterObject(GameObject gameObject)
		{
			GameObject = gameObject;
			Components = gameObject.GetComponents<Component>();
		}

		[field: SerializeField]
		public GameObject GameObject { get; }

		[field: SerializeField]
		public Component[] Components { get; }

		public IEnumerable<Material> Materials
		{
			get
			{
				foreach (Component c in Components)
				{
					switch (c)
					{
						case Renderer r: yield return r.sharedMaterial; break;
						case Graphic g: yield return g.materialForRendering; break;
						case Projector p: yield return p.material; break;
					}
				}
			}
		}

		public string Layer => $"{GameObject.layer:D2}: {LayerMask.LayerToName(GameObject.layer)}";

		public string Tag => GameObject.tag;
			
		public string Name => GameObject.name;
		private bool Equals(FilterObject other) => Equals(GameObject, other.GameObject);
		public bool Equals(GameObject other) => GameObject == other;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			return obj is FilterObject other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((GameObject != null ? GameObject.GetHashCode() : 0) * 397);
			}
		}
	}
}
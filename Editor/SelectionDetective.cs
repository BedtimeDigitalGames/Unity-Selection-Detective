using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BedtimeCore.SelectionDetective
{
	internal sealed class SelectionDetective : SearchableEditorWindow
	{
		[MenuItem("GameObject/Selection Detective", false, 0)]
		private static void ShowWindow() => GetWindow<SelectionDetective>().FocusSearch();

		private GameObject[] _selectionOwners = new GameObject[0];
		private IEnumerable<FilterObject> _toBeFiltered = new FilterObject[0];
		private SelectionObject[] _filteredSet = new SelectionObject[0];
		
		[SerializeField]
		private int _searchModeIndex;
		
		[SerializeField]
		private SortMode _sortMode = SortMode.Ascending;

		[SerializeField]
		private bool _lockSelection;

		[SerializeField]
		private bool _includeInactiveGameObjects = true;
		
		[SerializeField]
		private Vector2 _scroll;
		
		private string[] _searchModeNames;
		private static bool _isControllingSelection;
		private static Texture _lockedIcon;
		private static Texture _unlockedIcon;
		private static Texture _takeSelectionIcon;
		private new ISearchMode SearchMode => searchModes[_searchModeIndex];

		private readonly Type _thisType = typeof(SelectionDetective);
		private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
		private Action _drawSearchFieldGUI;
		private Func<string> _getSearchFilter;

		private string SearchField => _getSearchFilter.Invoke();

		public override void OnEnable()
		{
			base.OnEnable();
			titleContent = EditorGUIUtility.TrTextContentWithIcon("Selection Detective", "d_CanvasRenderer Icon");
			_takeSelectionIcon = EditorGUIUtility.IconContent("ArrowNavigationRight").image;
			_lockedIcon = EditorGUIUtility.IconContent("IN LockButton on act").image;
			_unlockedIcon = EditorGUIUtility.IconContent("IN LockButton act").image;
			searchModes = GetSearchModes();
			_searchModeNames = searchModes.Select(s => s.Name).ToArray();
			minSize = new Vector2(200, 140);

			var method = _thisType.GetMethod("SearchFieldGUI", FLAGS, null, new Type[0], new ParameterModifier[0]);
			_drawSearchFieldGUI = () => method?.Invoke(this, null);

			var t = _thisType.GetField("m_SearchFilter", FLAGS);
			_getSearchFilter = () => t?.GetValue(this) as string;
			
			Selection.selectionChanged += OnSelectionChanged;
			EditorApplication.hierarchyChanged -= OnHierarchyChanged;
			EditorApplication.hierarchyChanged += OnHierarchyChanged;
			UpdateOwner();
		}

		private List<ISearchMode> GetSearchModes()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes()).Where(t => typeof(ISearchMode).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToList();
			return types.Select(type => Activator.CreateInstance(type) as ISearchMode).ToList();
		}

		private void OnHierarchyChanged()
		{
			if (Application.isPlaying)
			{
				return;
			}
			
			FilterObject.InvalidateCache();
			if (this != null)
			{
				OnSelectionChanged();
			}
		}

		public override void OnDisable()
		{
			base.OnDisable();
			Selection.selectionChanged -= OnSelectionChanged;
		}

		private void OnSelectionChanged()
		{
			if ((_isControllingSelection && mouseOverWindow is SelectionDetective) || _lockSelection)
			{
				_isControllingSelection = false;
				return;
			}

			UpdateOwner();
		}

		private void OnGUI()
		{
			DrawToolbar();
			DrawHeader();
			DrawOwners();
			DrawList();
		}

		private void DrawOwners()
		{
			using (new EditorGUILayout.VerticalScope("Box"))
			{
				if (_selectionOwners?.Length > 0)
				{
					const float SAFE_ZONE = 12;
					const float ICON_WIDTH = 16f;
					var type = AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
					
					EditorGUILayout.BeginHorizontal();
					
					float totalSize = SAFE_ZONE;
					foreach (var o in _selectionOwners)
					{
						totalSize += ICON_WIDTH + GUI.skin.label.CalcSize(new GUIContent(o.name)).x;
					}

					if (totalSize >= Screen.width)
					{
						EditorGUI.BeginDisabledGroup(true);
						EditorGUILayout.TextArea($"{_selectionOwners.Length} Selection(s)", EditorStyles.centeredGreyMiniLabel);
						EditorGUI.EndDisabledGroup();
					}
					else
					{
						GUILayout.FlexibleSpace();
						foreach (var o in _selectionOwners)
						{
							var maxWidth = ICON_WIDTH + GUI.skin.label.CalcSize(new GUIContent(o.name)).x;
							if (GUILayout.Button(new GUIContent(o.name, type), EditorStyles.miniLabel, GUILayout.MaxWidth(maxWidth), GUILayout.ExpandWidth(false), GUILayout.Height(ICON_WIDTH)))
							{
								Selection.activeObject = o;
							}
						}
						GUILayout.FlexibleSpace();
					}
				
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					EditorGUILayout.LabelField("Nothing Selected", EditorStyles.centeredGreyMiniLabel);
				}
			}
		}

		private void DrawHeader()
		{
			EditorGUI.BeginChangeCheck();
			using (new EditorGUILayout.VerticalScope("Box"))
			{
				var labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 55;
				_searchModeIndex = EditorGUILayout.Popup(_searchModeIndex, _searchModeNames);
				_sortMode = (SortMode) EditorGUILayout.EnumPopup("Sorting", _sortMode);
				using (new EditorGUILayout.HorizontalScope())
				{
					_includeInactiveGameObjects = EditorGUILayout.Toggle(_includeInactiveGameObjects, GUILayout.Width(12));
					EditorGUIUtility.labelWidth = 200;
					EditorGUILayout.LabelField("Show Inactive Children");
				}

				EditorGUIUtility.labelWidth = labelWidth;
			}

			if (EditorGUI.EndChangeCheck())
			{
				UpdateFilter();
			}
		}

		private void UpdateOwner(bool keepSelection = false)
		{
			_selectionOwners = keepSelection ? _selectionOwners : Selection.gameObjects;
			_toBeFiltered = FilterObject.Get(_selectionOwners.SelectMany(s => s.GetComponentsInChildren<Transform>(_includeInactiveGameObjects)));
			UpdateFilter();
			Repaint();
		}

		private void UpdateFilter()
		{
			if (_toBeFiltered == null)
			{
				return;
			}
			_filteredSet = SelectionObject.Filter(SearchMode, _sortMode, _toBeFiltered).ToArray();
			UpdateSearch();
		}

		private void UpdateSearch()
		{
			_searchFilteredSet = _filteredSet;
			if (!string.IsNullOrEmpty(SearchField))
			{
				_searchFilteredSet = _searchFilteredSet.Where(s => StringContains(s.Label, SearchField)).ToArray();
			}
		}

		private bool StringContains(string source, string toCompare) => source.IndexOf(toCompare, StringComparison.OrdinalIgnoreCase) >= 0;
		
		private void DrawList()
		{
			var objects = _searchFilteredSet;
			var buttonHeight = SelectionObject.BUTTON_HEIGHT;
			scrollHeightMax = (int) ((position.height - PRE_SCROLL_HEIGHT) / buttonHeight);
			GUI.SetNextControlName("DetectiveMainArea");
			_scroll = GUILayout.BeginScrollView(_scroll, GUIStyle.none, GUI.skin.GetStyle("VerticalScrollbar"), GUILayout.ExpandHeight(true));
			_scroll = new Vector2(_scroll.x, _scroll.y - _scroll.y % buttonHeight);
			int firstIndex = (int) (_scroll.y / buttonHeight);
			firstIndex = Mathf.Clamp(firstIndex,0,Mathf.Max(0,objects.Length - scrollHeightMax));

			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			GUILayout.Space(firstIndex * buttonHeight);
			
			for (var i = firstIndex; i < Mathf.Min(objects.Length, firstIndex+scrollHeightMax); i++)
			{
				SelectionObject o = objects[i];
				o.Draw(searchModes[_searchModeIndex], SearchField);
			}
			
			GUILayout.Space(Mathf.Max(0,(objects.Length-firstIndex-scrollHeightMax) * buttonHeight));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();

			if (!(mouseOverWindow is SelectionDetective))
			{
				return;
			}

			if (Event.current.type == EventType.MouseDown)
			{
				EditorGUI.FocusTextInControl("");
			}
			try
			{
				mouseOverWindow.Repaint();
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			LockStyle.active = _lockSelection ? EditorStyles.toolbarButton.normal : EditorStyles.toolbarButton.active;
			LockStyle.normal = !_lockSelection ? EditorStyles.toolbarButton.normal : EditorStyles.toolbarButton.active;
			
			var lockVisuals = new GUIContent(_lockSelection ? _lockedIcon : _unlockedIcon, "Lock current view");
			var takeSelectionVisuals = new GUIContent(_takeSelectionIcon, "Reset view onto all current selections");

			EditorGUI.BeginDisabledGroup(!_selectionOwners.Any());
			
			EditorGUI.BeginDisabledGroup(Selection.gameObjects.All(g => _selectionOwners.Contains(g)));
			if (GUILayout.Button(takeSelectionVisuals, EditorStyles.toolbarButton, GUILayout.Width(32)))
			{
				UpdateOwner();
			}
			EditorGUI.EndDisabledGroup();
			
			if (GUILayout.Button(lockVisuals, LockStyle, GUILayout.Width(32)))
			{
				_lockSelection = !_lockSelection ;
				if (!_lockSelection)
				{
					UpdateOwner(true);
				}
			}
			EditorGUI.EndDisabledGroup();
			
			EditorGUILayout.Space();

			var oldSearch = SearchField;
			_drawSearchFieldGUI.Invoke();

			if (oldSearch != SearchField)
			{
				UpdateSearch();
			}
			
			EditorGUILayout.EndHorizontal();
		}

		private void FocusSearch() => _thisType.GetMethod("FocusSearchField",FLAGS)?.Invoke(this, null);

		public sealed class SelectionObject
		{
			public string Label { get; }
			private static HashSet<SelectionObject> Selected { get; } = new HashSet<SelectionObject>();
			private List<FilterObject> Content { get; }
			
			private Type Type { get; set; }

			private GUIStyle Style
			{
				get
				{
					SetupStyle();
					return Selected.Contains(this) ? buttonStyleDown : buttonStyleNormal;
				}
			}

			private void Select(SelectionType selectionType = SelectionType.Exclusive)
			{
				_isControllingSelection = true;
				
				switch (selectionType)
				{
					case SelectionType.Exclusive:
						Selected.Clear();
						Selection.objects = Content.Select(c => c.GameObject as UnityEngine.Object).ToArray();
						break;
					case SelectionType.Concatenate:
						Selection.objects = Selection.objects.Concat(Content.Select(c => c.GameObject).ToArray()).ToArray();
						break;
				}

				Selected.Add(this);
			}

			private static bool StringContains(string source, string toCompare) => source.IndexOf(toCompare, StringComparison.OrdinalIgnoreCase) >= 0;
			
			public void Draw(ISearchMode mode, string searchFilter)
			{
				if (!string.IsNullOrEmpty(searchFilter) && !StringContains(Label, searchFilter))
				{
					return;
				}
				
				var selectedColor = new Color(.6f, .6f, .6f, 1f);
				var orgBGColor = GUI.backgroundColor;
				
				Texture icon = null;
				if (Type != null)
				{
					icon = AssetPreview.GetMiniTypeThumbnail(Type);
				}
				if(icon == null)
				{
					icon = mode.Icon;
				}

				GUIContent info = new GUIContent(Label, icon);

				if (Selected.Contains(this))
				{
					GUI.backgroundColor = selectedColor;
				}

				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button(info, Style))
				{
					Select(Event.current.control ? SelectionType.Concatenate : SelectionType.Exclusive);
				}
				EditorGUILayout.EndHorizontal();
				
				GUI.backgroundColor = orgBGColor;
			}
			
			public static IEnumerable<SelectionObject> Filter(ISearchMode mode, SortMode sort, IEnumerable<FilterObject> filterObjects)
			{
				Selected.Clear();
				var dict = new Dictionary<string, SelectionObject>(32);
				
				void Add(string key, FilterObject value, Type type = null)
				{
					if(!dict.TryGetValue(key, out var selectionObject))
					{
						selectionObject = new SelectionObject(key);
					}

					selectionObject.Type = type;
					selectionObject.Content.Add(value);
					dict[key] = selectionObject;
				}
				
				foreach (FilterObject fo in filterObjects)
				{
					mode.Filter(Add, fo);
				}

				IEnumerable<SelectionObject> output;
				switch (sort)
				{
					case SortMode.Ascending: output = dict.Values.OrderBy(v => v.Label); break;
					case SortMode.Random: output = dict.Values.OrderBy(v => v.GetHashCode()); break;
					default: output = dict.Values.OrderByDescending(v => v.Label); break;
				}

				return output;
			}
			
			private SelectionObject(string label)
			{
				Label = label;
				Content = new List<FilterObject>();
			}
			
			private void SetupStyle()
			{
				if (buttonStyleNormal != null)
				{
					return;
				}

				var hoverColor = Color.yellow + Color.white * .5f;
				var activeColor = Color.green + Color.white * .5f;
				
				buttonStyleNormal = new GUIStyle(EditorStyles.toolbarButton);
				buttonStyleNormal.alignment = TextAnchor.MiddleLeft;
				buttonStyleNormal.hover.background = buttonStyleNormal.normal.background;
				buttonStyleNormal.hover.textColor = hoverColor;
				buttonStyleNormal.fixedHeight = BUTTON_HEIGHT;
				
				buttonStyleDown = new GUIStyle(buttonStyleNormal);
				buttonStyleDown.normal = buttonStyleNormal.onActive;
				buttonStyleDown.normal.textColor = activeColor;
				buttonStyleDown.hover.background = buttonStyleDown.normal.background;
				buttonStyleDown.hover.textColor = activeColor;
				buttonStyleDown.onActive = buttonStyleNormal.normal;
			}

			private static GUIStyle buttonStyleNormal;
			private static GUIStyle buttonStyleDown;
			public const float BUTTON_HEIGHT = 16f;

			private enum SelectionType
			{
				Exclusive,
				Concatenate,
			}
		}

		public enum SortMode
		{
			Ascending,
			Descending,
			Random,
		}

		private List<ISearchMode> searchModes = new List<ISearchMode>();

		private static GUIStyle lockStyle;

		private static GUIStyle takeSelectionStyle;
		private int scrollHeightMax;
		private SelectionObject[] _searchFilteredSet;
		private const float PRE_SCROLL_HEIGHT = 46;

		private GUIStyle TakeSelectionStyle
		{
			get
			{
				if (takeSelectionStyle == null)
				{
					takeSelectionStyle = new GUIStyle(EditorStyles.toolbarButton);
					takeSelectionStyle.alignment = TextAnchor.MiddleCenter;
					takeSelectionStyle.contentOffset = new Vector2(0f, 0f);
				}

				return takeSelectionStyle;
			}
		}

		private GUIStyle LockStyle
		{
			get
			{
				if (lockStyle == null)
				{
					lockStyle = new GUIStyle(EditorStyles.toolbarButton);
					lockStyle.alignment = TextAnchor.MiddleCenter;
					lockStyle.contentOffset = new Vector2(-2f, -2f);
				}

				return lockStyle;
			}
		}
	}
}
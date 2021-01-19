using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BedtimeCore.Editor;
using BedtimeCore.Reflection;
using UnityEditor;
using UnityEngine;

namespace BedtimeCore.SelectionDetective
{
	internal sealed class SelectionDetective : SearchableEditorWindow
	{
		[MenuItem("GameObject/Selection Detective", false, 0)]
		private static void ShowWindow() => GetWindow<SelectionDetective>().FocusSearch();

		private GameObject[] selectionOwners = new GameObject[0];
		private IEnumerable<FilterObject> toBeFiltered = new FilterObject[0];
		private IEnumerable<SelectionObject> filteredSet = new SelectionObject[0];
		
		[SerializeField]
		private int searchModeIndex;
		
		[SerializeField]
		private SortMode sortMode = SortMode.Ascending;

		[SerializeField]
		private bool lockSelection;

		[SerializeField]
		private bool includeInactiveGameObjects = true;
		
		[SerializeField]
		private Vector2 scroll;
		
		private string[] searchModeNames;
		private static bool isControllingSelection;
		private static Texture lockedIcon;
		private static Texture unlockedIcon;
		private static Texture takeSelectionIcon;
		private static Texture collapseIcon;
		
		private new ISearchMode SearchMode => searchModes[searchModeIndex];
		private string SearchField
		{
			get => this.GetValue<string>("m_SearchFilter");
		}
		
		public override void OnEnable()
		{
			base.OnEnable();
			titleContent = typeof(EditorGUIUtility).InvokeMethod<GUIContent, string, string>("TextContentWithIcon", "Selection Detective", "d_CanvasRenderer Icon");

			collapseIcon = EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow").image;
			takeSelectionIcon = EditorGUIUtility.IconContent("ArrowNavigationRight").image;
			lockedIcon = EditorGUIUtility.IconContent("IN LockButton on act").image;
			unlockedIcon = EditorGUIUtility.IconContent("IN LockButton act").image;
			searchModes = GetSearchModes();
			searchModeNames = searchModes.Select(s => s.Name).ToArray();
			minSize = new Vector2(200, 140);
			
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
			if (isControllingSelection || lockSelection)
			{
				isControllingSelection = false;
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
				if (selectionOwners?.Length > 0)
				{
					const float SAFE_ZONE = 12;
					const float ICON_WIDTH = 16f;
					var type = AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
					
					EditorGUILayout.BeginHorizontal();
					
					float totalSize = SAFE_ZONE;
					foreach (var o in selectionOwners)
					{
						totalSize += ICON_WIDTH + GUI.skin.label.CalcSize(new GUIContent(o.name)).x;
					}

					if (totalSize >= Screen.width)
					{
						EditorGUILayout.TextArea($"{selectionOwners.Length} Selection(s)", EditorStyles.centeredGreyMiniLabel);
					}
					else
					{
						GUILayout.FlexibleSpace();
						foreach (var o in selectionOwners)
						{
							var maxWidth = ICON_WIDTH + GUI.skin.label.CalcSize(new GUIContent(o.name)).x;
							GUILayout.Button(new GUIContent(o.name, type), EditorStyles.miniLabel, GUILayout.MaxWidth(maxWidth), GUILayout.ExpandWidth(false), GUILayout.Height(ICON_WIDTH));
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
				// searchModeNames = (SearchMode)EditorGUILayout.EnumPopup("Mode", searchMode);
				searchModeIndex = EditorGUILayout.Popup(searchModeIndex, searchModeNames);
				sortMode = (SortMode) EditorGUILayout.EnumPopup("Sorting", sortMode);
				using (new EditorGUILayout.HorizontalScope())
				{
					includeInactiveGameObjects = EditorGUILayout.Toggle(includeInactiveGameObjects, GUILayout.Width(12));
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
			selectionOwners = keepSelection ? selectionOwners : Selection.gameObjects;
			toBeFiltered = FilterObject.Get(selectionOwners.SelectMany(s => s.GetComponentsInChildren<Transform>(includeInactiveGameObjects)));
			UpdateFilter();
			Repaint();
		}

		private void UpdateFilter()
		{
			if (toBeFiltered == null)
			{
				return;
			}
			filteredSet = SelectionObject.Filter(SearchMode, sortMode,  toBeFiltered);
		}

		private void DrawList()
		{
			GUI.SetNextControlName("DetectiveMainArea");
			scroll = GUILayout.BeginScrollView(scroll, GUIStyle.none, GUI.skin.GetStyle("VerticalScrollbar"));
			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			foreach (SelectionObject o in filteredSet)
			{
				o.Draw(searchModes[searchModeIndex], SearchField);
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			
			if (mouseOverWindow is SelectionDetective)
			{
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
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			LockStyle.active = lockSelection ? EditorStyles.toolbarButton.normal : EditorStyles.toolbarButton.active;
			LockStyle.normal = !lockSelection ? EditorStyles.toolbarButton.normal : EditorStyles.toolbarButton.active;
			
			var lockVisuals = new GUIContent(lockSelection ? lockedIcon : unlockedIcon, "Lock current view");
			var takeSelectionVisuals = new GUIContent(takeSelectionIcon, "Reset view onto all current selections");
			var collapseVisuals = new GUIContent(collapseIcon, "Collapse the hierarchy");

			EditorGUI.BeginDisabledGroup(!selectionOwners.Any());
			
			EditorGUI.BeginDisabledGroup(Selection.gameObjects.All(g => selectionOwners.Contains(g)));
			if (GUILayout.Button(takeSelectionVisuals, EditorStyles.toolbarButton, GUILayout.Width(32)))
			{
				UpdateOwner();
			}
			EditorGUI.EndDisabledGroup();
			
			if (GUILayout.Button(lockVisuals, LockStyle, GUILayout.Width(32)))
			{
				lockSelection = !lockSelection ;
				if (!lockSelection)
				{
					UpdateOwner(true);
				}
			}
			EditorGUI.EndDisabledGroup();
			
			if (GUILayout.Button(collapseVisuals, EditorStyles.toolbarButton))
			{
				foreach (EditorWindow editorWindow in TreeViewHelper.GetSceneHierarchyWindows())
				{
					TreeViewCollapser.CollapseHierarchy(editorWindow);
				}
			}

			EditorGUILayout.Space();
			this.InvokeVoid("SearchFieldGUI");

			EditorGUILayout.EndHorizontal();
		}

		private void FocusSearch()
		{
			this.InvokeVoid("FocusSearchField");
		}

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
				isControllingSelection = true;
				
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

			private bool StringContains(string source, string toCompare) => source.IndexOf(toCompare, StringComparison.OrdinalIgnoreCase) >= 0;
			
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

				switch (sort)
				{
					case SortMode.Ascending: return dict.Values.OrderBy(v => v.Label);
					case SortMode.Random: return dict.Values.OrderBy(v => v.GetHashCode());
					default: return dict.Values.OrderByDescending(v => v.Label);
				}
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
				buttonStyleNormal.fixedHeight = 16;
				
				buttonStyleDown = new GUIStyle(buttonStyleNormal);
				buttonStyleDown.normal = buttonStyleNormal.onActive;
				buttonStyleDown.normal.textColor = activeColor;
				buttonStyleDown.hover.background = buttonStyleDown.normal.background;
				buttonStyleDown.hover.textColor = activeColor;
				buttonStyleDown.onActive = buttonStyleNormal.normal;
			}

			private static GUIStyle buttonStyleNormal;
			private static GUIStyle buttonStyleDown;

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
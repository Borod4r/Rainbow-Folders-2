using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Borodar.RainbowFolders.ProjectWindowAdapter.ViewMode;

namespace Borodar.RainbowFolders
{
    [InitializeOnLoad]
    public class RainbowFoldersGUI
    {
        private const double WINDOWS_UPDATE_DELAY = 0.2;
        private const float SMALL_ICON_SIZE = 16f;

        private static readonly Color ROW_SHADING_COLOR = new Color(0f, 0f, 0f, 0.03f);

        private static readonly Dictionary<object, Action<int, Rect>> ON_GUI_ROW_CALLBACKS = new Dictionary<object, Action<int, Rect>>();
        private static readonly Dictionary<object, Action> REPAINT_CALLBACKS = new Dictionary<object, Action>();
        private static readonly HashSet<int> WINDOW_HASH_SET = new HashSet<int>();

        private static Vector2Int _projectWindowsCount = Vector2Int.zero;

        private static bool _multiSelection;
        private static double _nextWindowsUpdate;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        static RainbowFoldersGUI()
        {
            EditorApplication.projectWindowItemOnGUI += DrawRowShading;
            EditorApplication.projectWindowItemOnGUI += DrawEditIcon;
            EditorApplication.update += Update;

            var target = typeof(RainbowFoldersGUI);
            const string method = nameof(DrawIconInObjectList);
            ProjectWindowAdapter.AddPostAssetIconDrawCallback(target, method);
        }

        //---------------------------------------------------------------------
        // Delegates
        //---------------------------------------------------------------------

        private static void DrawRowShading(string guid, Rect rect)
        {
            if (!ProjectPreferences.DrawRowShading || !IsIconSmall(rect)) return;

            var isOdd = Mathf.FloorToInt(((rect.y - 4) / 16) % 2) != 0;
            if (isOdd) return;

            var drawArea = new Rect(rect);
            drawArea.width += rect.x + 16f;
            drawArea.height += 1f;
            drawArea.x = 0f;

            // Background
            EditorGUI.DrawRect(drawArea, ROW_SHADING_COLOR);
            // Top line
            drawArea.height = 1f;
            EditorGUI.DrawRect(drawArea, ROW_SHADING_COLOR);
            // Bottom line
            drawArea.y += 16f;
            EditorGUI.DrawRect(drawArea, ROW_SHADING_COLOR);
        }

        private static void DrawEditIcon(string guid, Rect rect)
        {
            if (!ProjectPreferences.IsEditModifierPressed(Event.current))
            {
                _multiSelection = false;
                return;
            }

            var isSmall = IsIconSmall(rect);
            var iconRect = GetIconRect(rect, isSmall);
            var isMouseOver = rect.Contains(Event.current.mousePosition);
            _multiSelection = (IsSelected(guid)) ? isMouseOver || _multiSelection : !isMouseOver && _multiSelection;

            // if mouse is not over current folder icon or selected group
            if (!isMouseOver && (!IsSelected(guid) || !_multiSelection)) return;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(path)) return;

            var editIcon = ProjectEditorUtility.GetEditFolderIcon(isSmall);
            GUI.DrawTexture(iconRect, editIcon);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                ShowPopupWindow(iconRect, path);
            }

            EditorApplication.RepaintProjectWindow();
        }

        private static void Update()
        {
            if (_nextWindowsUpdate > EditorApplication.timeSinceStartup) return;
            _nextWindowsUpdate = EditorApplication.timeSinceStartup + WINDOWS_UPDATE_DELAY;

            var projectWindows = ProjectWindowAdapter.GetAllProjectWindows();

            if (!ProjectWindowsChanged(projectWindows)) return;
            _projectWindowsCount = Vector2Int.zero;

            RemoveOldCallbacks();
            AddNewCallbacks(projectWindows);
        }

        private static void DrawIconInObjectList(Rect iconRect, string guid, bool isListMode)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(path)) return;

            var ruleset = ProjectRuleset.Instance;
            if (ruleset == null) return;

            var rule = ProjectRuleset.Instance.GetRuleByPath(path, true);
            if (rule == null) return;

            DrawBackgroundInSecondColumn(iconRect, rule, isListMode);
        }

        //---------------------------------------------------------------------
        // GUI
        //--------------------------------------------------------------------

        private static void DrawFoldouts(Rect rect, int id, EditorWindow window)
        {
            if (!ProjectPreferences.ShowProjectTree) return;

            const float textureWidth = 128f;

            var fx = Mathf.Max(0, rect.x - textureWidth - 16f);
            var fw = Mathf.Min(textureWidth, rect.x - 16f);
            var foldoutRect = new Rect(rect) {width = fw, x = fx};

            var tw = foldoutRect.width / textureWidth;
            var texCoords = new Rect(1 - tw, 0, tw, 1f);

            GUI.DrawTextureWithTexCoords(foldoutRect, ProjectEditorUtility.GetFoldoutLevelsIcon(), texCoords);

            if (IsRootItem(rect) || ProjectWindowAdapter.HasChildren(window, id)) return;

            foldoutRect.width = 16f;
            foldoutRect.x = rect.x - 16f;
            GUI.DrawTexture(foldoutRect, ProjectEditorUtility.GetFoldoutIcon());
        }

        private static void DrawIconInFirstColumn(object controller, object state, Rect rect, int assetId, string path)
        {
            if (!AssetDatabase.IsValidFolder(path)) return;

            var ruleset = ProjectRuleset.Instance;
            if (ruleset == null) return;

            var rule = ProjectRuleset.Instance.GetRuleByPath(path, true);
            if (rule == null) return;

            // Background
            var selected = ProjectWindowAdapter.IsItemSelected(controller, state, assetId);
            if (!selected) DrawBackgroundInFirstColumn(rect, rule);
            // Icon
            DrawCustomIcon(controller, rule, selected, true, rect);
        }

        private static void DrawBackgroundInFirstColumn(Rect rect, ProjectRule rule)
        {
            if (rule == null || !rule.HasBackground()) return;

            var backgroundRect = new Rect(rect);
            backgroundRect.x += SMALL_ICON_SIZE + 1f;
            backgroundRect.width -= SMALL_ICON_SIZE + 1f;

            var backgroundTex = (rule.HasCustomBackground())
                ? rule.BackgroundTexture
                : ProjectBackgroundsStorage.GetBackground(rule.BackgroundType);

            GUI.DrawTexture(backgroundRect, backgroundTex);
        }

        private static void DrawBackgroundInSecondColumn(Rect rect, ProjectRule rule, bool isListMode)
        {
            if (rule == null || !rule.HasBackground()) return;

            var backgroundRect = isListMode
                ? new Rect(rect) {x = rect.x + rect.width + 2f, width = 200f}
                : new Rect(rect) {y = rect.y + rect.height, height = 16f};

            var backgroundTex = (rule.HasCustomBackground())
                ? rule.BackgroundTexture
                : ProjectBackgroundsStorage.GetBackground(rule.BackgroundType);

            GUI.DrawTexture(backgroundRect, backgroundTex);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Local")]
        private static bool ProjectWindowsChanged(IReadOnlyList<EditorWindow> projectWindows)
        {
            var actualWindowsCount = Vector2Int.zero;

            foreach (var window in projectWindows)
            {
                if (!ProjectWindowAdapter.ProjectWindowInitialized(window)) continue;
                if (!WINDOW_HASH_SET.Contains(window.GetHashCode())) return true;

                if (ProjectWindowAdapter.GetProjectViewMode(window) == OneColumn)
                {
                    actualWindowsCount.x++;
                }
                else
                {
                    actualWindowsCount.y++;
                }
            }

            return _projectWindowsCount != actualWindowsCount;
        }

        private static void RemoveOldCallbacks()
        {
            foreach (var callback in ON_GUI_ROW_CALLBACKS)
            {
                ProjectWindowAdapter.RemoveOnGUIRowCallback(callback.Key, callback.Value);
            }

            ON_GUI_ROW_CALLBACKS.Clear();

            foreach (var callback in REPAINT_CALLBACKS)
            {
                ProjectWindowAdapter.RemoveRepaintCallback(callback.Key, callback.Value);
            }

            REPAINT_CALLBACKS.Clear();
            WINDOW_HASH_SET.Clear();
        }

        [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Local")]
        private static void AddNewCallbacks(IReadOnlyList<EditorWindow> projectWindows)
        {
            foreach (var window in projectWindows)
            {
                if (!ProjectWindowAdapter.ProjectWindowInitialized(window)) continue;

                WINDOW_HASH_SET.Add(window.GetHashCode());

                object controller;
                if (ProjectWindowAdapter.GetProjectViewMode(window) == OneColumn)
                {
                    controller = ProjectWindowAdapter.GetAssetTreeController(window);
                    _projectWindowsCount.x++;
                }
                else
                {
                    controller = ProjectWindowAdapter.GetFolderTreeController(window);
                    _projectWindowsCount.y++;
                }

                // First Column
                var state = ProjectWindowAdapter.GetTreeViewState(controller);

                void OnGUIRowCallback(int id, Rect rect)
                {
                    DrawFoldouts(rect, id, window);

                    var path = AssetDatabase.GetAssetPath(id);
                    DrawIconInFirstColumn(controller, state, rect, id, path);
                }

                ProjectWindowAdapter.AddOnGUIRowCallback(controller, OnGUIRowCallback);
                ON_GUI_ROW_CALLBACKS.Add(controller, OnGUIRowCallback);

                // Second Column
                var objectListArea = ProjectWindowAdapter.GetObjectListArea(window);

                void ListAreaRepaintCallback()
                {
                    ProjectWindowAdapter.ReplaceIconsInListArea(objectListArea, ProjectRuleset.Instance);
                }

                // Call repaint callback manually first time to make sure
                // icons in second column are applied after recompilation
                ListAreaRepaintCallback();

                ProjectWindowAdapter.AddRepaintCallback(objectListArea, ListAreaRepaintCallback);
                REPAINT_CALLBACKS.Add(objectListArea, ListAreaRepaintCallback);
            }
        }

        private static void ShowPopupWindow(Rect rect, string path)
        {
            var window = ProjectPopupWindow.GetDraggableWindow();
            var position = GUIUtility.GUIToScreenPoint(rect.position + new Vector2(0, rect.height + 2));

            if (_multiSelection)
            {
                // ReSharper disable once RedundantTypeArgumentsOfMethod
                var paths = Selection.assetGUIDs
                    .Select<string, string>(AssetDatabase.GUIDToAssetPath)
                    .Where(AssetDatabase.IsValidFolder).ToList();

                var index = paths.IndexOf(path);
                window.ShowWithParams(position, paths, index);
            }
            else
            {
                window.ShowWithParams(position, new List<string> {path}, 0);
            }
        }

        private static void DrawCustomIcon(object controller, ProjectRule rule, bool selected, bool isSmall, Rect rect)
        {
            if (!rule.HasIcon()) return;

            Texture2D iconTex = null;
            if (rule.HasCustomIcon())
            {
                iconTex = isSmall ? rule.SmallIcon : rule.LargeIcon;
            }
            else
            {
                var icons = ProjectIconsStorage.GetIcons(rule.IconType);
                if (icons != null)
                {
                    iconTex = isSmall ? icons.Item2 : icons.Item1;
                }
            }

            if (iconTex == null) return;

            rect.width = rect.height;

            Color backgroundColor;
            if (selected)
            {
                var hasFocus = ProjectWindowAdapter.HasFocus(controller);
                backgroundColor = ColorHelper.GetSelectionColor(hasFocus);
            }
            else
            {
                backgroundColor = ColorHelper.GetBackgroundColor();
            }

            EditorGUI.DrawRect(rect, backgroundColor);
            GUI.DrawTexture(rect, iconTex);
        }

        private static bool IsIconSmall(Rect rect)
        {
            return rect.width > rect.height;
        }

        private static Rect GetIconRect(Rect rect, bool isSmall)
        {
            if (isSmall)
                rect.width = rect.height;
            else
                rect.height = rect.width;

            return rect;
        }

        private static bool IsSelected(string guid)
        {
            return Selection.assetGUIDs.Contains(guid);
        }

        private static bool IsRootItem(Rect rect)
        {
            return rect.x <= 20f;
        }
    }
}
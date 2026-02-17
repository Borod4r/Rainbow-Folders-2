using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Borodar.RainbowFolders.ColorHelper;
using KeyType = Borodar.RainbowFolders.ProjectRule.KeyType;

namespace Borodar.RainbowFolders
{
    public class ProjectPopupWindow : DraggablePopupWindow
    {
        private const float PADDING = 8f;
        private const float SPACING = 1f;
        private const float LINE_HEIGHT = 18f;
        private const float LABELS_WIDTH = 85f;
        private const float PREVIEW_SIZE_SMALL = 16f;
        private const float PREVIEW_SIZE_LARGE = 64f;
        private const float BUTTON_WIDTH = 55f;
        private const float BUTTON_WIDTH_SMALL = 16f;

        private const float WINDOW_WIDTH = 325f;
        private const float WINDOW_HEIGHT = 176f;

        private static readonly Vector2 WINDOW_SIZE = new(WINDOW_WIDTH, WINDOW_HEIGHT);

        private Rect _windowRect;
        private Rect _backgroundRect;

        private List<string> _paths;
        private ProjectRuleset _ruleset;
        private ProjectRule[] _existingItems;
        private ProjectRule _currentRule;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static ProjectPopupWindow GetDraggableWindow()
        {
            return GetDraggableWindow<ProjectPopupWindow>();
        }

        public void ShowWithParams(Vector2 inPosition, List<string> paths, int pathIndex)
        {
            _paths = paths;
            _ruleset = ProjectRuleset.Instance;

            var size = paths.Count;
            _existingItems = new ProjectRule[size];
            _currentRule = new ProjectRule(KeyType.Path, paths[pathIndex]);

            for (var i = 0; i < size; i++)
                _existingItems[i] = _ruleset.GetRuleByPath(paths[i]);

            if (_existingItems[pathIndex] != null)
                _currentRule.CopyFrom(_existingItems[pathIndex]);

            // Resize

            var customIconHeight = (_currentRule.HasCustomIcon()) ? LINE_HEIGHT * 2f : 0f;
            var customBackgroundHeight = (_currentRule.HasCustomBackground()) ? LINE_HEIGHT : 0f;

            var rect = new Rect(inPosition, WINDOW_SIZE)
            {
                height = WINDOW_HEIGHT + customIconHeight + customBackgroundHeight
            };

            _windowRect = new Rect(Vector2.zero, rect.size);
            _backgroundRect = new Rect(Vector2.one, rect.size - new Vector2(2f, 2f));

            Show<ProjectPopupWindow>(rect);
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnGUI()
        {
            if (_currentRule == null) { Close(); return; }

            base.OnGUI();

            ChangeWindowSize(_currentRule.HasCustomIcon(), _currentRule.HasCustomBackground());
            var rect = _windowRect;

            // Background

            var isProSkin = EditorGUIUtility.isProSkin;

            var borderColor = isProSkin ? POPUP_BORDER_CLR_PRO : POPUP_BORDER_CLR_FREE;
            EditorGUI.DrawRect(_windowRect, borderColor);

            var backgroundColor = isProSkin ? POPUP_BACKGROUND_CLR_PRO : POPUP_BACKGROUND_CLR_FREE;
            EditorGUI.DrawRect(_backgroundRect, backgroundColor);

            // Body

            DrawLabels(ref rect, _currentRule);
            DrawValues(ref rect, _currentRule, _paths);
            DrawPreview(ref rect, _currentRule, isProSkin);
            DrawSeparators(ref rect);
            DrawButtons(ref rect);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override bool IsDragModifierPressed(Event e)
        {
            return ProjectPreferences.IsDragModifierPressed(e);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void DrawLabels(ref Rect rect, ProjectRule rule)
        {
            rect.x += 0.5f * PADDING;
            rect.y += PADDING;
            rect.width = LABELS_WIDTH - PADDING;
            rect.height = LINE_HEIGHT;

            rule.Type = (KeyType)EditorGUI.EnumPopup(rect, rule.Type);

            rect.y += LINE_HEIGHT + 3f;
            EditorGUI.LabelField(rect, "Priority");

            rect.y += LINE_HEIGHT + 12f;
            EditorGUI.LabelField(rect, "Icon");

            if (rule.HasCustomIcon())
            {
                rect.y += LINE_HEIGHT + 4f;
                EditorGUI.LabelField(rect, "x16", EditorStyles.miniLabel);
                rect.y += LINE_HEIGHT + 2f;
                EditorGUI.LabelField(rect, "x64", EditorStyles.miniLabel);
            }

            rect.y += LINE_HEIGHT + 2f;
            EditorGUI.LabelField(rect, "Recursive", EditorStyles.miniLabel);

            rect.y += LINE_HEIGHT + 6f;
            EditorGUI.LabelField(rect, "Background");

            if (rule.HasCustomBackground())
            {
                rect.y += LINE_HEIGHT + 4f;
                EditorGUI.LabelField(rect, "x16", EditorStyles.miniLabel);
            }

            rect.y += LINE_HEIGHT + 2f;
            EditorGUI.LabelField(rect, "Recursive", EditorStyles.miniLabel);            
        }

        private void DrawValues(ref Rect rect, ProjectRule rule, IList<string> paths)
        {
            rect.x += LABELS_WIDTH;
            rect.y = _windowRect.y + PADDING;
            rect.width = _windowRect.width - LABELS_WIDTH - PADDING;

            GUI.enabled = false;
            if (paths.Count == 1)
                rule.Key = (rule.Type == KeyType.Path) ? paths[0] : Path.GetFileName(paths[0]);
            else
                rule.Key = "---";
            EditorGUI.TextField(rect, GUIContent.none, rule.Key);
            GUI.enabled = true;

            rect.y += LINE_HEIGHT + 3f;
            rule.Priority = EditorGUI.IntField(rect, GUIContent.none, rule.Priority);
            
            rect.width -= PREVIEW_SIZE_LARGE + PADDING;
            rect.y += LINE_HEIGHT + 5f * SPACING + 7f;

            DrawIconsPopup(rect, rule);

            if (rule.HasCustomIcon())
            {
                rect.y += LINE_HEIGHT + 2f + SPACING;
                rule.SmallIcon = (Texture2D) EditorGUI.ObjectField(rect, rule.SmallIcon, typeof(Texture2D), false);

                rect.y += LINE_HEIGHT + 1f + SPACING;
                rule.LargeIcon = (Texture2D) EditorGUI.ObjectField(rect, rule.LargeIcon, typeof(Texture2D), false);
                rect.y += 1f;
            }

            rect.y += LINE_HEIGHT + 2f;
            rule.IsIconRecursive = EditorGUI.Toggle(rect, rule.IsIconRecursive);


            rect.y += LINE_HEIGHT + SPACING * 6f;
            DrawBackgroundPopupMenu(rect, rule);

            if (rule.HasCustomBackground())
            {
                rect.y += LINE_HEIGHT + 2f + SPACING;
                rule.BackgroundTexture = (Texture2D) EditorGUI.ObjectField(rect, rule.BackgroundTexture, typeof(Texture2D), false);
                rect.y += 1f;
            }

            rect.y += LINE_HEIGHT + 2f;
            rule.IsBackgroundRecursive = EditorGUI.Toggle(rect, rule.IsBackgroundRecursive);
        }

        private void DrawPreview(ref Rect rect, ProjectRule rule, bool isProSkin)
        {
            rect.x += rect.width + PADDING;
            rect.y = _windowRect.y + 2f * LINE_HEIGHT + 15f;
            rect.width = rect.height = PREVIEW_SIZE_LARGE;

            // Large Icon
            
            Texture2D texture;
            if (rule.HasLargeIcon())
            {
                texture = (rule.HasCustomIcon()) ? rule.LargeIcon : ProjectIconsStorage.GetIcons(rule.IconType).Item1;
            }
            else
            {
                texture = ProjectEditorUtility.GetDefaultFolderIcon();
            }

            GUI.DrawTexture(rect, texture);
            GUI.DrawTexture(rect, ProjectEditorUtility.GetPreviewGradientPopup());
            
            // Small Icon

            rect.y += PREVIEW_SIZE_LARGE - PREVIEW_SIZE_SMALL - 4f;
            rect.width = rect.height = PREVIEW_SIZE_SMALL;
            
            if (rule.HasSmallIcon())
            {
                texture = (rule.HasCustomIcon()) ? rule.SmallIcon : ProjectIconsStorage.GetIcons(rule.IconType).Item2;
            }
            else
            {
                texture = ProjectEditorUtility.GetDefaultFolderIcon();
            }
            
            GUI.DrawTexture(rect, texture);
            
            // Background

            rect.y += LINE_HEIGHT + 3f * SPACING;
            rect.width = PREVIEW_SIZE_LARGE;

            if (rule.HasBackground())
            {
                texture = (rule.HasCustomBackground()) 
                    ? rule.BackgroundTexture
                    : ProjectBackgroundsStorage.GetBackground(rule.BackgroundType);
                
                GUI.DrawTexture(rect, texture);
            }
            
            rect.x += 13f;
            EditorGUI.LabelField(rect, "Folder");
        }

        private void DrawSeparators(ref Rect rect)
        {
            var color1 = (EditorGUIUtility.isProSkin) ? SEPARATOR_CLR_1_PRO : SEPARATOR_CLR_1_FREE;
            var color2 = (EditorGUIUtility.isProSkin) ? SEPARATOR_CLR_2_PRO : SEPARATOR_CLR_2_FREE;

            // First separator
            rect.x = 0.5f * PADDING;
            rect.y = 2f * LINE_HEIGHT + 16f;
            rect.width = WINDOW_WIDTH - PADDING;
            rect.height = 1f;
            EditorGUI.DrawRect(rect, color1);
            rect.y += 1;
            EditorGUI.DrawRect(rect, color2);

            // Second separator
            rect.y = position.height - LINE_HEIGHT - 11f;
            EditorGUI.DrawRect(rect, color1);
            rect.y += 1;
            EditorGUI.DrawRect(rect, color2);
        }

        private void DrawButtons(ref Rect rect)
        {
            rect.x = PADDING;
            rect.y = position.height - LINE_HEIGHT - 6f;
            rect.width = BUTTON_WIDTH_SMALL;
            rect.height = LINE_HEIGHT;
            ButtonSettings(rect);

            rect.x += BUTTON_WIDTH_SMALL + 0.75f * PADDING;
            ButtonFilter(rect);

            rect.x += BUTTON_WIDTH_SMALL + 0.75f * PADDING;
            ButtonDefault(rect);

            rect.x = WINDOW_WIDTH - 2f * (BUTTON_WIDTH + PADDING);
            rect.width = BUTTON_WIDTH;
            ButtonCancel(rect);

            rect.x = WINDOW_WIDTH - BUTTON_WIDTH - PADDING;
            ButtonApply(rect);
        }

        private void ChangeWindowSize(bool hasCustomIcon, bool hasCustomBackground)
        {
            var rect = position;
            var customIconHeight = (hasCustomIcon) ? LINE_HEIGHT * 2f + 6f : 0f;
            var customBackgroundHeight = (hasCustomBackground) ? LINE_HEIGHT + 4f : 0f;

            var resizeHeight = WINDOW_HEIGHT + customIconHeight + customBackgroundHeight;
            if (resizeHeight == rect.height) return;

            rect.height = resizeHeight;
            position = rect;

            _windowRect.height = rect.height;
            _backgroundRect.height = rect.height - 2f;
        }

        private void DrawIconsPopup(Rect rect, ProjectRule rule)
        {
            var menuName = rule.IconType.ToString();
            if (!GUI.Button(rect, new GUIContent(menuName), "MiniPopup")) return;

            var window = ProjectIconsPopup.GetDraggableWindow();
            window.ShowWithParams(position.position + rect.position, rule, this);
        }

        private void DrawBackgroundPopupMenu(Rect rect, ProjectRule rule)
        {
            var menuName = rule.BackgroundType.ToString();
            if (!GUI.Button(rect, new GUIContent(menuName), "MiniPopup")) return;

            var window = ProjectBackgroundsPopup.GetDraggableWindow();
            window.ShowWithParams(position.position + rect.position, rule, this);
        }

        private void ButtonSettings(Rect rect)
        {
            var icon = ProjectEditorUtility.GetSettingsButtonIcon();
            if (!GUI.Button(rect, new GUIContent(icon, "All Rules"), GUIStyle.none)) return;

            ProjectRuleset.ShowInspector();
            Close();
        }

        private void ButtonFilter(Rect rect)
        {
            var icon = ProjectEditorUtility.GetFilterButtonIcon();
            if (!GUI.Button(rect, new GUIContent(icon, "Folder Rules"), GUIStyle.none)) return;

            var folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_paths[0]);
            ProjectRuleset.ShowInspector(folderAsset);
            Close();
        }

        private void ButtonDefault(Rect rect)
        {
            var icon = ProjectEditorUtility.GetDeleteButtonIcon();
            if (!GUI.Button(rect, new GUIContent(icon, "Revert to Default"), GUIStyle.none)) return;

            _currentRule.Priority = 0;

            _currentRule.IconType = ProjectIcon.None;
            _currentRule.SmallIcon = null;
            _currentRule.LargeIcon = null;
            _currentRule.IsIconRecursive = false;

            _currentRule.BackgroundType = ProjectBackground.None;
            _currentRule.BackgroundTexture = null;
            _currentRule.IsBackgroundRecursive = false;
        }

        private void ButtonCancel(Rect rect)
        {
            if (!GUI.Button(rect, "Cancel")) return;
            Close();
        }

        private void ButtonApply(Rect rect)
        {
            if (!GUI.Button(rect, "Apply")) return;

            for (var i = 0; i < _existingItems.Length; i++)
            {
                _currentRule.Key = (_currentRule.Type == KeyType.Path)
                    ? _paths[i]
                    : Path.GetFileName(_paths[i]);

                _ruleset.UpdateRule(_existingItems[i], _currentRule);
            }
            Close();
        }
    }
}
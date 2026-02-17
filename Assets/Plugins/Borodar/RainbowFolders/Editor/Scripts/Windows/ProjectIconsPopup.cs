using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    public class ProjectIconsPopup : ProjectSelectionPopup<ProjectIcon>
    {
        private ProjectIconCategory _category;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static ProjectIconsPopup GetDraggableWindow()
        {
            return GetDraggableWindow<ProjectIconsPopup>();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void DrawButtons(Rect rect)
        {
            DrawCategoryButton(rect, "All", ProjectIconCategory.All);
            rect.y += LINE_HEIGHT + SPACING;
            DrawCategoryButton(rect, "Colors", ProjectIconCategory.Colors);
            rect.y += LINE_HEIGHT + SPACING;
            DrawCategoryButton(rect, "Tags", ProjectIconCategory.Tags);
            rect.y += LINE_HEIGHT + SPACING;
            DrawCategoryButton(rect, "Transparent", ProjectIconCategory.Transparent);
            rect.y += LINE_HEIGHT + SPACING;
            DrawCategoryButton(rect, "Types", ProjectIconCategory.Types);
            rect.y += LINE_HEIGHT + SPACING;
            DrawCategoryButton(rect, "Types BG", ProjectIconCategory.TypesBg);
            rect.y += LINE_HEIGHT + SPACING;
            DrawCategoryButton(rect, "Platforms", ProjectIconCategory.Platforms);
            rect.y += LINE_HEIGHT + SPACING;
            DrawCategoryButton(rect, "Platforms BG", ProjectIconCategory.PlatformsBg);

            rect.y += LINE_HEIGHT * 2f - SPACING;
            DrawCustomButton(rect);
            rect.y += LINE_HEIGHT + SPACING;
            DrawNoneButton(rect);
        }

        protected override void DrawIcons(Rect rect)
        {
            GUILayout.BeginArea(rect);
            ScrollPos = BeginScrollView(ScrollPos);

            var predicate = GetCategoryPredicate(_category);
            var icons = Enum.GetValues(typeof(ProjectIcon))
                .Cast<ProjectIcon>()
                .Where(predicate)
                .ToList();

            GUILayout.BeginVertical();
            DrawIconsGrid(icons);
            GUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        protected override void DrawIconButton(ProjectIcon iconType)
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(66), GUILayout.Height(66));
            if (GUI.Button(rect, GUIContent.none, "grey_border"))
            {
                AssignIconByType(ProjectRule, iconType);
            }

            var iconTex = ProjectIconsStorage.GetIcons(iconType);
            DrawPreview(rect, iconTex, EditorGUIUtility.isProSkin);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DrawCategoryButton(Rect rect, string text, ProjectIconCategory category)
        {
            if (!GUI.Button(rect, text, "MiniToolbarButtonLeft")) return;
            ApplyIconsCategory(category);
        }

        private void DrawNoneButton(Rect rect)
        {
            if (!GUI.Button(rect, "None", "minibutton")) return;
            ResetIconToDefault(ProjectRule);
        }

        private void DrawCustomButton(Rect rect)
        {
            if (!GUI.Button(rect, "Custom", "minibutton")) return;
            AssignIconByType(ProjectRule, ProjectIcon.Custom);
        }

        private static Func<ProjectIcon, bool> GetCategoryPredicate(ProjectIconCategory category)
        {
            switch (category)
            {
                case ProjectIconCategory.All:
                    return icon => icon != ProjectIcon.None && icon != ProjectIcon.Custom;
                case ProjectIconCategory.Colors:
                    return icon => (int) icon >= 10 && (int) icon < 500;
                case ProjectIconCategory.Transparent:
                    return icon => (int) icon >= 500 && (int) icon < 1000;
                case ProjectIconCategory.Tags:
                    return icon => (int) icon >= 1000 && (int) icon < 2000;
                case ProjectIconCategory.Types:
                    return icon => (int) icon >= 2000 && (int) icon < 2500;
                case ProjectIconCategory.TypesBg:
                    return icon => (int) icon >= 2500 && (int) icon < 3000;
                case ProjectIconCategory.Platforms:
                    return icon => (int) icon >= 3000 && (int) icon < 3500;
                case ProjectIconCategory.PlatformsBg:
                    return icon => (int) icon >= 3500 && (int) icon < 4000;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DrawPreview(Rect rect, Tuple<Texture2D, Texture2D> icon, bool isProSkin)
        {
            var (iconLarge, iconSmall) = icon;

            // Large Icon
            rect.x += 3f;
            rect.width = rect.height = PREVIEW_SIZE_LARGE;

            GUI.DrawTexture(rect, iconLarge);
            GUI.DrawTexture(rect, ProjectEditorUtility.GetPreviewGradientPopup());

            // Small Icon
            rect.y += PREVIEW_SIZE_LARGE - PREVIEW_SIZE_SMALL - 4f;
            rect.width = rect.height = PREVIEW_SIZE_SMALL;

            GUI.DrawTexture(rect, iconSmall);
        }

        private void ApplyIconsCategory(ProjectIconCategory category)
        {
            _category = category;
            ScrollPos = Vector2.zero;
        }

        private void AssignIconByType(dynamic rule, ProjectIcon type)
        {
            if (IsRuleSerialized)
            {
                rule.FindPropertyRelative("IconType").intValue = (int) type;
                rule.FindPropertyRelative("SmallIcon").objectReferenceValue = null;
                rule.FindPropertyRelative("LargeIcon").objectReferenceValue = null;
                ApplyPropertyChangesAndClose(rule);
            }
            else
            {
                rule.IconType = type;
                rule.SmallIcon = null;
                rule.LargeIcon = null;
                CloseAndRepaintParent();
            }
        }

        private void ResetIconToDefault(dynamic rule)
        {
            if (IsRuleSerialized)
            {
                rule.FindPropertyRelative("IconType").intValue = (int) ProjectIcon.None;
                rule.FindPropertyRelative("SmallIcon").objectReferenceValue = null;
                rule.FindPropertyRelative("LargeIcon").objectReferenceValue = null;
                rule.FindPropertyRelative("IsIconRecursive").boolValue = false;
                ApplyPropertyChangesAndClose(rule);
            }
            else
            {
                rule.IconType = ProjectIcon.None;
                rule.SmallIcon = null;
                rule.LargeIcon = null;
                rule.IsIconRecursive = false;
                CloseAndRepaintParent();
            }
        }
    }
}
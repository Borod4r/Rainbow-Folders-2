using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    public class ProjectBackgroundsPopup : ProjectSelectionPopup<ProjectBackground>
    {
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static ProjectBackgroundsPopup GetDraggableWindow()
        {
            return GetDraggableWindow<ProjectBackgroundsPopup>();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void DrawButtons(Rect rect)
        {
            DrawCategoryButton(rect, "All");

            rect.y += LINE_HEIGHT * 9f + SPACING * 6f;
            DrawCustomButton(rect);
            rect.y += LINE_HEIGHT + SPACING;
            DrawNoneButton(rect);
        }

        protected override void DrawIcons(Rect rect)
        {
            GUILayout.BeginArea(rect);
            ScrollPos = BeginScrollView(ScrollPos);

            var predicate = GetCategoryPredicate();
            var icons = Enum.GetValues(typeof(ProjectBackground))
                .Cast<ProjectBackground>()
                .Where(predicate)
                .ToList();

            GUILayout.BeginVertical();
            DrawIconsGrid(icons);
            GUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        protected override void DrawIconButton(ProjectBackground backgroundType)
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(66), GUILayout.Height(22));
            if (GUI.Button(rect, GUIContent.none, "grey_border"))
            {
                AssignBackgroundByType(ProjectRule, backgroundType);
            }

            var backgroundTex = ProjectBackgroundsStorage.GetBackground(backgroundType);
            DrawPreview(rect, backgroundTex);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DrawCategoryButton(Rect rect, string text)
        {
            if (!GUI.Button(rect, text, "MiniToolbarButtonLeft")) return;
            ApplyIconsCategory();
        }

        private void DrawCustomButton(Rect rect)
        {
            if (!GUI.Button(rect, "Custom", "minibutton")) return;
            AssignBackgroundByType(ProjectRule, ProjectBackground.Custom);
        }

        private void DrawNoneButton(Rect rect)
        {
            if (!GUI.Button(rect, "None", "minibutton")) return;
            ResetBackgroundToDefault(ProjectRule);
        }

        private static Func<ProjectBackground, bool> GetCategoryPredicate()
        {
            return icon => (int) icon >= 10 && (int) icon < 500;
        }

        private static void DrawPreview(Rect rect, Texture icon)
        {
            rect.x += 1f;
            rect.y += 1f;
            rect.width = PREVIEW_SIZE_LARGE;
            rect.height = PREVIEW_SIZE_SMALL + 4f;

            GUI.Label(rect, "Folder", "CenteredLabel");
            GUI.DrawTexture(rect, icon);
        }

        private void ApplyIconsCategory()
        {
            ScrollPos = Vector2.zero;
        }

        private void AssignBackgroundByType(dynamic rule, ProjectBackground type)
        {
            if (IsRuleSerialized)
            {
                rule.FindPropertyRelative("BackgroundType").intValue = (int) type;
                rule.FindPropertyRelative("BackgroundTexture").objectReferenceValue = null;
                ApplyPropertyChangesAndClose(rule);
            }
            else
            {
                rule.BackgroundType = type;
                rule.BackgroundTexture = null;
                CloseAndRepaintParent();
            }
        }

        private void ResetBackgroundToDefault(dynamic rule)
        {
            if (IsRuleSerialized)
            {
                rule.FindPropertyRelative("BackgroundType").intValue = (int) ProjectBackground.None;
                rule.FindPropertyRelative("BackgroundTexture").objectReferenceValue = null;
                rule.FindPropertyRelative("IsBackgroundRecursive").boolValue = false;
                ApplyPropertyChangesAndClose(rule);
            }
            else
            {
                rule.BackgroundType = ProjectBackground.None;
                rule.BackgroundTexture = null;
                rule.IsBackgroundRecursive = false;
                CloseAndRepaintParent();
            }
        }
    }
}
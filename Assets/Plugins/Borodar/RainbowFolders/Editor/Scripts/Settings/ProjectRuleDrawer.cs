using System;
using UnityEditor;
using UnityEngine;
using KeyType = Borodar.RainbowFolders.ProjectRule.KeyType;

namespace Borodar.RainbowFolders
{
    [CustomPropertyDrawer(typeof(ProjectRule))]
    public class ProjectRuleDrawer : PropertyDrawer
    {
        private const float PADDING = 8f;
        private const float SPACING = 1f;
        private const float LINE_HEIGHT = 18f;
        private const float LABELS_WIDTH = 100f;
        private const float PREVIEW_SIZE_SMALL = 16f;
        private const float PREVIEW_SIZE_LARGE = 64f;        
        private const float PROPERTY_HEIGHT = 136;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var isHidden = property.FindPropertyRelative("IsHidden").boolValue;
            if (isHidden) return;

            var originalPosition = position;
            var serializedItem = new SerializedItemWrapper(property);

            EditorGUI.BeginChangeCheck();

            DrawLabels(ref position, serializedItem);            
            DrawValues(ref position, originalPosition, serializedItem);
            DrawPreview(ref position, originalPosition, serializedItem);

            if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var isHidden = property.FindPropertyRelative("IsHidden").boolValue;
            if (isHidden) return 0;

            var iconType = property.FindPropertyRelative("IconType");
            var backgroundType = property.FindPropertyRelative("BackgroundType");
            var hasCustomIcon = (iconType.intValue == (int) ProjectIcon.Custom);
            var hasCustomBackground = (backgroundType.intValue == (int) ProjectBackground.Custom);

            var height = PROPERTY_HEIGHT;
            if (hasCustomIcon) height += (LINE_HEIGHT + SPACING) * 2f;
            if (hasCustomBackground) height += LINE_HEIGHT + SPACING;

            return height;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void DrawLabels(ref Rect position, SerializedItemWrapper item)
        {
            position.y += PADDING;
            position.width = LABELS_WIDTH - PADDING;
            position.height = LINE_HEIGHT;

            var typeSelected = (KeyType) Enum.GetValues(typeof(KeyType)).GetValue(item.FolderKeyType.enumValueIndex);
            item.FolderKeyType.enumValueIndex = (int)(KeyType) EditorGUI.EnumPopup(position, typeSelected);

            position.y += LINE_HEIGHT + SPACING * 4f;
            EditorGUI.LabelField(position, "Priority");
            
            position.y += LINE_HEIGHT + SPACING * 4f;
            EditorGUI.LabelField(position, "Icon");

            if (item.HasCustomIcon)
            {
                position.y += LINE_HEIGHT + SPACING;
                EditorGUI.LabelField(position, "x16");

                position.y += LINE_HEIGHT + SPACING;
                EditorGUI.LabelField(position, "x64");
            }

            position.y += LINE_HEIGHT + SPACING;
            EditorGUI.LabelField(position, "Recursive");

            position.y += LINE_HEIGHT + SPACING * 4f;
            EditorGUI.LabelField(position, "Background");
            
            if (item.HasCustomBackground)
            {
                position.y += LINE_HEIGHT + SPACING;
                EditorGUI.LabelField(position, "x16");
            }

            position.y += LINE_HEIGHT + SPACING;
            EditorGUI.LabelField(position, "Recursive");
        }

        private static void DrawValues(ref Rect position, Rect originalPosition, SerializedItemWrapper item)
        {
            position.x += LABELS_WIDTH;
            position.y = originalPosition.y + PADDING;
            position.width = originalPosition.width - LABELS_WIDTH;
            EditorGUI.PropertyField(position, item.FolderKey, GUIContent.none);

            position.y += LINE_HEIGHT + SPACING * 4f;
            EditorGUI.PropertyField(position, item.Priority, GUIContent.none);

            position.width = originalPosition.width - LABELS_WIDTH - PREVIEW_SIZE_LARGE - PADDING;
            
            position.y += LINE_HEIGHT + SPACING * 4f;
            DrawIconPopupMenu(position, item.Property, item.HasCustomIcon, item.IconType.intValue);

            if (item.HasCustomIcon)
            {
                position.y += LINE_HEIGHT + SPACING;
                EditorGUI.PropertyField(position, item.SmallIcon, GUIContent.none);

                position.y += LINE_HEIGHT + SPACING;
                EditorGUI.PropertyField(position, item.LargeIcon, GUIContent.none);
            }

            position.y += LINE_HEIGHT + SPACING;
            EditorGUI.PropertyField(position, item.IconRecursive, GUIContent.none);
            
            position.y += LINE_HEIGHT + SPACING * 4f;
            DrawBackgroundPopupMenu(position, item.Property, item.HasCustomBackground, item.BackgroundType.intValue);

            if (item.HasCustomBackground)
            {
                position.y += LINE_HEIGHT + SPACING;
                EditorGUI.PropertyField(position, item.Background, GUIContent.none);
            }

            position.y += LINE_HEIGHT + SPACING;
            EditorGUI.PropertyField(position, item.BackgroundRecursive, GUIContent.none);
        }

        private static void DrawPreview(ref Rect position, Rect originalPosition, SerializedItemWrapper item)
        {
            Texture2D smallTexture = null;
            Texture2D largeTexture = null;
            
            if (item.HasIcon)
            {
                if (item.HasCustomIcon)
                {
                    smallTexture = (Texture2D) item.SmallIcon.objectReferenceValue;
                    largeTexture = (Texture2D) item.LargeIcon.objectReferenceValue;
                }
                else
                {
                    var tuple = ProjectIconsStorage.GetIcons(item.IconType.intValue);
                    if (tuple != null)
                    {
                        largeTexture = tuple.Item1;
                        smallTexture = tuple.Item2;
                    }
                }
            }
            
            if (smallTexture == null)
                smallTexture = ProjectEditorUtility.GetDefaultFolderIcon();

            if (largeTexture == null)
                largeTexture = ProjectEditorUtility.GetDefaultFolderIcon();

            // Draw large texture
            position.x += position.width + PADDING;
            position.y = originalPosition.y + LINE_HEIGHT * 2f + SPACING + 8f;
            position.width = position.height = PREVIEW_SIZE_LARGE;
            GUI.DrawTexture(position, largeTexture);
            GUI.DrawTexture(position, ProjectEditorUtility.GetPreviewGradientDrawer());

            // Draw small texture
            position.y += PREVIEW_SIZE_LARGE - PREVIEW_SIZE_SMALL - 4f;
            position.width = position.height = PREVIEW_SIZE_SMALL;
            GUI.DrawTexture(position, smallTexture);
            
            // Draw background
            position.y += LINE_HEIGHT + SPACING * 4f;
            position.width = PREVIEW_SIZE_LARGE;
            
            if (item.HasBackground)
            {
                var backgroundRef = (item.HasCustomBackground)
                    ? (Texture2D) item.Background.objectReferenceValue
                    : ProjectBackgroundsStorage.GetBackground(item.BackgroundType.intValue);

                if (backgroundRef != null) GUI.DrawTexture(position, backgroundRef);
            }
            
            position.x += 13f;
            EditorGUI.LabelField(position, "Folder");
        }
        
        private static void DrawIconPopupMenu(Rect rect, SerializedProperty property, bool hasCustomIcon, int iconType)
        {
            var menuName = (hasCustomIcon) ? "Custom" : ((ProjectIcon) iconType).ToString();
            if (GUI.Button(rect, new GUIContent(menuName), "MiniPopup"))
            {
                var screenPoint = GUIUtility.GUIToScreenPoint(rect.position);
                var window = ProjectIconsPopup.GetDraggableWindow();
                window.ShowWithParams(screenPoint, property);
            }
        }
        
        private static void DrawBackgroundPopupMenu(Rect rect, SerializedProperty property, bool hasCustomBackground, int backgroundType)
        {
            var menuName = (hasCustomBackground) ? "Custom" : ((ProjectBackground) backgroundType).ToString();
            if (GUI.Button(rect, new GUIContent(menuName), "MiniPopup"))
            {
                var screenPoint = GUIUtility.GUIToScreenPoint(rect.position);
                var window = ProjectBackgroundsPopup.GetDraggableWindow();
                window.ShowWithParams(screenPoint, property);
            }
        }
        
        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        private class SerializedItemWrapper
        {
            public readonly SerializedProperty Property;
            
            public readonly SerializedProperty FolderKey;
            public readonly SerializedProperty FolderKeyType;

            public readonly SerializedProperty Priority;

            public readonly SerializedProperty IconType;
            public readonly SerializedProperty SmallIcon;
            public readonly SerializedProperty LargeIcon;
            public readonly SerializedProperty IconRecursive;
            
            public readonly SerializedProperty BackgroundType;
            public readonly SerializedProperty Background;
            public readonly SerializedProperty BackgroundRecursive;

            public readonly bool HasIcon;
            public readonly bool HasCustomIcon;
            public readonly bool HasBackground;
            public readonly bool HasCustomBackground;

            public SerializedItemWrapper(SerializedProperty property)
            {
                Property = property;
                
                FolderKey = property.FindPropertyRelative("Key");
                FolderKeyType = property.FindPropertyRelative("Type");

                Priority = property.FindPropertyRelative("Priority");

                IconType = property.FindPropertyRelative("IconType");
                SmallIcon = property.FindPropertyRelative("SmallIcon");
                LargeIcon = property.FindPropertyRelative("LargeIcon");
                IconRecursive = property.FindPropertyRelative("IsIconRecursive");
            
                BackgroundType = property.FindPropertyRelative("BackgroundType");
                Background = property.FindPropertyRelative("BackgroundTexture");
                BackgroundRecursive = property.FindPropertyRelative("IsBackgroundRecursive");

                HasIcon = (IconType.intValue != (int) ProjectIcon.None);
                HasCustomIcon = (IconType.intValue == (int) ProjectIcon.Custom);
                HasBackground = (BackgroundType.intValue != (int) ProjectBackground.None);
                HasCustomBackground = (BackgroundType.intValue == (int) ProjectBackground.Custom);
            }
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    [SuppressMessage("ReSharper", "ConvertIfStatementToNullCoalescingExpression")]
    public static class ProjectEditorUtility
    {
        private static Texture2D _defaultFolderIcon;

        private static Texture2D _editIconSmall;
        private static Texture2D _editIconLarge;

        private static Texture2D _previewGradientPopup;
        private static Texture2D _previewGradientDrawer;

        private static Texture2D _settingsIcon;
        private static Texture2D _filterIcon;
        private static Texture2D _deleteIcon;

        private static Texture2D _foldoutIcon;
        private static Texture2D _foldoutLevelsIcon;

        private static string _projectName;

        //---------------------------------------------------------------------
        // Project
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "InvertIf")]
        public static string ProjectName
        {
            get
            {
                if (_projectName == null)
                {
                    var path = Application.dataPath.Split('/');
                    _projectName = path[path.Length - 2];
                }

                return _projectName;
            }
        }

        //---------------------------------------------------------------------
        // Assets
        //---------------------------------------------------------------------

        /// <summary>
        /// Creates .asset file of the specified <see cref="UnityEngine.ScriptableObject"/>
        /// </summary>
        public static string CreateAsset<T>(string baseName, string forcedPath = "") where T : ScriptableObject
        {
            if (baseName.Contains("/"))
                throw new ArgumentException("Base name should not contain slashes");

            var asset = ScriptableObject.CreateInstance<T>();

            string path;
            if (!string.IsNullOrEmpty(forcedPath))
            {
                path = forcedPath;
                Directory.CreateDirectory(forcedPath);
            }
            else
            {
                path = AssetDatabase.GetAssetPath(Selection.activeObject);

                if (string.IsNullOrEmpty(path))
                {
                    path = "Assets";
                }
                else if (Path.GetExtension(path) != string.Empty)
                {
                    path = path.Replace(Path.GetFileName(path), string.Empty);
                }
            }

            var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + baseName + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            return assetPathAndName;
        }

        public static string[] FindPathsForAllRulesets()
        {
            var filter = $"t:{nameof(ProjectRuleset)}";
            var rulesetGUIDs = AssetDatabase.FindAssets(filter);
            return rulesetGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        }

        //---------------------------------------------------------------------
        // GUI
        //---------------------------------------------------------------------

        public static bool SearchField(ref string query, ref Enum filter, Enum defaultFilter, params GUILayoutOption[] options)
        {
            var queryBefore = query;
            var filterBefore = filter;
            var changed = false;

            GUILayout.BeginHorizontal();

            var queryRect = GUILayoutUtility.GetRect(GUIContent.none, "ToolbarSearchTextFieldPopup", options);
            queryRect.x += 2f;
            queryRect.width -= 13f;

            var filterRect = queryRect;
            filterRect.width = 20f;

            filter = EditorGUI.EnumPopup(filterRect, filter, "label");
            if (!Equals(filter, filterBefore)) changed = true;

            query = EditorGUI.TextField(queryRect, GUIContent.none, query, "ToolbarSearchTextFieldPopup");
            if (query != null && !query.Equals(queryBefore)) changed = true;

            var cancelRect = queryRect;
            cancelRect.x += queryRect.width;
            cancelRect.width = 12f;
            if (GUI.Button(cancelRect, GUIContent.none, "ToolbarSearchCancelButton"))
            {
                query = string.Empty;
                filter = defaultFilter;
                changed = true;
                // workaround for bug with selected text
                GUIUtility.keyboardControl = 0;
            }

            GUILayout.EndHorizontal();

            return changed;
        }

        //---------------------------------------------------------------------
        // Textures
        //---------------------------------------------------------------------

        public static Texture2D GetDefaultFolderIcon()
        {
            if (_defaultFolderIcon == null)
                _defaultFolderIcon = EditorGUIUtility.FindTexture("Folder Icon");

            return _defaultFolderIcon;
        }

        public static Texture2D GetEditFolderIcon(bool isSmall)
        {
            return (isSmall) ? GetEditIconSmall() : GetEditIconLarge();
        }

        public static Texture2D GetSettingsButtonIcon()
        {
            return GetTexture(ref _settingsIcon, ProjectEditorTexture.IcnSettings);
        }

        public static Texture2D GetFilterButtonIcon()
        {
            return GetTexture(ref _filterIcon, ProjectEditorTexture.IcnFilter);
        }

        public static Texture2D GetDeleteButtonIcon()
        {
            return GetTexture(ref _deleteIcon, ProjectEditorTexture.IcnDelete);
        }

        public static Texture2D GetFoldoutIcon()
        {
            return GetTexture(ref _foldoutIcon, ProjectEditorTexture.IcnFoldoutMiddle);
        }
        
        public static Texture2D GetFoldoutLevelsIcon()
        {
            return GetTexture(ref _foldoutLevelsIcon, ProjectEditorTexture.IcnFoldoutLevels);
        }

        public static Texture2D GetPreviewGradientPopup()
        {
            return GetTexture(ref _previewGradientPopup, ProjectEditorTexture.IcnPreviewGradientPopup);
        }

        public static Texture2D GetPreviewGradientDrawer()
        {
            return GetTexture(ref _previewGradientDrawer, ProjectEditorTexture.IcnPreviewGradientDrawer);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static Texture2D GetEditIconSmall()
        {
            return GetTexture(ref _editIconSmall, ProjectEditorTexture.IcnEditSmall);
        }

        private static Texture2D GetEditIconLarge()
        {
            return GetTexture(ref _editIconLarge, ProjectEditorTexture.IcnEditLarge);
        }

        private static Texture2D GetTexture(ref Texture2D texture, ProjectEditorTexture type)
        {
            if (texture == null)
                texture = ProjectEditorTexturesStorage.GetTexture(type);

            return texture;
        }
    }
}
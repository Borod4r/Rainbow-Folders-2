using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Borodar.RainbowFolders.RList;
using UnityEditor;
using UnityEngine;
using static Borodar.RainbowFolders.ProjectRule.KeyType;

namespace Borodar.RainbowFolders
{
    [CustomEditor(typeof (ProjectRuleset))]
    public class ProjectRulesetEditor : Editor
    {
        public static readonly HashSet<ProjectRulesetEditor> EDITORS = new HashSet<ProjectRulesetEditor>();

        private const string SEARCH_RESULTS_TITLE = "Search Results";
        private const string PROP_NAME_FOLDERS = "Rules";
        private const string NEGATIVE_LOOKAHEAD = "(?!.*)"; // Regex that matches nothing

        private SerializedProperty _foldersProperty;
        private ReorderableList _reorderableList;

        private string _query = string.Empty;
        private Enum _filter = Filter.All;
        private bool _matchCase;
        private bool _useRegex;

        private string _warningMessage;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public DefaultAsset Asset { get; set; }

        public int SearchTab { get; set; }

        public bool ForceUpdate { get; set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void OnEnable()
        {
            EDITORS.Add(this);

            _foldersProperty = serializedObject.FindProperty(PROP_NAME_FOLDERS);
            _reorderableList = new ReorderableList(_foldersProperty)
            {
                label = new GUIContent(string.Empty),
                elementDisplayType = ReorderableList.ElementDisplayType.SingleLine,
                expandable = false,
                headerHeight = 0f,
                paginate = true,
                pageSize = 10,
            };

            _reorderableList.onChangedCallback += (list) => OnRulesetChange();

            // ReSharper disable once DelegateSubtraction
            Undo.undoRedoPerformed -= OnRulesetChange;
            Undo.undoRedoPerformed += OnRulesetChange;
        }

        protected void OnDisable()
        {
            EDITORS.Remove(this);
            ClearHiddenFlags();

            // ReSharper disable once DelegateSubtraction
            Undo.undoRedoPerformed -= OnRulesetChange;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);

            var searchTabBefore = SearchTab;
            SearchTab = GUILayout.Toolbar(SearchTab, new [] {"Filter by folder", "Filter by key"});
            ForceUpdate |= SearchTab != searchTabBefore;

            EditorGUILayout.BeginVertical("AvatarMappingBox");
            {
                GUILayout.Space(6f);

                switch (SearchTab)
                {
                    case 0: // Folder
                    {
                        DrawSearchByFolderPanel(ForceUpdate);
                        break;
                    }
                    case 1: // Key
                    {
                        DrawSearchByKeyPanel(ForceUpdate);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(SearchTab), SearchTab, null);
                }

                if (!string.IsNullOrEmpty(_warningMessage))
                {
                    EditorGUILayout.HelpBox(_warningMessage, MessageType.Warning);
                }

                GUILayout.Space(4f);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(2f);

            serializedObject.Update();
            DrawReorderableList();

            ForceUpdate = false;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void OnRulesetChange()
        {
            ProjectRuleset.OnRulesetChange();
        }

        private void DrawSearchByFolderPanel(bool forceUpdate)
        {
            var assetBefore = Asset;
            Asset = (DefaultAsset) EditorGUILayout.ObjectField(Asset, typeof(DefaultAsset), false);

            if (!forceUpdate && Asset == assetBefore) return;

            if (Asset == null)
            {
                ClearHiddenFlags();
            }
            else
            {
                ApplyHiddenFlagsByAsset();
            }
        }

        private void DrawSearchByKeyPanel(bool forceUpdate)
        {
            EditorGUILayout.BeginHorizontal();
            var queryChanged = ProjectEditorUtility.SearchField(ref _query, ref _filter, Filter.All);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (!Equals(_filter, Filter.All))
            {
                var rect = GUILayoutUtility.GetRect(GUIContent.none, "MiniLabel");
                rect.x += 2f;
                rect.y += 1f;
                rect.width = 55f;
                GUI.Label(rect, $"➔ {_filter}", "MiniLabel");
            }

            GUILayout.FlexibleSpace();

            var matchCaseBefore = _matchCase;
            _matchCase = EditorGUILayout.ToggleLeft("Match case", _matchCase, "MiniLabel", GUILayout.Width(83f));
            var matchCaseChanged = _matchCase != matchCaseBefore;

            var useRegexBefore = _useRegex;
            _useRegex = EditorGUILayout.ToggleLeft("Regex", _useRegex, "MiniLabel", GUILayout.Width(58f));
            var useRegexChanged = _useRegex != useRegexBefore;

            EditorGUILayout.EndHorizontal();

            if (!forceUpdate && !queryChanged && !matchCaseChanged && !useRegexChanged) return;

            _warningMessage = string.Empty;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var isDefaultFilter = Equals(Filter.All, _filter);
            if (string.IsNullOrEmpty(_query) && isDefaultFilter)
            {
                ClearHiddenFlags();
            }
            else
            {
                ApplyHiddenFlagsByKey();
            }
        }

        private void ClearHiddenFlags()
        {
            if (_foldersProperty == null) return;

            for (var i = 0; i < _foldersProperty.arraySize; i++)
            {
                var item = _foldersProperty.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("IsHidden").boolValue = false;
            }

            _foldersProperty.serializedObject.ApplyModifiedProperties();

            _reorderableList.canAdd = true;
            _reorderableList.headerHeight = 0f;
            _reorderableList.label.text = string.Empty;
            _reorderableList.paginate = true;
        }

        private void ApplyHiddenFlagsByAsset()
        {
            var folderPath = AssetDatabase.GetAssetPath(Asset);
            var folderName = System.IO.Path.GetFileName(folderPath);

            foreach (var rule in ((ProjectRuleset) target).Rules)
            {
                bool match;
                switch (rule.Type)
                {
                    case Name:
                        match = rule.Key.Equals(folderName) ||
                                 (rule.IsRecursive() && folderPath.Contains($"/{rule.Key}/"));
                        break;

                    case Path:
                        match = rule.Key.Equals(folderPath) ||
                                (rule.IsRecursive() && folderPath.StartsWith(rule.Key + "/"));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                rule.IsHidden = !match;
            }

            _reorderableList.canAdd = false;
            _reorderableList.headerHeight = 18f;
            _reorderableList.label.text = SEARCH_RESULTS_TITLE;
            _reorderableList.paginate = false;
        }

        private void ApplyHiddenFlagsByKey()
        {
            var regex = (_useRegex) ? MakeRegexFromQuery() : null;

            for (var i = 0; i < _foldersProperty.arraySize; i++)
            {
                var item = _foldersProperty.GetArrayElementAtIndex(i);
                var isHidden = item.FindPropertyRelative("IsHidden");

                switch (_filter)
                {
                    case Filter.All:
                        isHidden.boolValue = !KeyContainsQuery(item, regex);
                        break;
                    case Filter.Name:
                        isHidden.boolValue = !KeyHasSameType(item, Name) || !KeyContainsQuery(item, regex);
                        break;
                    case Filter.Path:
                        isHidden.boolValue = !KeyHasSameType(item, Path) || !KeyContainsQuery(item, regex);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_filter), _filter, null);
                }
            }

            _foldersProperty.serializedObject.ApplyModifiedProperties();

            _reorderableList.canAdd = false;
            _reorderableList.headerHeight = 18f;
            _reorderableList.label = new GUIContent(SEARCH_RESULTS_TITLE);
            _reorderableList.paginate = false;
        }

        private Regex MakeRegexFromQuery()
        {
            var options = _matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;

            try
            {
                return new Regex(_query, options);
            }
            catch (ArgumentException ex)
            {
                _warningMessage = ex.Message;
                return new Regex(NEGATIVE_LOOKAHEAD);
            }
        }

        private static bool KeyHasSameType(SerializedProperty item, ProjectRule.KeyType keyType)
        {
            var propType = item.FindPropertyRelative("Type").enumValueIndex;
            return propType == (int) keyType;
        }

        private bool KeyContainsQuery(SerializedProperty item, Regex regex)
        {
            var key = item.FindPropertyRelative("Key").stringValue;

            // Regex search
            if (_useRegex)
            {
                var match = regex.Match(key);
                return match.Success;
            }

            // Simple search
            var comparison = _matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return key.IndexOf(_query, comparison) >= 0;
        }

        private void DrawReorderableList()
        {
            EditorGUI.BeginChangeCheck();

            _reorderableList.DoLayoutList();

            // Track changes in reorderable list
            if (EditorGUI.EndChangeCheck())
            {
                ProjectRuleset.OnRulesetChange();
                serializedObject.ApplyModifiedProperties();
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        private enum Filter
        {
            All, Name, Path
        }
    }
}
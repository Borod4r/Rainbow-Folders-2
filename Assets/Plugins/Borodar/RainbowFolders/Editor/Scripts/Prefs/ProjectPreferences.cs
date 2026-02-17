using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Borodar.RainbowFolders
{
    public static class ProjectPreferences
    {
        private const float PREF_LABEL_WIDTH = 150f;

        private const string RULESET_PKEY = "Borodar.RainbowFolders.Ruleset.";
        private const string RULESET_DEFAULT = "Assets/Plugins/RainbowAssets/RainbowFolders/Editor/Data/RainbowFoldersRuleset.asset";
        private const string RULESET_HINT = "The ruleset that is currently used. You could have multiple rulesets in your project and switch between them using this option.";

        private const string EDIT_MODIFIER_PKEY = "Borodar.RainbowFolders.EditMod.";
        private const string EDIT_MODIFIER_HINT = "Modifier key that is used to show configuration dialogue when clicking on a folder icon.";
        private const EventModifiers EDIT_MODIFIER_DEFAULT = EventModifiers.Alt;

        private const string DRAG_MODIFIER_PKEY = "Borodar.RainbowFolders.DragMod.";
        private const string DRAG_MODIFIER_HINT = "Modifier key that is used to drag configuration dialogue.";
        private const EventModifiers DRAG_MODIFIER_DEFAULT = EventModifiers.Shift;
        
        private const string PROJECT_TREE_PKEY = "Borodar.RainbowFolders.ShowProjectTree.";
        private const string PROJECT_TREE_HINT = "Change this setting to show/hide the \"branches\" in the project window.";
        private const bool PROJECT_TREE_DEFAULT = true;
        
        private const string ROW_SHADING_PKEY = "Borodar.RainbowFolders.RowShading.";
        private const string ROW_SHADING_HINT = "Change this settings to enable/disable row shading in the project window.";
        private const bool ROW_SHADING_DEFAULT = true;

        private static readonly EditorPrefsStringPopup RULESET_PREF;
        private static readonly EditorPrefsModifierKey EDIT_MODIFIER_PREF;
        private static readonly EditorPrefsModifierKey DRAG_MODIFIER_PREF;
        private static readonly EditorPrefsBoolean PROJECT_TREE_PREF;
        private static readonly EditorPrefsBoolean ROW_SHADING_PREF;

        private static EventModifiers _editModifier;
        private static EventModifiers _dragModifier;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public static bool ShowProjectTree { get; private set; }
        public static bool DrawRowShading { get; private set; }
        public static string RulesetPath { get; private set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        static ProjectPreferences()
        {
            var projectName = ProjectEditorUtility.ProjectName;

            var rulesetLabel = new GUIContent("Current Ruleset", RULESET_HINT);
            RULESET_PREF = new EditorPrefsStringPopup(RULESET_PKEY + projectName, RULESET_DEFAULT, rulesetLabel, PREF_LABEL_WIDTH);
            RulesetPath = RULESET_PREF.Value;

            var editModifierLabel = new GUIContent("Edit Modifier", EDIT_MODIFIER_HINT);
            EDIT_MODIFIER_PREF = new EditorPrefsModifierKey(EDIT_MODIFIER_PKEY + projectName, EDIT_MODIFIER_DEFAULT, editModifierLabel);
            _editModifier = EDIT_MODIFIER_PREF.Value;

            var dragModifierLabel = new GUIContent("Drag Modifier", DRAG_MODIFIER_HINT);
            DRAG_MODIFIER_PREF = new EditorPrefsModifierKey(DRAG_MODIFIER_PKEY + projectName, DRAG_MODIFIER_DEFAULT, dragModifierLabel);
            _dragModifier = EDIT_MODIFIER_PREF.Value;
            
            var hierarchyTreeLabel = new GUIContent("Project Tree", PROJECT_TREE_HINT);
            PROJECT_TREE_PREF = new EditorPrefsBooleanRepaint(PROJECT_TREE_PKEY + projectName, PROJECT_TREE_DEFAULT, hierarchyTreeLabel, PREF_LABEL_WIDTH);
            ShowProjectTree = PROJECT_TREE_PREF.Value;
            
            var rowShadingLabel = new GUIContent("Row Shading", ROW_SHADING_HINT);
            ROW_SHADING_PREF = new EditorPrefsBooleanRepaint(ROW_SHADING_PKEY + projectName, ROW_SHADING_DEFAULT, rowShadingLabel, PREF_LABEL_WIDTH);
            DrawRowShading = ROW_SHADING_PREF.Value;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        [SettingsProvider]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static SettingsProvider CreateSettingProvider()
        {
            return new SettingsProvider("Borodar/RainbowFolders", SettingsScope.Project)
            {
                label = AssetInfo.NAME,
                guiHandler = OnGUI,
                activateHandler = OnActivate
            };
        }

        public static void UpdateRulesetPath(string path, bool updatePref, bool reloadInstance)
        {
            if (RulesetPath == path) return;
            if (updatePref) RULESET_PREF.Value = path;

            RulesetPath = path;

            if (!reloadInstance) return;

            ProjectRuleset.UpdateInstance();
            EditorApplication.RepaintProjectWindow();
        }

        public static bool IsEditModifierPressed(Event e)
        {
            return (e.modifiers & _editModifier) == _editModifier;
        }

        public static bool IsDragModifierPressed(Event e)
        {
            return (e.modifiers & _dragModifier) == _dragModifier;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private static void OnActivate(string searchContext, VisualElement rootElement)
        {
            var options = ProjectEditorUtility.FindPathsForAllRulesets();
            var optionNames = options.Select(path => path.Replace('/', '\\')).ToArray();
            RULESET_PREF.UpdateOptions(options, optionNames);
        }

        private static void OnGUI(string searchContext)
        {
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            RULESET_PREF.Draw();
            UpdateRulesetPath(RULESET_PREF.Value, false, true);
            TinySeparator();

            EDIT_MODIFIER_PREF.Draw();
            _editModifier = EDIT_MODIFIER_PREF.Value;

            DRAG_MODIFIER_PREF.Draw();
            _dragModifier = DRAG_MODIFIER_PREF.Value;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Enhancements", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            PROJECT_TREE_PREF.Draw();
            ShowProjectTree = PROJECT_TREE_PREF.Value;
            TinySeparator();

            ROW_SHADING_PREF.Draw();
            DrawRowShading = ROW_SHADING_PREF.Value;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Import / Export Ruleset", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Import")) ProjectRulesetExporter.Import();
                if (GUILayout.Button("Export")) ProjectRulesetExporter.Export();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Version " + AssetInfo.VERSION, EditorStyles.centeredGreyMiniLabel);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void TinySeparator()
        {
            GUILayoutUtility.GetRect(0f, 0f);
        }
    }
}
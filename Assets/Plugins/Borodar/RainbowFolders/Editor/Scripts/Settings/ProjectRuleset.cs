using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static Borodar.RainbowFolders.RFLogger;
using KeyType = Borodar.RainbowFolders.ProjectRule.KeyType;

namespace Borodar.RainbowFolders
{
    [HelpURL(AssetInfo.HELP_URL)]

    public class ProjectRuleset : ScriptableObject
    {
        public const int VERSION = 1;

        private const double INSTANCE_CHECK_INTERVAL = 1d;
        private const string UNDO_GROUP_NAME = "Modify Rainbow Folder Ruleset";

        // avoid unnecessary memory allocations
        private static readonly ProjectRule RECURSIVE_RULE = new ProjectRule(KeyType.Name, string.Empty);

        [FormerlySerializedAs("Folders")]
        public List<ProjectRule> Rules = new List<ProjectRule>();

        private readonly Dictionary<string, ProjectRule> _rulesByName = new Dictionary<string, ProjectRule>();
        private readonly List<ProjectRule> _rulesByNameRecursive = new List<ProjectRule>();
        private readonly Dictionary<string, ProjectRule> _rulesByPath = new Dictionary<string, ProjectRule>();
        private readonly List<ProjectRule> _rulesByPathRecursive = new List<ProjectRule>();

        private static double _nextInstanceCheck;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public static Action OnRulesetChange;

        //---------------------------------------------------------------------
        // Instance
        //---------------------------------------------------------------------

        private static ProjectRuleset _instance;

        public static ProjectRuleset Instance
        {
            get
            {
                if (_instance == null && IsReadyForInstanceCheck()) UpdateInstance();
                return _instance;
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        // Please note that this could be called multiple times without corresponding OnDisable call
        // For example, when renaming scriptable object in Project Browser
        private void OnEnable()
        {
            SubscribeToUndo();
            OnRulesetChange();
        }

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        private void OnDisable()
        {
            UnsubscribeFromUndo();
        }

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        private void OnDestroy()
        {
            UnsubscribeFromRulesetChanges();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static void UpdateInstance()
        {
            _instance = LoadRulesetFromAsset();
            if (_instance == null) return;

            _instance.UpdateOrdinals();
            _instance.UpdateDictionaries();

            SubscribeToRulesetChanges();
        }

        public static void ShowInspector(DefaultAsset asset = null)
        {
            Selection.activeObject = Instance;
            // Workaround with double delay to make sure all Inspectors already enabled
            EditorApplication.delayCall += () => EditorApplication.delayCall += () =>
            {
                foreach (var editor in ProjectRulesetEditor.EDITORS)
                {
                    editor.Asset = asset;
                    editor.ForceUpdate = true;
                    editor.SearchTab = 0;
                    editor.Repaint();
                }
            };
        }

        /// <summary>  
        /// Searches for a rule that has the same type and key values.
        /// Returns the first occurrence within the settings, if found; null otherwise.
        /// </summary>  
        public ProjectRule GetRule(ProjectRule match)
        {
            if (IsNullOrEmpty(Rules) || match == null) return null;
            return Rules.Find(x => x.Type == match.Type && x.Key == match.Key);
        }

        /// <summary>
        /// Searches for a folder config that should be applied for the specified path (regardless of
        /// the key type). Returns the last occurrence within the settings, if found; null otherwise.
        /// </summary>  
        public ProjectRule GetRuleByPath(string folderPath, bool allowRecursive = false)
        {
            if (IsNullOrEmpty(Rules)) return null;

            var assetName = GetAssetNameFromPath(folderPath);
            ProjectRule result = null;

            if (allowRecursive)
            {
                foreach (var nameRecursive in _rulesByNameRecursive.
                     Where(nameRecursive => folderPath.Contains($"/{nameRecursive.Key}/")))
                     ReplaceWithHighestPriority(ref result, nameRecursive, true);
            }

            _rulesByName.TryGetValue(assetName, out var ruleByName);
            ReplaceWithHighestPriority(ref result, ruleByName);

            if (allowRecursive)
            {
                foreach (var pathRecursive in _rulesByPathRecursive.
                    Where(pathRecursive => folderPath.StartsWith($"{pathRecursive.Key}/")))
                    ReplaceWithHighestPriority(ref result, pathRecursive, true);
            }

            _rulesByPath.TryGetValue(folderPath, out var folderByPath);
            ReplaceWithHighestPriority(ref result, folderByPath);

            return result;
        }

        /// <summary>  
        /// Searches for a folder config that has the same type and key, and updates
        /// its other fields with provided value, if found; creates new folder config otherwise.
        /// </summary>  
        public void UpdateRule(ProjectRule match, ProjectRule value)
        {
            Undo.RecordObject(this, UNDO_GROUP_NAME);

            var existingFolder = GetRule(match);
            if (existingFolder != null)
            {
                if (value.HasAtLeastOneTexture())
                {
                    existingFolder.CopyFrom(value);
                    SaveSetting();
                }
                else
                {
                    RemoveAll(match);
                }
            }
            else
            {
                if (value.HasAtLeastOneTexture()) AddRule(value);
            }
        }

        public void AddRule(ProjectRule value)
        {
            Rules.Add(new ProjectRule(value));
            SaveSetting();
        }

        public void RemoveAll(ProjectRule match)
        {
            if (match == null) return;
            Undo.RecordObject(this, UNDO_GROUP_NAME);
            Rules.RemoveAll(x => x.Type == match.Type && x.Key == match.Key);
            SaveSetting();
        }

        public void SaveSetting()
        {
            EditorUtility.SetDirty(this);
            OnRulesetChange();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "InvertIf")]
        private static bool IsReadyForInstanceCheck()
        {
            if (EditorApplication.timeSinceStartup > _nextInstanceCheck)
            {
                _nextInstanceCheck = EditorApplication.timeSinceStartup + INSTANCE_CHECK_INTERVAL;
                return true;
            }

            return false;
        }

        private static ProjectRuleset LoadRulesetFromAsset()
        {
            var assetPath = ProjectPreferences.RulesetPath;
            var ruleset = AssetDatabase.LoadAssetAtPath<ProjectRuleset>(assetPath);

            if (ruleset) return ruleset;

            LogWarning($"There is no ruleset at path: {assetPath}\n" +
                        "Trying to find another ruleset in project... ");

            assetPath = ProjectEditorUtility.FindPathsForAllRulesets().FirstOrDefault();
            ruleset = AssetDatabase.LoadAssetAtPath<ProjectRuleset>(assetPath);

            if (ruleset)
            {
                Log($"Found another ruleset at path: {assetPath}\n" +
                            "You could select different one by using \"Edit → Project Settings → Rainbow Folders → Current Ruleset\"");

                ProjectPreferences.UpdateRulesetPath(assetPath, true, false);
                return ruleset;
            }

            assetPath = ProjectEditorUtility.CreateAsset<ProjectRuleset>("RainbowFoldersRuleset");
            ruleset = AssetDatabase.LoadAssetAtPath<ProjectRuleset>(assetPath);

            if (ruleset)
            {
                Log($"No rulesets found. Created new ruleset at path: {assetPath}\n");

                ProjectPreferences.UpdateRulesetPath(assetPath, true, false);
                return ruleset;
            }

            LogError("Could not find any ruleset in the project.\n" +
                     "Please make sure that your project contains at least one ruleset and it has been imported correctly.");

            return null;
        }

        private static void SubscribeToRulesetChanges()
        {
            UnsubscribeFromRulesetChanges();
            OnRulesetChange += _instance.UpdateOrdinals;
            OnRulesetChange += _instance.UpdateDictionaries;
        }

        private static void UnsubscribeFromRulesetChanges()
        {
            OnRulesetChange -= _instance.UpdateOrdinals;
            OnRulesetChange -= _instance.UpdateDictionaries;
        }

        private void SubscribeToUndo()
        {
            UnsubscribeFromUndo();
            Undo.undoRedoPerformed += PerformUndoRedo;
        }

        private void UnsubscribeFromUndo()
        {
            Undo.undoRedoPerformed -= PerformUndoRedo;
        }

        private void UpdateOrdinals()
        {
            for (var i = 0; i < Rules.Count; i++)
            {
                Rules[i].Ordinal = i;
            }
        }

        private void UpdateDictionaries()
        {
            _rulesByName.Clear();
            _rulesByNameRecursive.Clear();
            _rulesByPath.Clear();
            _rulesByPathRecursive.Clear();

            foreach (var rule in Rules)
            {
                switch (rule.Type)
                {
                    case KeyType.Name:
                        if (_rulesByName.TryGetValue(rule.Key, out var valueByName))
                        {
                            ReplaceWithHighestPriority(ref valueByName, rule);
                            _rulesByName[rule.Key] = valueByName;
                        }
                        else
                        {
                            _rulesByName.Add(rule.Key, rule);
                        }

                        if (rule.IsRecursive()) _rulesByNameRecursive.Add(rule);
                        break;

                    case KeyType.Path:
                        if (_rulesByPath.TryGetValue(rule.Key, out var valueByPath))
                        {
                            ReplaceWithHighestPriority(ref valueByPath, rule);
                            _rulesByPath[rule.Key] = valueByPath;
                        }
                        else
                        {
                            _rulesByPath.Add(rule.Key, rule);
                        }

                        if (rule.IsRecursive()) _rulesByPathRecursive.Add(rule);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void ReplaceWithHighestPriority(ref ProjectRule result, ProjectRule replacement, bool recursive = false)
        {
            if (result == null)
            {
                result = replacement;
            }
            else if (replacement != null && replacement.Priority >= result.Priority)
            {
                if (replacement.Priority == result.Priority)
                {
                    if (replacement.Ordinal > result.Ordinal)
                    {
                        result = replacement;
                    }
                }
                else
                {
                    result = replacement;
                }
            }

            if (recursive) result = CopyRecursiveItem(result);
        }

        private static ProjectRule CopyRecursiveItem(ProjectRule rule)
        {
            RECURSIVE_RULE.CopyFrom(rule);

            if (!rule.IsIconRecursive)
            {
                RECURSIVE_RULE.IconType = ProjectIcon.None;
            }

            if (!rule.IsBackgroundRecursive)
            {
                RECURSIVE_RULE.BackgroundType = ProjectBackground.None;
            }

            return RECURSIVE_RULE;
        }

        private static string GetAssetNameFromPath(string path)
        {
            var nameIndex = path.LastIndexOf("/", StringComparison.Ordinal) + 1;
            return path.Substring(nameIndex);
        }

        private static bool IsNullOrEmpty(ICollection collection)
        {
            return collection == null || (collection.Count == 0);
        }

        private void PerformUndoRedo()
        {
            if (UNDO_GROUP_NAME.Equals(Undo.GetCurrentGroupName()))
            {
                SaveSetting();
            }
        }
    }
}
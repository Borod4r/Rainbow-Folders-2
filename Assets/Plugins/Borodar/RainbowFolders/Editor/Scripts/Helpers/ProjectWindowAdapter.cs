using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    public static class ProjectWindowAdapter
    {
        private const string EDITOR_WINDOW_TYPE = "UnityEditor.ProjectBrowser";

        private const BindingFlags STATIC_PRIVATE = BindingFlags.Static | BindingFlags.NonPublic;        
        private const BindingFlags STATIC_PUBLIC = BindingFlags.Static | BindingFlags.Public;
        private const BindingFlags INSTANCE_PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags INSTANCE_PUBLIC = BindingFlags.Instance | BindingFlags.Public;

        // Project Browser
        private static MethodInfo _allProjectBrowsersMethod;
        private static MethodInfo _projectBrowserInitializedMethod;
        // First Column
        private static FieldInfo _projectViewModeField;
        private static FieldInfo _projectAssetTreeField;
        private static FieldInfo _projectFolderTreeField;
        private static FieldInfo _controllerDragSelectionField;
        private static FieldInfo _integerCacheListField;

        private static PropertyInfo _controllerDataProperty;
        private static PropertyInfo _controllerStateProperty;
        private static PropertyInfo _controllerGUICallbackProperty;
        private static  MethodInfo _controllerHasFocusMethod;
        private static PropertyInfo _stateSelectedIdsProperty;
        private static MethodInfo _twoColumnItemsMethod;
        private static MethodInfo _oneColumnItemsMethod;
        // Second Column
        private static FieldInfo _projectObjectListField;
        private static FieldInfo _projectLocalAssetsField;
        private static PropertyInfo _objectListRepaintCallback;
        private static FieldInfo _objectListIconEvent;
        private static PropertyInfo _assetsListModeProperty;
        private static FieldInfo _listFilteredHierarchyField;
        private static PropertyInfo _filteredHierarchyResultsMethod;
        // Filter Result
        private static FieldInfo _filterResultIDField;
        private static FieldInfo _filterResultIsFolderField;
        private static PropertyInfo _filterResultIconProperty;

        private static bool _isInitialized;
        private static bool _isInternalApiSupported;

        //---------------------------------------------------------------------
        // Initialization
        //---------------------------------------------------------------------

        private static bool EnsureInitialized()
        {
            if (_isInitialized) return _isInternalApiSupported;
            _isInitialized = true;

            try
            {
                // Reflections

                var assembly = Assembly.GetAssembly(typeof(EditorWindow));

                // Project Browser

                var projectWindowType = assembly.GetType(EDITOR_WINDOW_TYPE);
                _allProjectBrowsersMethod = projectWindowType.GetMethod("GetAllProjectBrowsers", STATIC_PUBLIC);
                _projectBrowserInitializedMethod = projectWindowType.GetMethod("Initialized", INSTANCE_PUBLIC);

                // First Column

                _projectViewModeField = projectWindowType.GetField("m_ViewMode", INSTANCE_PRIVATE);
                _projectAssetTreeField = projectWindowType.GetField("m_AssetTree", INSTANCE_PRIVATE);
                _projectFolderTreeField = projectWindowType.GetField("m_FolderTree", INSTANCE_PRIVATE);

                var treeViewControllerTypeGeneric = assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController`1");
                var treeViewControllerType = treeViewControllerTypeGeneric.MakeGenericType(typeof(EntityId));

                _controllerDragSelectionField = treeViewControllerType.GetField("m_DragSelection", INSTANCE_PRIVATE);

                _integerCacheListField = treeViewControllerTypeGeneric.GetNestedType("IntegerCache", INSTANCE_PRIVATE)
                    .MakeGenericType(typeof(EntityId))
                    .GetField("m_List", INSTANCE_PRIVATE);

                _controllerDataProperty = treeViewControllerType.GetProperty("data", INSTANCE_PUBLIC);
                _controllerStateProperty = treeViewControllerType.GetProperty("state", INSTANCE_PUBLIC);
                _controllerGUICallbackProperty = treeViewControllerType.GetProperty("onGUIRowCallback", INSTANCE_PUBLIC);
                _controllerHasFocusMethod = treeViewControllerType.GetMethod("HasFocus", INSTANCE_PUBLIC);

                var treeViewStateGeneric = assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewState`1");
                var treeViewState = treeViewStateGeneric.MakeGenericType(typeof(EntityId));

                _stateSelectedIdsProperty = treeViewState.GetProperty("selectedIDs", INSTANCE_PUBLIC);

                var oneColumnTreeViewDataType = assembly.GetType("UnityEditor.ProjectBrowserColumnOneTreeViewDataSource");
                _twoColumnItemsMethod = oneColumnTreeViewDataType.GetMethod("GetRows", INSTANCE_PUBLIC);

                var twoColumnTreeViewDataType = assembly.GetType("UnityEditor.AssetsTreeViewDataSource");
                _oneColumnItemsMethod = twoColumnTreeViewDataType.GetMethod("GetRows", INSTANCE_PUBLIC);

                // Second Column

                _projectObjectListField = projectWindowType.GetField("m_ListArea", INSTANCE_PRIVATE);

                var objectListType = assembly.GetType("UnityEditor.ObjectListArea");
                _projectLocalAssetsField = objectListType.GetField("m_LocalAssets", INSTANCE_PRIVATE);
                _objectListRepaintCallback = objectListType.GetProperty("repaintCallback", INSTANCE_PUBLIC);
                _objectListIconEvent = objectListType.GetField("postAssetIconDrawCallback", STATIC_PRIVATE);

                var localGroupType = objectListType.GetNestedType("LocalGroup", INSTANCE_PRIVATE);
                _assetsListModeProperty = localGroupType.GetProperty("ListMode", INSTANCE_PUBLIC);
                _listFilteredHierarchyField = localGroupType.GetField("m_FilteredHierarchy", INSTANCE_PRIVATE);

                var filteredHierarchyType = assembly.GetType("UnityEditor.FilteredHierarchy");
                _filteredHierarchyResultsMethod = filteredHierarchyType.GetProperty("results", INSTANCE_PUBLIC);

                // Filter Result

                var filterResultType = filteredHierarchyType.GetNestedType("FilterResult");
                _filterResultIDField = filterResultType.GetField("entityId", INSTANCE_PUBLIC);
                _filterResultIsFolderField = filterResultType.GetField("isFolder", INSTANCE_PUBLIC);
                _filterResultIconProperty = filterResultType.GetProperty("icon", INSTANCE_PUBLIC);

                // Callbacks
                ProjectRuleset.OnRulesetChange -= ApplyDefaultIconsToSecondColumn;
                ProjectRuleset.OnRulesetChange += ApplyDefaultIconsToSecondColumn;

                _isInternalApiSupported = true;
            }
            catch (Exception ex)
            {
                RFLogger.LogWarning($"Extension disabled. Unity internal API change detected: {ex.Message}");
                _isInternalApiSupported = false;
            }

            return _isInternalApiSupported;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
        public static IReadOnlyList<EditorWindow> GetAllProjectWindows()
        {
            if (!EnsureInitialized()) return new List<EditorWindow>();

            var browsersList = _allProjectBrowsersMethod.Invoke(null, null);
            return (IReadOnlyList<EditorWindow>) browsersList;

        }
        
        public static EditorWindow GetFirstProjectWindow()
        {
            return GetAllProjectWindows().FirstOrDefault();
        }

        public static object GetAssetTreeController(EditorWindow window)
        {
            if (!EnsureInitialized()) return null;
            return _projectAssetTreeField.GetValue(window);
        }

        public static object GetFolderTreeController(EditorWindow window)
        {
            if (!EnsureInitialized()) return null;
            return _projectFolderTreeField.GetValue(window);
        }

        public static object GetTreeViewState(object treeViewController)
        {
            if (!EnsureInitialized()) return null;
            return _controllerStateProperty.GetValue(treeViewController);
        }

        public static bool HasChildren(EditorWindow window, EntityId assetId)
        {
            if (!EnsureInitialized()) return false;

            var treeViewItems = GetFirstColumnItems(window);
            if (treeViewItems == null) return false;

            var treeViewItem = treeViewItems.FirstOrDefault(item => item.id == assetId);
            return treeViewItem != null && treeViewItem.hasChildren;
        }

        public static bool IsItemSelected(object treeViewController, object state, EntityId assetId)
        {
            if (!EnsureInitialized()) return false;

            var dragSelectionField = _controllerDragSelectionField.GetValue(treeViewController);
            var dragSelection = (List<EntityId>) _integerCacheListField.GetValue(dragSelectionField);

            if (dragSelection != null && dragSelection.Count > 0)
            {
                return dragSelection.Contains(assetId);
            }
            else
            {
                var selectedIds = (List<EntityId>) _stateSelectedIdsProperty.GetValue(state);
                return selectedIds.Contains(assetId);
            }
        }

        public static bool HasFocus(object treeViewController)
        {
            if (!EnsureInitialized()) return false;
            return (bool) _controllerHasFocusMethod.Invoke(treeViewController, null);
        }

        public static ViewMode GetProjectViewMode(EditorWindow window)
        {
            if (!EnsureInitialized()) return ViewMode.OneColumn;
            return (ViewMode) _projectViewModeField.GetValue(window);
        }

        public static bool ProjectWindowInitialized(EditorWindow window)
        {
            if (!EnsureInitialized()) return false;
            return (bool) _projectBrowserInitializedMethod.Invoke(window, null);
        }

        public static object GetObjectListArea(EditorWindow window)
        {
            if (!EnsureInitialized()) return null;
            return _projectObjectListField.GetValue(window);
        }

        public static void ReplaceIconsInListArea(object objectListArea, ProjectRuleset ruleset)
        {
            if (!EnsureInitialized()) return;

            var localAssets = _projectLocalAssetsField.GetValue(objectListArea);
            var inListMode = InListMode(localAssets);
            var filteredHierarchy = _listFilteredHierarchyField.GetValue(localAssets);
            var items = _filteredHierarchyResultsMethod.GetValue(filteredHierarchy, null);

            foreach (var item in (IEnumerable<object>) items)
            {
                if (!ListItemIsFolder(item)) continue;
                var id = GetInstanceIdFromListItem(item);
                var path = AssetDatabase.GetAssetPath(id);
                var rule = ruleset.GetRuleByPath(path,true);
                if (rule == null || !rule.HasIcon()) continue;

                Texture2D iconTex = null;
                if (rule.HasCustomIcon())
                {
                    iconTex = inListMode ? rule.SmallIcon : rule.LargeIcon;
                }
                else
                {
                    var icons = ProjectIconsStorage.GetIcons(rule.IconType);
                    if (icons != null)
                    {
                        iconTex = inListMode ? icons.Item2 : icons.Item1;
                    }
                }

                if (iconTex != null) SetIconForListItem(item, iconTex);
            }
        }

        //---------------------------------------------------------------------
        // Callbacks
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        public static void AddOnGUIRowCallback(object treeViewController, Action<EntityId, Rect> action)
        {
            if (!EnsureInitialized()) return;

            var value = (Action<EntityId, Rect>) _controllerGUICallbackProperty.GetValue(treeViewController);
            _controllerGUICallbackProperty.SetValue(treeViewController, action + value);
        }

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        public static void RemoveOnGUIRowCallback(object treeViewController, Action<EntityId, Rect> action)
        {
            if (!EnsureInitialized()) return;

            var value = (Action<EntityId, Rect>) _controllerGUICallbackProperty.GetValue(treeViewController);
            _controllerGUICallbackProperty.SetValue(treeViewController, value - action);
        }

        public static void AddRepaintCallback(object objectListArea, Action repaintCallback)
        {
            if (!EnsureInitialized()) return;

            var value = (Action) _objectListRepaintCallback.GetValue(objectListArea);
            _objectListRepaintCallback.SetValue(objectListArea, value + repaintCallback);
        }

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        public static void RemoveRepaintCallback(object objectListArea, Action repaintCallback)
        {
            if (!EnsureInitialized()) return;

            var value = (Action) _objectListRepaintCallback.GetValue(objectListArea);
            _objectListRepaintCallback.SetValue(objectListArea, value - repaintCallback);
        }

        public static void AddPostAssetIconDrawCallback(Type target, string method)
        {
            if (!EnsureInitialized()) return;

            var tempDelegate = Delegate.CreateDelegate(_objectListIconEvent.FieldType, target, method);
            var value = (Delegate) _objectListIconEvent.GetValue(null);
            _objectListIconEvent.SetValue(null, Delegate.Combine(tempDelegate, value));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "InvertIf")]
        private static IEnumerable<TreeViewItem<EntityId>> GetFirstColumnItems(EditorWindow window)
        {
            var oneColumnTree = _projectAssetTreeField.GetValue(window);
            if (oneColumnTree != null)
            {                
                var treeViewData = _controllerDataProperty.GetValue(oneColumnTree, null);
                var treeViewItems = (IEnumerable<TreeViewItem<EntityId>>) _oneColumnItemsMethod.Invoke(treeViewData, null);
                return treeViewItems;
            }
            
            var twoColumnTree = _projectFolderTreeField.GetValue(window);
            if (twoColumnTree != null)
            {                
                var treeViewData = _controllerDataProperty.GetValue(twoColumnTree, null);
                var treeViewItems = (IEnumerable<TreeViewItem<EntityId>>) _twoColumnItemsMethod.Invoke(treeViewData, null);
                return treeViewItems;
            }

            return null;
        }

        private static IEnumerable<object> GetSecondColumnItems(EditorWindow window, bool onlyInListMode = false)
        {
            var assetsList = _projectObjectListField.GetValue(window);
            if (assetsList == null) return null;
            
            var localAssets = _projectLocalAssetsField.GetValue(assetsList);
            if (onlyInListMode && !InListMode(localAssets)) return null;
                
            var filteredHierarchy = _listFilteredHierarchyField.GetValue(localAssets);
            var results = _filteredHierarchyResultsMethod.GetValue(filteredHierarchy, null);
                
            return (IEnumerable<object>) results;
        }

        private static void ApplyDefaultIconsToSecondColumn()
        {
            foreach (var window in GetAllProjectWindows())
            {
                var listItems = GetSecondColumnItems(window);
                if (listItems == null) continue;

                foreach (var item in listItems) SetIconForListItem(item, null);

                // Repaint the current project window
                window.Repaint();
            }
        }

        private static bool InListMode(object localAssets)
        {
            return (bool) _assetsListModeProperty.GetValue(localAssets, null);
        }

        private static EntityId GetInstanceIdFromListItem(object listItem)
        {
            return (EntityId) _filterResultIDField.GetValue(listItem);
        }

        private static void SetIconForListItem(object listItem, Texture2D icon)
        {
            _filterResultIconProperty.SetValue(listItem, icon, null);
        }

        private static bool ListItemIsFolder(object listItem)
        {
            return (bool) _filterResultIsFolderField.GetValue(listItem);
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum ViewMode
        {
            OneColumn,
            TwoColumns,
        }
    }
}
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
        private static readonly MethodInfo ALL_PROJECT_BROWSERS_METHOD;
        private static readonly MethodInfo PROJECT_BROWSER_INITIALIZED_METHOD;
        // First Column
        private static readonly FieldInfo PROJECT_VIEW_MODE_FIELD;
        private static readonly FieldInfo PROJECT_ASSET_TREE_FIELD;
        private static readonly FieldInfo PROJECT_FOLDER_TREE_FIELD;
        private static readonly FieldInfo CONTROLLER_DRAG_SELECTION_FIELD;
        private static readonly FieldInfo INTEGER_CACHE_LIST_FIELD;

        private static readonly PropertyInfo CONTROLLER_DATA_PROPERTY;
        private static readonly PropertyInfo CONTROLLER_STATE_PROPERTY;
        private static readonly PropertyInfo CONTROLLER_GUI_CALLBACK_PROPERTY;
        private static readonly MethodInfo CONTROLLER_HAS_FOCUS_METHOD;
        private static readonly PropertyInfo STATE_SELECTED_IDS_PROPERTY;
        private static readonly MethodInfo TWO_COLUMN_ITEMS_METHOD;
        private static readonly MethodInfo ONE_COLUMN_ITEMS_METHOD;
        // Second Column
        private static readonly FieldInfo PROJECT_OBJECT_LIST_FIELD;
        private static readonly FieldInfo PROJECT_LOCAL_ASSETS_FIELD;
        private static readonly PropertyInfo OBJECT_LIST_REPAINT_CALLBACK;
        private static readonly FieldInfo OBJECT_LIST_ICON_EVENT;
        private static readonly PropertyInfo ASSETS_LIST_MODE_PROPERTY;
        private static readonly FieldInfo LIST_FILTERED_HIERARCHY_FIELD;
        private static readonly PropertyInfo FILTERED_HIERARCHY_RESULTS_METHOD;
        // Filter Result
        private static readonly FieldInfo FILTER_RESULT_ID_FIELD;
        private static readonly FieldInfo FILTER_RESULT_IS_FOLDER_FIELD;
        private static readonly PropertyInfo FILTER_RESULT_ICON_PROPERTY;

        //---------------------------------------------------------------------
        // Ctor
        //---------------------------------------------------------------------

        static ProjectWindowAdapter()
        {
            // Reflections            

            var assembly = Assembly.GetAssembly(typeof(EditorWindow));

            // Project Browser

            var projectWindowType = assembly.GetType(EDITOR_WINDOW_TYPE);
            ALL_PROJECT_BROWSERS_METHOD = projectWindowType.GetMethod("GetAllProjectBrowsers", STATIC_PUBLIC);
            PROJECT_BROWSER_INITIALIZED_METHOD = projectWindowType.GetMethod("Initialized", INSTANCE_PUBLIC);
            
            // First Column

            PROJECT_VIEW_MODE_FIELD = projectWindowType.GetField("m_ViewMode", INSTANCE_PRIVATE);
            PROJECT_ASSET_TREE_FIELD = projectWindowType.GetField("m_AssetTree", INSTANCE_PRIVATE);
            PROJECT_FOLDER_TREE_FIELD = projectWindowType.GetField("m_FolderTree", INSTANCE_PRIVATE);

            var treeViewControllerTypeGeneric = assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController`1");
            var treeViewControllerType = treeViewControllerTypeGeneric.MakeGenericType(typeof(EntityId));

            CONTROLLER_DRAG_SELECTION_FIELD = treeViewControllerType.GetField("m_DragSelection", INSTANCE_PRIVATE);

            INTEGER_CACHE_LIST_FIELD = treeViewControllerTypeGeneric.GetNestedType("IntegerCache", INSTANCE_PRIVATE)
                .MakeGenericType(typeof(EntityId))
                .GetField("m_List", INSTANCE_PRIVATE);

            CONTROLLER_DATA_PROPERTY = treeViewControllerType.GetProperty("data", INSTANCE_PUBLIC);
            CONTROLLER_STATE_PROPERTY = treeViewControllerType.GetProperty("state", INSTANCE_PUBLIC);
            CONTROLLER_GUI_CALLBACK_PROPERTY = treeViewControllerType.GetProperty("onGUIRowCallback", INSTANCE_PUBLIC);
            CONTROLLER_HAS_FOCUS_METHOD = treeViewControllerType.GetMethod("HasFocus", INSTANCE_PUBLIC);

            var treeViewStateGeneric = assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewState`1");
            var treeViewState = treeViewStateGeneric.MakeGenericType(typeof(EntityId));

            STATE_SELECTED_IDS_PROPERTY = treeViewState.GetProperty("selectedIDs", INSTANCE_PUBLIC);

            var oneColumnTreeViewDataType = assembly.GetType("UnityEditor.ProjectBrowserColumnOneTreeViewDataSource");
            TWO_COLUMN_ITEMS_METHOD = oneColumnTreeViewDataType.GetMethod("GetRows", INSTANCE_PUBLIC);
            
            var twoColumnTreeViewDataType = assembly.GetType("UnityEditor.AssetsTreeViewDataSource");
            ONE_COLUMN_ITEMS_METHOD = twoColumnTreeViewDataType.GetMethod("GetRows", INSTANCE_PUBLIC);
            
            // Second Column

            PROJECT_OBJECT_LIST_FIELD = projectWindowType.GetField("m_ListArea", INSTANCE_PRIVATE);
            
            var objectListType = assembly.GetType("UnityEditor.ObjectListArea");            
            PROJECT_LOCAL_ASSETS_FIELD = objectListType.GetField("m_LocalAssets", INSTANCE_PRIVATE);
            OBJECT_LIST_REPAINT_CALLBACK = objectListType.GetProperty("repaintCallback", INSTANCE_PUBLIC);
            OBJECT_LIST_ICON_EVENT = objectListType.GetField("postAssetIconDrawCallback", STATIC_PRIVATE);

            var localGroupType = objectListType.GetNestedType("LocalGroup", INSTANCE_PRIVATE);
            ASSETS_LIST_MODE_PROPERTY = localGroupType.GetProperty("ListMode", INSTANCE_PUBLIC);
            LIST_FILTERED_HIERARCHY_FIELD = localGroupType.GetField("m_FilteredHierarchy", INSTANCE_PRIVATE);
            
            var filteredHierarchyType = assembly.GetType("UnityEditor.FilteredHierarchy");            
            FILTERED_HIERARCHY_RESULTS_METHOD = filteredHierarchyType.GetProperty("results", INSTANCE_PUBLIC);
            
            // Filter Result
            
            var filterResultType = filteredHierarchyType.GetNestedType("FilterResult");
            FILTER_RESULT_ID_FIELD = filterResultType.GetField("entityId", INSTANCE_PUBLIC);
            FILTER_RESULT_IS_FOLDER_FIELD = filterResultType.GetField("isFolder", INSTANCE_PUBLIC);
            FILTER_RESULT_ICON_PROPERTY = filterResultType.GetProperty("icon", INSTANCE_PUBLIC);

            // Callbacks

            ProjectRuleset.OnRulesetChange += ApplyDefaultIconsToSecondColumn;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
        public static IReadOnlyList<EditorWindow> GetAllProjectWindows()
        {
            var browsersList = ALL_PROJECT_BROWSERS_METHOD.Invoke(null, null);
            return (IReadOnlyList<EditorWindow>) browsersList;
        }
        
        public static EditorWindow GetFirstProjectWindow()
        {
            return GetAllProjectWindows().FirstOrDefault();
        }

        public static object GetAssetTreeController(EditorWindow window)
        {
            return PROJECT_ASSET_TREE_FIELD.GetValue(window);
        }

        public static object GetFolderTreeController(EditorWindow window)
        {
            return PROJECT_FOLDER_TREE_FIELD.GetValue(window);
        }

        public static object GetTreeViewState(object treeViewController)
        {
            return CONTROLLER_STATE_PROPERTY.GetValue(treeViewController);
        }

        public static bool HasChildren(EditorWindow window, EntityId assetId)
        {
            var treeViewItems = GetFirstColumnItems(window);
            if (treeViewItems == null) return false;

            var treeViewItem = treeViewItems.FirstOrDefault(item => item.id == assetId);
            return treeViewItem != null && treeViewItem.hasChildren;
        }

        public static bool IsItemSelected(object treeViewController, object state, EntityId assetId)
        {
            var dragSelectionField = CONTROLLER_DRAG_SELECTION_FIELD.GetValue(treeViewController);
            var dragSelection = (List<EntityId>) INTEGER_CACHE_LIST_FIELD.GetValue(dragSelectionField);

            if (dragSelection != null && dragSelection.Count > 0)
            {
                return dragSelection.Contains(assetId);
            }
            else
            {
                var selectedIds = (List<EntityId>) STATE_SELECTED_IDS_PROPERTY.GetValue(state);
                return selectedIds.Contains(assetId);
            }
        }

        public static bool HasFocus(object treeViewController)
        {
            return (bool) CONTROLLER_HAS_FOCUS_METHOD.Invoke(treeViewController, null);
        }

        public static ViewMode GetProjectViewMode(EditorWindow window)
        {
            return (ViewMode) PROJECT_VIEW_MODE_FIELD.GetValue(window);
        }

        public static bool ProjectWindowInitialized(EditorWindow window)
        {
            return (bool) PROJECT_BROWSER_INITIALIZED_METHOD.Invoke(window, null);
        }

        public static object GetObjectListArea(EditorWindow window)
        {
            return PROJECT_OBJECT_LIST_FIELD.GetValue(window);
        }

        public static void ReplaceIconsInListArea(object objectListArea, ProjectRuleset ruleset)
        {
            var localAssets = PROJECT_LOCAL_ASSETS_FIELD.GetValue(objectListArea);
            var inListMode = InListMode(localAssets);
            var filteredHierarchy = LIST_FILTERED_HIERARCHY_FIELD.GetValue(localAssets);
            var items = FILTERED_HIERARCHY_RESULTS_METHOD.GetValue(filteredHierarchy, null);

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
            var value = (Action<EntityId, Rect>) CONTROLLER_GUI_CALLBACK_PROPERTY.GetValue(treeViewController);
            CONTROLLER_GUI_CALLBACK_PROPERTY.SetValue(treeViewController, action + value);
        }

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        public static void RemoveOnGUIRowCallback(object treeViewController, Action<EntityId, Rect> action)
        {
            var value = (Action<EntityId, Rect>) CONTROLLER_GUI_CALLBACK_PROPERTY.GetValue(treeViewController);
            CONTROLLER_GUI_CALLBACK_PROPERTY.SetValue(treeViewController, value - action);
        }

        public static void AddRepaintCallback(object objectListArea, Action repaintCallback)
        {
            var value = (Action) OBJECT_LIST_REPAINT_CALLBACK.GetValue(objectListArea);
            OBJECT_LIST_REPAINT_CALLBACK.SetValue(objectListArea, value + repaintCallback);
        }

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        public static void RemoveRepaintCallback(object objectListArea, Action repaintCallback)
        {
            var value = (Action) OBJECT_LIST_REPAINT_CALLBACK.GetValue(objectListArea);
            OBJECT_LIST_REPAINT_CALLBACK.SetValue(objectListArea, value - repaintCallback);
        }

        public static void AddPostAssetIconDrawCallback(Type target, string method)
        {
            var tempDelegate = Delegate.CreateDelegate(OBJECT_LIST_ICON_EVENT.FieldType, target, method);
            var value = (Delegate) OBJECT_LIST_ICON_EVENT.GetValue(null);
            OBJECT_LIST_ICON_EVENT.SetValue(null, Delegate.Combine(tempDelegate, value));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "InvertIf")]
        private static IEnumerable<TreeViewItem<EntityId>> GetFirstColumnItems(EditorWindow window)
        {
            var oneColumnTree = PROJECT_ASSET_TREE_FIELD.GetValue(window);
            if (oneColumnTree != null)
            {                
                var treeViewData = CONTROLLER_DATA_PROPERTY.GetValue(oneColumnTree, null);
                var treeViewItems = (IEnumerable<TreeViewItem<EntityId>>) ONE_COLUMN_ITEMS_METHOD.Invoke(treeViewData, null);
                return treeViewItems;
            }
            
            var twoColumnTree = PROJECT_FOLDER_TREE_FIELD.GetValue(window);
            if (twoColumnTree != null)
            {                
                var treeViewData = CONTROLLER_DATA_PROPERTY.GetValue(twoColumnTree, null);
                var treeViewItems = (IEnumerable<TreeViewItem<EntityId>>) TWO_COLUMN_ITEMS_METHOD.Invoke(treeViewData, null);
                return treeViewItems;
            }

            return null;
        }

        private static IEnumerable<object> GetSecondColumnItems(EditorWindow window, bool onlyInListMode = false)
        {
            var assetsList = PROJECT_OBJECT_LIST_FIELD.GetValue(window);
            if (assetsList == null) return null;
            
            var localAssets = PROJECT_LOCAL_ASSETS_FIELD.GetValue(assetsList);                
            if (onlyInListMode && !InListMode(localAssets)) return null;
                
            var filteredHierarchy = LIST_FILTERED_HIERARCHY_FIELD.GetValue(localAssets);
            var results = FILTERED_HIERARCHY_RESULTS_METHOD.GetValue(filteredHierarchy, null);
                
            return (IEnumerable<object>) results;
        }

        private static void ApplyDefaultIconsToSecondColumn()
        {
            foreach (var window in GetAllProjectWindows())
            {
                var listItems = GetSecondColumnItems(window);
                if (listItems == null) continue;

                foreach (var item in listItems) SetIconForListItem(item, null);

                // Repaint current project window
                window.Repaint();
            }
        }

        private static bool InListMode(object localAssets)
        {
            return (bool) ASSETS_LIST_MODE_PROPERTY.GetValue(localAssets, null);
        }

        private static EntityId GetInstanceIdFromListItem(object listItem)
        {
            return (EntityId) FILTER_RESULT_ID_FIELD.GetValue(listItem);
        }

        private static void SetIconForListItem(object listItem, Texture2D icon)
        {
            FILTER_RESULT_ICON_PROPERTY.SetValue(listItem, icon, null);
        }

        private static bool ListItemIsFolder(object listItem)
        {
            return (bool) FILTER_RESULT_IS_FOLDER_FIELD.GetValue(listItem);
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
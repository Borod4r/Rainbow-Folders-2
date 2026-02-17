using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using StorageHelper = Borodar.RainbowFolders.TexturesStorageHelper<Borodar.RainbowFolders.ProjectIcon>;

namespace Borodar.RainbowFolders
{
    public static class ProjectIconsStorage
    {
        private static readonly Dictionary<ProjectIcon, Tuple<Texture2D, Texture2D>> ICON_TEXTURES;
        private static readonly Dictionary<ProjectIcon, Tuple<Lazy<string>, Lazy<string>>> ICON_STRINGS;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        static ProjectIconsStorage()
        {
            ICON_TEXTURES = new Dictionary<ProjectIcon, Tuple<Texture2D, Texture2D>>();
            ICON_STRINGS = (EditorGUIUtility.isProSkin) ? ProjectIconsArchivePro.GetDict()
                : ProjectIconsArchiveFree.GetDict();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static Tuple<Texture2D, Texture2D> GetIcons(int type)
        {
            return GetIcons((ProjectIcon) type);
        }

        public static Tuple<Texture2D, Texture2D> GetIcons(ProjectIcon type)
        {
            return StorageHelper.GetTextures(type, FilterMode.Bilinear, ICON_STRINGS, ICON_TEXTURES);
        }
    }
}
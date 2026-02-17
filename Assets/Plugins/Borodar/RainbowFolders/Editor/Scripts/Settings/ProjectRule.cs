using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    [Serializable]
    public class ProjectRule
    {
        public KeyType Type;
        public string Key;

        public int Ordinal;
        public int Priority;

        public ProjectIcon IconType;
        public Texture2D SmallIcon;
        public Texture2D LargeIcon;
        public bool IsIconRecursive;

        public ProjectBackground BackgroundType;
        public Texture2D BackgroundTexture;
        public bool IsBackgroundRecursive;

        [UsedImplicitly]
        public bool IsHidden;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public ProjectRule(ProjectRule value)
        {
            Type = value.Type;
            Key = value.Key;

            Ordinal = value.Ordinal;
            Priority = value.Priority;

            IconType = value.IconType;
            SmallIcon = value.SmallIcon;
            LargeIcon = value.LargeIcon;
            IsIconRecursive = value.IsIconRecursive;

            BackgroundType = value.BackgroundType;
            BackgroundTexture = value.BackgroundTexture;
            IsBackgroundRecursive = value.IsBackgroundRecursive;
        }

        public ProjectRule(KeyType type, string key)
        {
            Type = type;
            Key = key;
        }
        
        public ProjectRule(KeyType type, string key, ProjectIcon iconType)
        {
            Type = type;
            Key = key;
            IconType = iconType;
            SmallIcon = null;
            LargeIcon = null;
        }

        public ProjectRule(KeyType type, string key, Texture2D smallIcon, Texture2D largeIcon)
        {
            Type = type;
            Key = key;
            IconType = ProjectIcon.Custom;
            SmallIcon = smallIcon;
            LargeIcon = largeIcon;
        }
        
        public ProjectRule(KeyType type, string key, ProjectBackground background)
        {
            Type = type;
            Key = key;
            BackgroundType = background;
            BackgroundTexture = null;
        }

        public ProjectRule(KeyType type, string key, Texture2D background)
        {
            Type = type;
            Key = key;
            IconType = ProjectIcon.Custom;
            BackgroundTexture = background;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void CopyFrom(ProjectRule target)
        {
            Type = target.Type;
            Key = target.Key;

            Ordinal = target.Ordinal;
            Priority = target.Priority;

            IconType = target.IconType;
            SmallIcon = target.SmallIcon;
            LargeIcon = target.LargeIcon;
            IsIconRecursive = target.IsIconRecursive;

            BackgroundType = target.BackgroundType;
            BackgroundTexture = target.BackgroundTexture;
            IsBackgroundRecursive = target.IsBackgroundRecursive;
        }
        
        public bool HasIcon()
        {
            return IconType != ProjectIcon.None && (!HasCustomIcon() || SmallIcon != null || LargeIcon != null);
        }
        
        public bool HasSmallIcon()
        {
            return IconType != ProjectIcon.None && (!HasCustomIcon() || SmallIcon != null);
        }
        
        public bool HasLargeIcon()
        {
            return IconType != ProjectIcon.None && (!HasCustomIcon() || LargeIcon != null);
        }
        
        public bool HasCustomIcon()
        {
            return IconType == ProjectIcon.Custom;
        }
        
        public bool HasBackground()
        {
            return BackgroundType != ProjectBackground.None  && (!HasCustomBackground() || BackgroundTexture != null);
        }
        
        public bool HasCustomBackground()
        {
            return BackgroundType == ProjectBackground.Custom;
        }        

        public bool HasAtLeastOneTexture()
        {
            return HasIcon() || HasBackground();
        }
       
        public bool IsRecursive()
        {
            return IsIconRecursive || IsBackgroundRecursive;
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum KeyType
        {
            Name,
            Path
        }
    }
}
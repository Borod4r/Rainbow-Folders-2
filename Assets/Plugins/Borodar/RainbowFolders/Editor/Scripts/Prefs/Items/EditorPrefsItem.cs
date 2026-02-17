using System;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    public abstract class EditorPrefsItem<T>
    {
        protected readonly string Key;
        protected readonly GUIContent Label;
        protected readonly T DefaultValue;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected EditorPrefsItem(string key, T defaultValue, GUIContent label)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Key = key;
            DefaultValue = defaultValue;
            Label = label;
        }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public abstract T Value { get; set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public abstract void Draw();

        public static implicit operator T(EditorPrefsItem<T> s)
        {
            return s.Value;
        }
    }
}
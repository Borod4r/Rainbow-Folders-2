using UnityEditor;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    public class EditorPrefsString : EditorPrefsItem<string>
    {
        private readonly float _labelWidth;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public EditorPrefsString(string key, string defaultValue, GUIContent label, float labelWidth)
            : base(key, defaultValue, label)
        {
            _labelWidth = labelWidth;
        }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override string Value
        {
            get => EditorPrefs.GetString(Key, DefaultValue);
            set => EditorPrefs.SetString(Key, value);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Draw()
        {
            EditorGUIUtility.labelWidth = _labelWidth;
            Value = EditorGUILayout.TextField(Label, Value);
        }
    }
}
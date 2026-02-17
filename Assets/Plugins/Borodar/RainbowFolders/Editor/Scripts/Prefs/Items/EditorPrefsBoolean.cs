using UnityEditor;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    public class EditorPrefsBoolean : EditorPrefsItem<bool>
    {
        private readonly float _labelWidth;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected EditorPrefsBoolean(string key, bool defaultValue, GUIContent label, float labelWidth)
            : base(key, defaultValue, label)
        {
            _labelWidth = labelWidth;
        }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override bool Value
        {
            get
            {
                return EditorPrefs.GetBool(Key, DefaultValue);
            }
            set
            {
                var isChanged = Value != value;
                EditorPrefs.SetBool(Key, value);
                if (isChanged) OnChange(value);
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Draw()
        {
            EditorGUIUtility.labelWidth = _labelWidth;
            Value = EditorGUILayout.Toggle(Label, Value);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected virtual void OnChange(bool value) { }
    }
}
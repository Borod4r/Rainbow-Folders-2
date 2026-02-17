using UnityEditor;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    public class EditorPrefsStringPopup : EditorPrefsItem<string>
    {
        private float _labelWidth;
        private string[] _options;
        private string[] _optionNames;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public EditorPrefsStringPopup(string key, string defaultValue, GUIContent label, float labelWidth,
            string[] options = null, string[] optionNames = null) : base(key, defaultValue, label)
        {
            _labelWidth = labelWidth;
            _options = options;
            _optionNames = optionNames;
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

        public void UpdateOptions(string[] options, string[] optionNames)
        {
            _options = options;
            _optionNames = optionNames;
        }

        public override void Draw()
        {
            EditorGUIUtility.labelWidth = _labelWidth;

            var selectedIndex = 0;
            for (var i = _options.Length - 1; i >= 0; i--)
            {
                selectedIndex = i;
                if (_options[i] == Value) break;
            }

            selectedIndex = EditorGUILayout.Popup(Label, selectedIndex, _optionNames);
            Value = _options[selectedIndex];
        }
    }
}
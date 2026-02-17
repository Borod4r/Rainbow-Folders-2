using UnityEditor;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    internal class EditorPrefsBooleanRepaint : EditorPrefsBoolean
    {
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public EditorPrefsBooleanRepaint(string key, bool defaultValue, GUIContent label, float labelWidth)
            : base(key, defaultValue, label, labelWidth) { }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnChange(bool value)
        {
            EditorApplication.RepaintProjectWindow();
        }
    }
}
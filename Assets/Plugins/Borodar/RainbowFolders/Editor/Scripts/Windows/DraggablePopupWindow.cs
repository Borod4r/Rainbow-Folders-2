using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    public abstract class DraggablePopupWindow : EditorWindow
    {
        private Vector2 _offset;

        //---------------------------------------------------------------------
        // Static
        //---------------------------------------------------------------------

        /// <summary>
        /// Returns the first DraggablePopupWindow of type T which is currently on the screen.
        /// If there is none, creates and shows new window and returns the instance of it.
        /// </summary>
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        protected static T GetDraggableWindow<T>() where T : DraggablePopupWindow
        {
            var array = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
            var t = (array.Length <= 0) ? null : array[0];

            return t ? t : CreateInstance<T>();
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        /// <summary>
        /// Callback for drawing GUI controls for the popup window.
        /// </summary>
        [SuppressMessage("ReSharper", "InvertIf")]
        protected virtual void OnGUI()
        {
            var e = Event.current;

            // Close the window if ESC is pressed
            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Escape) Close();

            // calculate offset for the mouse cursor when start dragging
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                _offset = position.position - GUIUtility.GUIToScreenPoint(e.mousePosition);
            }

            // drag window
            if (e.button == 0 && e.type == EventType.MouseDrag && IsDragModifierPressed(e))
            {
                var mousePos = GUIUtility.GUIToScreenPoint(e.mousePosition);
                position = new Rect(mousePos + _offset, position.size);
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        /// <summary>
        /// Show draggable editor window with popup-style framing.
        /// </summary>
        protected void Show<T>(Rect inPosition, bool focus = true) where T : DraggablePopupWindow
        {
            minSize = inPosition.size;
            position = inPosition;

            ShowPopup();
            if (focus) Focus();
        }

        protected abstract bool IsDragModifierPressed(Event e);
    }
}
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    [SuppressMessage("ReSharper", "ConvertIfStatementToNullCoalescingExpression")]
    public static class ColorHelper
    {
        public static readonly Color POPUP_BORDER_CLR_FREE = new Color(0.51f, 0.51f, 0.51f);
        public static readonly Color POPUP_BORDER_CLR_PRO = new Color(0.13f, 0.13f, 0.13f);

        public static readonly Color POPUP_BACKGROUND_CLR_FREE = new Color(0.83f, 0.83f, 0.83f);
        public static readonly Color POPUP_BACKGROUND_CLR_PRO = new Color(0.18f, 0.18f, 0.18f);

        public static readonly Color SEPARATOR_CLR_1_FREE = new Color(0.65f, 0.65f, 0.65f, 1f);
        public static readonly Color SEPARATOR_CLR_2_FREE = new Color(0.9f, 0.9f, 0.9f, 1f);
        public static readonly Color SEPARATOR_CLR_1_PRO = new Color(0.13f, 0.13f, 0.13f, 1f);
        public static readonly Color SEPARATOR_CLR_2_PRO = new Color(0.22f, 0.22f, 0.22f, 1f);

        private static readonly Color BG_COLOR_FREE = new Color(0.7607f, 0.7607f, 0.7607f);
        private static readonly Color BG_COLOR_PRO = new Color(0.2196f, 0.2196f, 0.2196f);

        private static readonly Color SELECTION_COLOR_FREE = new Color(0.2275f, 0.4471f, 0.6902f);
        private static readonly Color SELECTION_COLOR_PRO = new Color(0.1725f, 0.3647f, 0.5294f);

        private static readonly Color SELECTION_GRAY_COLOR_FREE = new Color(0.6824f, 0.6824f, 0.6824f);
        private static readonly Color SELECTION_GRAY_COLOR_PRO = new Color(0.302f, 0.302f, 0.302f);

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static Color GetBackgroundColor()
        {
            return EditorGUIUtility.isProSkin ? BG_COLOR_PRO : BG_COLOR_FREE;
        }

        public static Color GetSelectionColor(bool focused)
        {
            if (focused)
            {
                return EditorGUIUtility.isProSkin ? SELECTION_COLOR_PRO : SELECTION_COLOR_FREE;
            }

            return EditorGUIUtility.isProSkin ? SELECTION_GRAY_COLOR_PRO : SELECTION_GRAY_COLOR_FREE;
        }
    }
}
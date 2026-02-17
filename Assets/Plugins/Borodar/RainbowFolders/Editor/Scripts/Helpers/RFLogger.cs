using UnityEngine;

namespace Borodar.RainbowFolders
{
    internal static class RFLogger
    {
        private const string TAG = "<b>[RF]</b>";

        public static void Log(string message)
        {
            Debug.Log($"{TAG} {message}");
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning($"{TAG} {message}");
        }

        public static void LogError(string message)
        {
            Debug.LogError($"{TAG} {message}");
        }
    }
}
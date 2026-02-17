using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Borodar.RainbowFolders
{
    public static class TexturesStorageHelper<T>
    {
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "InvertIf")]
        public static Texture2D GetTexture(T type, FilterMode filterMode, Dictionary<T, Lazy<string>> strings, Dictionary<T, Texture2D> textures)
        {
            if (!textures.TryGetValue(type, out var texture))
            {
                if (strings.TryGetValue(type, out var lazyString))
                {
                    texture = TextureFromString(lazyString.Value, filterMode);
                }
                else
                {
                    texture = Texture2D.grayTexture;
                    RFLogger.LogWarning($"Cannot find texture with ID: {type}");
                }

                textures.Add(type, texture);
            }

            return texture;
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        public static Tuple<Texture2D, Texture2D> GetTextures(T type, FilterMode filterMode, Dictionary<T, Tuple<Lazy<string>, Lazy<string>>> strings, Dictionary<T, Tuple<Texture2D, Texture2D>> textures)
        {
            if (!textures.TryGetValue(type, out var texture))
            {
                if (strings.TryGetValue(type, out var lazyStrings))
                {
                    var texture1 = TextureFromString(lazyStrings.Item1.Value, filterMode);
                    var texture2 = TextureFromString(lazyStrings.Item2.Value, filterMode);
                    texture = Tuple.Create(texture1, texture2);
                }
                else
                {
                    texture = Tuple.Create(Texture2D.grayTexture, Texture2D.grayTexture);
                    RFLogger.LogWarning($"Cannot find texture with ID: {type}");
                }

                textures.Add(type, texture);
            }

            return texture;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        private static Texture2D TextureFromString(string value, FilterMode filterMode)
        {
            var texture = new Texture2D(4, 4, TextureFormat.DXT5, false, false);
            texture.filterMode = filterMode;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.LoadImage(Convert.FromBase64String(value));
            return texture;
        }
    }
}
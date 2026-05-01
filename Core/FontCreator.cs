using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace UnknownCasting.Core.Assets
{
    public static class FontCreator
    {
        public static TMP_FontAsset LoadEmbeddedFontAsset(string resourceName, int size = 48)
        {
            byte[] fontData = LoadFontBytes(resourceName);
            if (fontData == null)
            {
                Debug.LogError($"[FontCreator] Failed to load embedded resource: {resourceName}");
                return null;
            }

            string tempPath = Path.Combine(Application.temporaryCachePath, resourceName + ".ttf");
            File.WriteAllBytes(tempPath, fontData);

            Font font = new Font(tempPath);
            if (font == null)
            {
                Debug.LogError("[FontCreator] Failed to create Font from temp file.");
                return null;
            }

            TMP_FontAsset tmpFont = TMP_FontAsset.CreateFontAsset(
                font,
                size,
                9,
                GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true
            );

            return tmpFont;
        }

        private static byte[] LoadFontBytes(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
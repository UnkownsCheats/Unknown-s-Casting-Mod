using UnityEngine;
using UnityEngine.UI;

namespace UnknownCasting
{
    public class eUI
    {
        public static int RoundedSliderInt(int value, int min, int max, Rect rect, Texture2D backgroundTex, Texture2D fillTex, Texture2D borderTex = null, bool showBorder = false, int radius = 6, int borderSize = 2)
        {
            int range = max - min;
            float percent = Mathf.Clamp01((float)(value - min) / range);

            if (showBorder && borderTex != null)
            {
                DrawTexture(rect, borderTex, radius);
                Rect innerRect = new Rect(rect.x + borderSize, rect.y + borderSize, rect.width - borderSize * 2, rect.height - borderSize * 2);
                DrawTexture(innerRect, backgroundTex, radius - borderSize);
                int fillWidth = Mathf.RoundToInt(innerRect.width * percent);
                Rect fillRect = new Rect(innerRect.x, innerRect.y, fillWidth, innerRect.height);
                DrawTexture(fillRect, fillTex, radius - borderSize);
            }
            else
            {
                DrawTexture(rect, backgroundTex, radius);
                int fillWidth = Mathf.RoundToInt(rect.width * percent);
                Rect fillRect = new Rect(rect.x, rect.y, fillWidth, rect.height);
                DrawTexture(fillRect, fillTex, radius);
            }

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    float pos = (Event.current.mousePosition.x - rect.x) / rect.width;
                    int newValue = Mathf.Clamp(Mathf.RoundToInt(min + range * pos), min, max);
                    value = newValue;
                    Event.current.Use();
                }
            }

            return value;
        }

        public static int TabSliderInt(int value, int min, int max, Rect rect, Texture2D backgroundTex, Texture2D thumbTex, Texture2D borderTex = null, bool showBorder = false, int radius = 6, int borderSize = 2, float thumbWidth = 20f)
        {
            int range = max - min;
            float percent = Mathf.Clamp01((float)(value - min) / range);

            Rect sliderRect = rect;

            if (showBorder && borderTex != null)
            {
                DrawTexture(sliderRect, borderTex, radius);
                Rect innerRect = new Rect(sliderRect.x + borderSize, sliderRect.y + borderSize, sliderRect.width - borderSize * 2, sliderRect.height - borderSize * 2);
                DrawTexture(innerRect, backgroundTex, radius - borderSize);
            }
            else
            {
                DrawTexture(sliderRect, backgroundTex, radius);
            }

            float thumbPos = Mathf.Round(sliderRect.x + (sliderRect.width - thumbWidth) * percent);
            Rect thumbRect = new Rect(thumbPos, sliderRect.y, thumbWidth, sliderRect.height);
            DrawTexture(thumbRect, thumbTex, radius);

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            {
                if (sliderRect.Contains(Event.current.mousePosition))
                {
                    float pos = (Event.current.mousePosition.x - sliderRect.x) / sliderRect.width;
                    int newValue = Mathf.Clamp(Mathf.RoundToInt(min + range * pos), min, max);
                    value = newValue;
                    Event.current.Use();
                }
            }

            return value;
        }

        public static float RoundedSlider(float value, float min, float max, Rect rect, Texture2D backgroundTex, Texture2D fillTex, Texture2D borderTex = null, bool showBorder = false, int radius = 6, int borderSize = 2)
        {
            if (showBorder && borderTex != null)
            {
                DrawTexture(rect, borderTex, radius);
                Rect innerRect = new Rect(rect.x + borderSize, rect.y + borderSize, rect.width - borderSize * 2, rect.height - borderSize * 2);
                DrawTexture(innerRect, backgroundTex, radius - borderSize);
                float percent = Mathf.Clamp01(Mathf.InverseLerp(min, max, value));
                int fillWidth = Mathf.RoundToInt(innerRect.width * percent);
                Rect fillRect = new Rect(innerRect.x, innerRect.y, fillWidth, innerRect.height);
                DrawTexture(fillRect, fillTex, radius - borderSize);
            }
            else
            {
                DrawTexture(rect, backgroundTex, radius);
                float percent = Mathf.Clamp01(Mathf.InverseLerp(min, max, value));
                int fillWidth = Mathf.RoundToInt(rect.width * percent);
                Rect fillRect = new Rect(rect.x, rect.y, fillWidth, rect.height);
                DrawTexture(fillRect, fillTex, radius);
            }

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    float newValue = Mathf.Lerp(min, max, (Event.current.mousePosition.x - rect.x) / rect.width);
                    value = Mathf.Clamp(newValue, min, max);
                    Event.current.Use();
                }
            }

            return value;
        }

        public static float TabSlider(float value, float min, float max, Rect rect, Texture2D backgroundTex, Texture2D thumbTex, Texture2D borderTex = null, bool showBorder = false, int radius = 6, int borderSize = 2, float thumbWidth = 20f)
        {
            Rect sliderRect = rect;

            if (showBorder && borderTex != null)
            {
                DrawTexture(sliderRect, borderTex, radius);
                Rect innerRect = new Rect(sliderRect.x + borderSize, sliderRect.y + borderSize, sliderRect.width - borderSize * 2, sliderRect.height - borderSize * 2);
                DrawTexture(innerRect, backgroundTex, radius - borderSize);
            }
            else
            {
                DrawTexture(sliderRect, backgroundTex, radius);
            }

            float percent = Mathf.Clamp01(Mathf.InverseLerp(min, max, value));
            float thumbPos = Mathf.Round(sliderRect.x + (sliderRect.width - thumbWidth) * percent);
            Rect thumbRect = new Rect(thumbPos, sliderRect.y, thumbWidth, sliderRect.height);
            DrawTexture(thumbRect, thumbTex, radius);

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            {
                if (sliderRect.Contains(Event.current.mousePosition))
                {
                    float newValue = Mathf.Lerp(min, max, (Event.current.mousePosition.x - sliderRect.x) / sliderRect.width);
                    value = Mathf.Clamp(newValue, min, max);
                    Event.current.Use();
                }
            }

            return value;
        }

        public static void DrawDivider(Rect rect, Color col, int thickness = 2, int radius = 4)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            DrawTexture(rect, tex, radius);
            UnityEngine.Object.DestroyImmediate(tex);
        }

        public static void DrawTexture(Rect rect, Texture2D texture, int borderRadius, Vector4 borderRadius4 = default(Vector4))
        {
            if (borderRadius4 == Vector4.zero)
            {
                borderRadius4 = new Vector4(borderRadius, borderRadius, borderRadius, borderRadius);
            }
            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, false, 0f, GUI.color, Vector4.zero, borderRadius4);
        }

        public static bool Button(string text, Rect rect, Texture2D bg, int radius = 8)
        {
            DrawTexture(rect, bg, radius);
            return GUI.Button(rect, text, new GUIStyle(GUI.skin.button)
            {
                font = Dev.Plugin.silkScreen,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15
            });
        }

        public static string TextField(string input, Rect rect, Texture2D bg, int radius = 8)
        {
            DrawTexture(rect, bg, radius);
            return GUI.TextField(rect, input, new GUIStyle(GUI.skin.label)
            {
                font = null,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15
            });
        }

        public static void Box(float width, float height, Texture2D bg, int radius = 8)
        {
            Rect rect = GUILayoutUtility.GetRect(width, height);
            DrawTexture(rect, bg, radius);
        }

        public static void BorderBox(float width, float height, Texture2D bg, Texture2D fill, int radius = 8, int bordersize = 2)
        {
            Rect rect = GUILayoutUtility.GetRect(width, height);
            Rect inner = new Rect(rect.x + bordersize, rect.y + bordersize, rect.width - bordersize * 2, rect.height - bordersize * 2);
            DrawTexture(rect, bg, radius);
            DrawTexture(inner, fill, radius - bordersize);
        }
    }
}
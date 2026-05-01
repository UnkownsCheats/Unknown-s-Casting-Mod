using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

namespace UnknownCasting.Core
{
    public class FPSCounter : MonoBehaviour
    {
        public static FPSCounter Instance;
        
        public enum FPSPosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            TopCenter,
            BottomCenter,
            Custom
        }
        
        public enum FPSStyle
        {
            Simple,
            Detailed,
            Minimal,
            Fancy
        }
        
        private FPSPosition position = FPSPosition.TopLeft;
        private FPSStyle style = FPSStyle.Simple;
        private bool enabled = true;
        private float updateInterval = 0.5f;
        
        private float accum = 0;
        private int frames = 0;
        private float timeleft;
        private float fps = 0;
        
        private GameObject fpsTextObj;
        private TextMeshProUGUI fpsText;
        private RectTransform rectTransform;
        
        private float customX = 10f;
        private float customY = 10f;
        
        private float fontSize = 24f;
        private Color textColor = Color.white;
        private Color backgroundColor = new Color(0, 0, 0, 0.5f);
        
        private bool showBackground = true;
        private bool matchFPSColor = true; // Toggle to enable/disable FPS color match
        
        private Texture2D bgTexture;
        
        void Awake()
        {
            Instance = this;
            CreateFPSText();
            DontDestroyOnLoad(gameObject);
            timeleft = updateInterval;
            Debug.Log("[FPSCounter] Awoke, instance created");
        }
        
        void Start()
        {
            if (fpsTextObj != null)
            {
                fpsTextObj.SetActive(true);
                Debug.Log("[FPSCounter] FPS text should be visible");
            }
        }
        
        private void CreateFPSText()
        {
            // Create at root level
            GameObject canvasObj = new GameObject("FPSCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9998;
            
            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            fpsTextObj = new GameObject("FPSText");
            fpsTextObj.transform.SetParent(canvasObj.transform, false);
            
            rectTransform = fpsTextObj.AddComponent<RectTransform>();
            
            Image bg = null;
            if (showBackground)
            {
                bg = fpsTextObj.AddComponent<Image>();
                bgTexture = CreateRoundedTexture(backgroundColor, 8);
                bg.sprite = Sprite.Create(bgTexture, new Rect(0, 0, bgTexture.width, bgTexture.height), Vector2.one * 0.5f);
                bg.type = Image.Type.Sliced;
            }
            
            fpsText = fpsTextObj.AddComponent<TextMeshProUGUI>();
            fpsText.color = textColor;
            fpsText.fontSize = (int)fontSize;
            fpsText.alignment = TextAlignmentOptions.Midline;
            fpsText.raycastTarget = false;
            
            UpdatePosition();
        }
        
        private Texture2D CreateRoundedTexture(Color color, int radius)
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distX = Mathf.Abs(x - radius);
                    float distY = Mathf.Abs(y - radius);
                    float dist = Mathf.Sqrt(distX * distX + distY * distY);
                    
                    if (x < radius && y < radius)
                    {
                        colors[y * size + x] = (dist > radius) ? Color.clear : color;
                    }
                    else if (x >= size - radius && y < radius)
                    {
                        colors[y * size + x] = (dist > radius) ? Color.clear : color;
                    }
                    else if (x < radius && y >= size - radius)
                    {
                        colors[y * size + x] = (dist > radius) ? Color.clear : color;
                    }
                    else if (x >= size - radius && y >= size - radius)
                    {
                        colors[y * size + x] = (dist > radius) ? Color.clear : color;
                    }
                    else
                    {
                        colors[y * size + x] = color;
                    }
                }
            }
            
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }
        
        void Update()
        {
            if (!enabled) return;
            
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;
            
            if (timeleft <= 0)
            {
                fps = accum / frames;
                timeleft = updateInterval;
                accum = 0;
                frames = 0;
                
                UpdateText();
            }
        }
        
        private void UpdateText()
        {
            if (fpsText == null) 
            {
                // Try to recreate if null
                CreateFPSText();
                return;
            }
            
            string fpsString = "";
            Color fpsColor = GetFPSColor();
            
            switch (style)
            {
                case FPSStyle.Simple:
                    fpsString = $"FPS: {(int)fps}";
                    break;
                case FPSStyle.Detailed:
                    fpsString = $"FPS: {(int)fps}\nFrame: {frames}";
                    break;
                case FPSStyle.Minimal:
                    fpsString = $"{(int)fps}";
                    break;
                case FPSStyle.Fancy:
                    fpsString = $"<color=#{ColorToHex(fpsColor)}>[{fps:F1}]</color>";
                    break;
            }
            
            fpsText.text = fpsString;
            fpsText.color = fpsColor;
        }
        
        private Color GetFPSColor()
        {
            if (!matchFPSColor) return Color.white;
            if (fps >= 72) return Color.green;
            if (fps >= 60) return Color.yellow;
            return Color.red;
        }
        
        private string ColorToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
        
        private void UpdatePosition()
        {
            if (rectTransform == null) return;
            
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            Vector2 anchorMin, anchorMax;
            
            switch (position)
            {
                case FPSPosition.TopLeft:
                    anchorMin = new Vector2(0, 1);
                    anchorMax = new Vector2(0, 1);
                    rectTransform.anchoredPosition = new Vector2(customX, -customY);
                    break;
                case FPSPosition.TopRight:
                    anchorMin = new Vector2(1, 1);
                    anchorMax = new Vector2(1, 1);
                    rectTransform.anchoredPosition = new Vector2(-customX, -customY);
                    break;
                case FPSPosition.BottomLeft:
                    anchorMin = new Vector2(0, 0);
                    anchorMax = new Vector2(0, 0);
                    rectTransform.anchoredPosition = new Vector2(customX, customY);
                    break;
                case FPSPosition.BottomRight:
                    anchorMin = new Vector2(1, 0);
                    anchorMax = new Vector2(1, 0);
                    rectTransform.anchoredPosition = new Vector2(-customX, customY);
                    break;
                case FPSPosition.TopCenter:
                    anchorMin = new Vector2(0.5f, 1);
                    anchorMax = new Vector2(0.5f, 1);
                    rectTransform.anchoredPosition = new Vector2(0, -customY);
                    break;
                case FPSPosition.BottomCenter:
                    anchorMin = new Vector2(0.5f, 0);
                    anchorMax = new Vector2(0.5f, 0);
                    rectTransform.anchoredPosition = new Vector2(0, customY);
                    break;
                case FPSPosition.Custom:
                    anchorMin = new Vector2(0, 1);
                    anchorMax = new Vector2(0, 1);
                    rectTransform.anchoredPosition = new Vector2(customX, -customY);
                    break;
                default:
                    anchorMin = new Vector2(0, 1);
                    anchorMax = new Vector2(0, 1);
                    rectTransform.anchoredPosition = new Vector2(customX, -customY);
                    break;
            }
            
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            
            float width = style == FPSStyle.Detailed ? 120f : 80f;
            float height = style == FPSStyle.Detailed ? 60f : 30f;
            rectTransform.sizeDelta = new Vector2(width, height);
        }
        
        public void SetPosition(FPSPosition pos)
        {
            position = pos;
            UpdatePosition();
        }
        
        public void SetStyle(FPSStyle newStyle)
        {
            style = newStyle;
            UpdatePosition();
            UpdateText();
        }
        
        public void SetEnabled(bool enable)
        {
            enabled = enable;
            Debug.Log($"[FPSCounter] SetEnabled called: {enable}");
            if (fpsTextObj != null)
            {
                fpsTextObj.SetActive(enable);
                Debug.Log($"[FPSCounter] fpsTextObj.SetActive({enable}) called");
            }
        }
        
        public void SetCustomPosition(float x, float y)
        {
            customX = x;
            customY = y;
            if (position == FPSPosition.Custom)
                UpdatePosition();
        }
        
        public void SetFontSize(float size)
        {
            fontSize = size;
            if (fpsText != null)
                fpsText.fontSize = (int)size;
        }
        
        public void SetTextColor(Color color)
        {
            textColor = color;
            if (fpsText != null)
                fpsText.color = color;
        }
        
        public void SetBackgroundEnabled(bool enable)
        {
            showBackground = enable;
            Image bg = fpsTextObj?.GetComponent<Image>();
            if (bg != null)
                bg.enabled = enable;
        }
        
        public void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            Image bg = fpsTextObj?.GetComponent<Image>();
            if (bg != null && bgTexture != null)
            {
                bgTexture = CreateRoundedTexture(color, 8);
                bg.sprite = Sprite.Create(bgTexture, new Rect(0, 0, bgTexture.width, bgTexture.height), Vector2.one * 0.5f);
            }
        }
        
        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Max(0.1f, interval);
        }
        
        public void SetMatchFPSColor(bool enable)
        {
            matchFPSColor = enable;
            UpdateText();
        }
        
        public void ToggleMatchFPSColor()
        {
            matchFPSColor = !matchFPSColor;
            UpdateText();
        }
    }
}
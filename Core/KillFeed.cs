using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

namespace UnknownCasting.Core
{
    public class KillFeed : MonoBehaviour
    {
        public static KillFeed Instance;
        
        private List<KillFeedEntry> entries = new List<KillFeedEntry>();
        private float entryDuration = 5f;
        private float entryHeight = 35f;
        private float entrySpacing = 5f;
        private float entryWidth = 350f;
        private float positionX = 20f;
        private float positionY = 150f;
        private float animSpeed = 8f;
        
        private int maxEntries = 10;
        
        private GameObject feedContainer;
        private RectTransform containerRect;
        
        private Texture2D bgTexture;
        private Font silkScreen;
        
        private bool enabled = true;
        private bool showBackground = true;
        
        private class KillFeedEntry
        {
            public GameObject gameObject;
            public RectTransform rectTransform;
            public CanvasGroup canvasGroup;
            public TextMeshProUGUI killerText;
            public TextMeshProUGUI victimText;
            public float lifetime;
            public bool removing;
            public string killerName;
            public string victimName;
            public Color killerColor;
            public Color victimColor;
        }
        
        private List<KillFeedEntry> activeEntries = new List<KillFeedEntry>();
        
        void Awake()
        {
            Instance = this;
            CreateKillFeedContainer();
            DontDestroyOnLoad(gameObject);
        }
        
        private void CreateKillFeedContainer()
        {
            GameObject canvasObj = new GameObject("KillFeedCanvas");
            canvasObj.transform.SetParent(transform, false);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9997;
            
            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            feedContainer = new GameObject("KillFeedContainer");
            feedContainer.transform.SetParent(canvasObj.transform, false);
            
            containerRect = feedContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(0, 1);
            containerRect.pivot = new Vector2(0, 1);
            containerRect.anchoredPosition = new Vector2(positionX, -positionY);
            containerRect.sizeDelta = new Vector2(entryWidth, 500f);
            
            bgTexture = CreateRoundedTexture(new Color(0.1f, 0.1f, 0.1f, 0.7f), 8);
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
            
            ProcessEntries();
        }
        
        private void ProcessEntries()
        {
            float targetY = 0;
            
            for (int i = activeEntries.Count - 1; i >= 0; i--)
            {
                KillFeedEntry entry = activeEntries[i];
                entry.lifetime += Time.deltaTime;
                
                if (entry.lifetime > entryDuration)
                {
                    entry.removing = true;
                }
                
                float targetX = entry.removing ? -entryWidth - 50f : 0;
                
                Vector2 anchoredPos = Vector2.Lerp(
                    entry.rectTransform.anchoredPosition,
                    new Vector2(targetX, targetY),
                    Time.deltaTime * animSpeed
                );
                entry.rectTransform.anchoredPosition = anchoredPos;
                
                if (entry.removing)
                {
                    entry.canvasGroup.alpha = Mathf.Lerp(entry.canvasGroup.alpha, 0f, Time.deltaTime * animSpeed);
                    
                    if (entry.canvasGroup.alpha < 0.01f)
                    {
                        DestroyEntry(entry);
                        activeEntries.RemoveAt(i);
                        continue;
                    }
                }
                
                targetY += entryHeight + entrySpacing;
            }
        }
        
        public void AddKill(string killerName, string victimName, Color killerColor, Color victimColor)
        {
            if (!enabled) return;
            
            if (activeEntries.Count >= maxEntries)
            {
                KillFeedEntry oldest = activeEntries[0];
                oldest.removing = true;
            }
            
            CreateEntry(killerName, victimName, killerColor, victimColor);
        }
        
        private void CreateEntry(string killerName, string victimName, Color killerColor, Color victimColor)
        {
            GameObject entryObj = new GameObject("KillFeedEntry");
            entryObj.transform.SetParent(feedContainer.transform, false);
            
            RectTransform rect = entryObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(-entryWidth - 50f, 0);
            rect.sizeDelta = new Vector2(entryWidth, entryHeight);
            
            if (showBackground)
            {
                Image bg = entryObj.AddComponent<Image>();
                bg.sprite = Sprite.Create(bgTexture, new Rect(0, 0, bgTexture.width, bgTexture.height), Vector2.one * 0.5f);
                bg.type = Image.Type.Sliced;
            }
            
            CanvasGroup canvasGroup = entryObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            
            GameObject killerObj = new GameObject("KillerText");
            killerObj.transform.SetParent(entryObj.transform, false);
            RectTransform killerRect = killerObj.AddComponent<RectTransform>();
            killerRect.anchorMin = Vector2.zero;
            killerRect.anchorMax = new Vector2(0.45f, 1);
            killerRect.offsetMin = new Vector2(10, 2);
            killerRect.offsetMax = new Vector2(-5, -2);
            
            TextMeshProUGUI killerText = killerObj.AddComponent<TextMeshProUGUI>();
            killerText.text = killerName;
            killerText.color = killerColor;
            killerText.fontSize = 18;
            killerText.fontStyle = FontStyles.Bold;
            killerText.alignment = TextAlignmentOptions.MidlineRight;
            killerText.raycastTarget = false;
            
            GameObject arrowObj = new GameObject("ArrowText");
            arrowObj.transform.SetParent(entryObj.transform, false);
            RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.45f, 0);
            arrowRect.anchorMax = new Vector2(0.55f, 1);
            arrowRect.offsetMin = Vector2.zero;
            arrowRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
            arrowText.text = "→";
            arrowText.color = Color.white;
            arrowText.fontSize = 20;
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.raycastTarget = false;
            
            GameObject victimObj = new GameObject("VictimText");
            victimObj.transform.SetParent(entryObj.transform, false);
            RectTransform victimRect = victimObj.AddComponent<RectTransform>();
            victimRect.anchorMin = new Vector2(0.55f, 0);
            victimRect.anchorMax = Vector2.one;
            victimRect.offsetMin = new Vector2(5, 2);
            victimRect.offsetMax = new Vector2(-10, -2);
            
            TextMeshProUGUI victimText = victimObj.AddComponent<TextMeshProUGUI>();
            victimText.text = victimName;
            victimText.color = victimColor;
            victimText.fontSize = 18;
            victimText.fontStyle = FontStyles.Bold;
            victimText.alignment = TextAlignmentOptions.MidlineLeft;
            victimText.raycastTarget = false;
            
            KillFeedEntry entry = new KillFeedEntry
            {
                gameObject = entryObj,
                rectTransform = rect,
                canvasGroup = canvasGroup,
                killerText = killerText,
                victimText = victimText,
                lifetime = 0f,
                removing = false,
                killerName = killerName,
                victimName = victimName,
                killerColor = killerColor,
                victimColor = victimColor
            };
            
            activeEntries.Add(entry);
            
            canvasGroup.alpha = 1f;
        }
        
        private void DestroyEntry(KillFeedEntry entry)
        {
            if (entry.gameObject != null)
                Destroy(entry.gameObject);
        }
        
        public void SetEnabled(bool enable)
        {
            enabled = enable;
            if (feedContainer != null)
                feedContainer.SetActive(enable);
        }
        
        public void SetPosition(float x, float y)
        {
            positionX = x;
            positionY = y;
            if (containerRect != null)
                containerRect.anchoredPosition = new Vector2(x, -y);
        }
        
        public void SetEntryDuration(float duration)
        {
            entryDuration = duration;
        }
        
        public void SetMaxEntries(int max)
        {
            maxEntries = Mathf.Max(1, max);
        }
        
        public void SetBackgroundEnabled(bool enable)
        {
            showBackground = enable;
            foreach (var entry in activeEntries)
            {
                Image bg = entry.gameObject?.GetComponent<Image>();
                if (bg != null)
                    bg.enabled = enable;
            }
        }
        
        public void ClearFeed()
        {
            for (int i = activeEntries.Count - 1; i >= 0; i--)
            {
                activeEntries[i].removing = true;
            }
        }
    }
}
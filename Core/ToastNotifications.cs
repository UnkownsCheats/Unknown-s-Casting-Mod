using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

namespace UnknownCasting.Core
{
    public class ToastNotification : MonoBehaviour
    {
        public static ToastNotification Instance;
        
        private List<ToastData> toastQueue = new List<ToastData>();
        private List<GameObject> activeToasts = new List<GameObject>();
        private float toastDuration = 3f;
        private float toastSpacing = 60f;
        private float toastHeight = 40f;
        private float toastWidth = 300f;
        private float toastX = 20f;
        private float toastY = 20f;
        private float animSpeed = 8f;
        
        private bool showJoinNotifications = true;
        private bool showLeaveNotifications = true;
        
        private struct ToastData
        {
            public string message;
            public ToastType type;
            public float timestamp;
            public string playerName;
            public Color color;
        }
        
        public enum ToastType
        {
            Join,
            Leave,
            Kill,
            Info
        }
        
        private class ToastInfo
        {
            public GameObject gameObject;
            public RectTransform rectTransform;
            public CanvasGroup canvasGroup;
            public TextMeshProUGUI text;
            public ToastData data;
            public float lifetime;
            public bool removing;
        }
        
        private List<ToastInfo> toastInfos = new List<ToastInfo>();
        
        private Texture2D toastBackground;
        private Font silkScreen;
        
        void Awake()
        {
            Instance = this;
            Debug.Log("[ToastNotification] Awoke, creating canvas");
            gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            var scaler = GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            toastBackground = CreateRoundedTexture(new Color(0.15f, 0.15f, 0.15f, 0.9f), 8);
            
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ToastNotification] Canvas setup complete");
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
                        if (dist > radius) colors[y * size + x] = Color.clear;
                        else colors[y * size + x] = color;
                    }
                    else if (x >= size - radius && y < radius)
                    {
                        if (dist > radius) colors[y * size + x] = Color.clear;
                        else colors[y * size + x] = color;
                    }
                    else if (x < radius && y >= size - radius)
                    {
                        if (dist > radius) colors[y * size + x] = Color.clear;
                        else colors[y * size + x] = color;
                    }
                    else if (x >= size - radius && y >= size - radius)
                    {
                        if (dist > radius) colors[y * size + x] = Color.clear;
                        else colors[y * size + x] = color;
                    }
                    else
                    {
                        colors[y * size + x] = color;
                    }
                }
            }
            
            tex.SetPixels(colors);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }
        
        void Update()
        {
            ProcessToasts();
        }
        
        private void ProcessToasts()
        {
            float targetY = toastY;
            
            for (int i = toastInfos.Count - 1; i >= 0; i--)
            {
                ToastInfo info = toastInfos[i];
                info.lifetime += Time.deltaTime;
                
                if (info.lifetime > toastDuration)
                {
                    info.removing = true;
                }
                
                float targetX = info.removing ? -toastWidth - 50f : toastX;
                
                Vector2 anchoredPos = Vector2.Lerp(
                    info.rectTransform.anchoredPosition,
                    new Vector2(targetX, targetY),
                    Time.deltaTime * animSpeed
                );
                info.rectTransform.anchoredPosition = anchoredPos;
                
                if (info.removing)
                {
                    info.canvasGroup.alpha = Mathf.Lerp(info.canvasGroup.alpha, 0f, Time.deltaTime * animSpeed);
                    
                    if (info.canvasGroup.alpha < 0.01f)
                    {
                        DestroyToast(info);
                        toastInfos.RemoveAt(i);
                        continue;
                    }
                }
                
                targetY += toastHeight + toastSpacing;
            }
            
            while (toastInfos.Count < 5 && toastQueue.Count > 0)
            {
                ToastData data = toastQueue[0];
                toastQueue.RemoveAt(0);
                CreateToast(data);
            }
        }
        
        public void ShowToast(string message, ToastType type, string playerName = "")
        {
            if (type == ToastType.Join && !showJoinNotifications) return;
            if (type == ToastType.Leave && !showLeaveNotifications) return;
            
            Color color = type switch
            {
                ToastType.Join => new Color(0.2f, 0.8f, 0.2f),
                ToastType.Leave => new Color(0.8f, 0.3f, 0.2f),
                ToastType.Kill => new Color(1f, 0.5f, 0f),
                ToastType.Info => new Color(0.3f, 0.5f, 1f),
                _ => Color.white
            };
            
            ToastData data = new ToastData
            {
                message = message,
                type = type,
                timestamp = Time.time,
                playerName = playerName,
                color = color
            };
            
            toastQueue.Add(data);
        }
        
        private void CreateToast(ToastData data)
        {
            GameObject toastObj = new GameObject("Toast_" + data.type);
            toastObj.transform.SetParent(transform, false);
            
            RectTransform rect = toastObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(-toastWidth - 50f, toastY);
            rect.sizeDelta = new Vector2(toastWidth, toastHeight);
            
            Image background = toastObj.AddComponent<Image>();
            background.sprite = Sprite.Create(toastBackground, new Rect(0, 0, toastBackground.width, toastBackground.height), Vector2.one * 0.5f);
            background.type = Image.Type.Sliced;
            background.color = new Color(data.color.r, data.color.g, data.color.b, 0.3f);
            
            CanvasGroup canvasGroup = toastObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            
            GameObject textObj = new GameObject("ToastText");
            textObj.transform.SetParent(toastObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = data.message;
            text.color = data.color;
            text.fontSize = 18;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            
            if (silkScreen != null)
            {
                text.font = null; // Use default font
            }
            
            ToastInfo info = new ToastInfo
            {
                gameObject = toastObj,
                rectTransform = rect,
                canvasGroup = canvasGroup,
                text = text,
                data = data,
                lifetime = 0f,
                removing = false
            };
            
            toastInfos.Add(info);
            
            canvasGroup.alpha = 1f;
        }
        
        private void DestroyToast(ToastInfo info)
        {
            if (info.gameObject != null)
                Destroy(info.gameObject);
        }
        
        public void OnPlayerJoined(string playerName)
        {
            ShowToast(playerName + " joined", ToastType.Join, playerName);
        }
        
        public void OnPlayerLeft(string playerName)
        {
            ShowToast(playerName + " left", ToastType.Leave, playerName);
        }
        
        public void SetShowJoinNotifications(bool show)
        {
            showJoinNotifications = show;
        }
        
        public void SetShowLeaveNotifications(bool show)
        {
            showLeaveNotifications = show;
        }
        
        public void SetToastDuration(float duration)
        {
            toastDuration = duration;
        }
        
        public void SetToastPosition(float x, float y)
        {
            toastX = x;
            toastY = y;
        }
        
        public void SetToastSize(float width, float height)
        {
            toastWidth = width;
            toastHeight = height;
        }
    }
}
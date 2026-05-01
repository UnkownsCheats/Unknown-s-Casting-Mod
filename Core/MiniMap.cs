using UnityEngine;
using UnityEngine.UI;

namespace UnknownCasting.Core
{
    public class MiniMap : MonoBehaviour
    {
        public static MiniMap Instance { get; private set; }

        public static bool ShowMiniMap { get; set; } = false;
        public static float MiniMapSize { get; set; } = 270f;
        public static float MiniMapZoom { get; set; } = 8f;
        public static float MiniMapOpacity { get; set; } = 1f;
        public static Vector2 MiniMapPosition { get; set; } = new Vector2(0f, 0f);
        public static float MiniMapHeight { get; set; } = 15f; // New setting for camera height

        public Camera miniMapCamera;
        public RawImage miniMapDisplay;
        private RenderTexture renderTexture;
        public GameObject miniMapCanvas;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Prevent destruction when scenes change
            InitializeMiniMap();
        }

        void InitializeMiniMap()
        {
            // Create render texture with better settings
            renderTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            renderTexture.antiAliasing = 2;
            renderTexture.Create();

            // Create mini map camera
            GameObject cameraObj = new GameObject("MiniMapCamera");
            miniMapCamera = cameraObj.AddComponent<Camera>();
            DontDestroyOnLoad(cameraObj); // Prevent camera destruction

            // Set camera properties
            miniMapCamera.orthographic = true;
            miniMapCamera.orthographicSize = MiniMapZoom;
            miniMapCamera.targetTexture = renderTexture;
            miniMapCamera.cullingMask = LayerMask.GetMask("Default", "Player", "GorillaTag");
            miniMapCamera.depth = -1;
            miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
            miniMapCamera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
            miniMapCamera.enabled = true; // Keep camera enabled

            // Position camera closer to the player
            UpdateCameraPosition();

            // Create UI canvas for mini map display
            CreateMiniMapUI();
        }

        public void UpdateCameraPosition()
        {
            if (CameraUpdater.cam != null)
            {
                // Position camera closer to the player (reduced from 50 to 15 units)
                Vector3 mainCamPos = CameraUpdater.cam.transform.position;
                miniMapCamera.transform.position = new Vector3(mainCamPos.x, mainCamPos.y + MiniMapHeight, mainCamPos.z);
                miniMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                // Fallback positioning - also closer
                miniMapCamera.transform.position = new Vector3(0f, MiniMapHeight, 0f);
                miniMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }

        void CreateMiniMapUI()
        {
            // Create canvas
            miniMapCanvas = new GameObject("MiniMapCanvas");
            Canvas canvas = miniMapCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // High order to appear on top

            CanvasScaler scaler = miniMapCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            miniMapCanvas.AddComponent<GraphicRaycaster>();

            // Create mini map display
            GameObject displayObj = new GameObject("MiniMapDisplay");
            displayObj.transform.SetParent(miniMapCanvas.transform);
            miniMapDisplay = displayObj.AddComponent<RawImage>();
            miniMapDisplay.texture = renderTexture;

            // Set initial position and size
            UpdateMiniMapDisplay();

            miniMapCanvas.SetActive(ShowMiniMap);
        }

        void Update()
        {
            if (ShowMiniMap && miniMapCamera != null)
            {
                UpdateCameraPosition();

                // Force camera to render every frame
                if (!miniMapCamera.enabled)
                    miniMapCamera.enabled = true;

                // Manual render if needed
                miniMapCamera.Render();
            }
            else if (miniMapCamera != null)
            {
                miniMapCamera.enabled = false;
            }
        }

        void LateUpdate()
        {
            // Additional update in LateUpdate for better synchronization
            if (ShowMiniMap && miniMapCamera != null)
            {
                miniMapCamera.Render();
            }
        }

        public void UpdateMiniMapDisplay()
        {
            if (miniMapDisplay != null)
            {
                // Set size and position
                miniMapDisplay.rectTransform.sizeDelta = new Vector2(MiniMapSize, MiniMapSize);
                miniMapDisplay.rectTransform.anchoredPosition = MiniMapPosition;

                // Set anchors to top-left for consistent positioning
                miniMapDisplay.rectTransform.anchorMin = new Vector2(0, 1);
                miniMapDisplay.rectTransform.anchorMax = new Vector2(0, 1);
                miniMapDisplay.rectTransform.pivot = new Vector2(0, 1);

                // Set opacity
                Color displayColor = miniMapDisplay.color;
                displayColor.a = MiniMapOpacity;
                miniMapDisplay.color = displayColor;

                // Update camera zoom
                if (miniMapCamera != null)
                {
                    miniMapCamera.orthographicSize = MiniMapZoom;
                }
            }

            // Ensure canvas and camera visibility match ShowMiniMap state
            if (miniMapCanvas != null)
                miniMapCanvas.SetActive(ShowMiniMap);
            if (miniMapCamera != null)
                miniMapCamera.enabled = ShowMiniMap;
        }

        public static void SetMiniMapHeight(float height)
        {
            MiniMapHeight = Mathf.Clamp(height, 5f, 50f); // Clamp between 5 and 50 units
            if (Instance != null)
            {
                Instance.UpdateCameraPosition();
            }
        }

        public static void ToggleMiniMap()
        {
            ShowMiniMap = !ShowMiniMap;
            if (Instance != null && Instance.miniMapCanvas != null)
            {
                Instance.miniMapCanvas.SetActive(ShowMiniMap);
                if (Instance.miniMapCamera != null)
                {
                    Instance.miniMapCamera.enabled = ShowMiniMap;
                }
            }
        }

        void OnDestroy()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }

            if (miniMapCanvas != null)
            {
                Destroy(miniMapCanvas);
            }

            if (miniMapCamera != null)
            {
                Destroy(miniMapCamera.gameObject);
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
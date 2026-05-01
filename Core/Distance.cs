using Dev;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnknownCasting.Core
{
    public static class Distance
    {
        private static GameObject distanceDisplay;
        private static TextMeshProUGUI distanceText;
        private static Canvas canvas;
        private static Image backgroundImage;
        private static RectTransform backgroundRect;
        private static RectTransform textRect;

        private static List<VRRig> GetVRRigs()
        {
            var result = new List<VRRig>();
            
            // First add local player
            var localRig = GorillaTagger.Instance?.offlineVRRig;
            if (localRig != null) result.Add(localRig);

            // Then add all networked players using GorillaGameManager
            try
            {
                var manager = GorillaGameManager.instance;
                if (manager != null)
                {
                    foreach (var player in PhotonNetwork.PlayerList)
                    {
                        if (player == PhotonNetwork.LocalPlayer) continue;
                        
                        var vrrig = manager.FindPlayerVRRig(player);
                        if (vrrig != null) result.Add(vrrig);
                    }
                }
            }
            catch { }
            return result;
        }

        // Background customization properties
        public static float backgroundX = 100f;
        public static float backgroundY = 200f;
        public static float backgroundWidth = 1920f; // Half of 1080p width
        public static float backgroundHeight = 300f; // Tall height
        public static float backgroundOpacity = 0.9f;

        // Text customization properties
        public static float textX = 0f; // Relative to background
        public static float textY = 0f; // Relative to background
        public static float textSize = 22f; // Original text size
        public static float textOpacity = 1f;
        public static float textScaleX = 1f; // Text horizontal scale
        public static float textScaleY = 1f; // Text vertical scale

        // Threshold properties
        private static float warningThreshold = 10f;
        private static float dangerThreshold = 5f;

        // Color indication toggle
        public static bool colorIndicationEnabled = false;

        // Update tracking
        private static bool isInitialized = false;

        public static void InitializeDistanceDisplay()
        {
            // Create canvas if it doesn't exist
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("DistanceCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                // Make it persistent
                GameObject.DontDestroyOnLoad(canvasObj);
            }

            // Create display background using embedded image
            if (distanceDisplay == null)
            {
                distanceDisplay = new GameObject("DistanceDisplay");
                distanceDisplay.transform.SetParent(canvas.transform, false);

                // Add image component for the background using embedded image
                backgroundImage = distanceDisplay.AddComponent<Image>();
                backgroundRect = distanceDisplay.GetComponent<RectTransform>();

                // Load and set the embedded image
                Texture2D backgroundTexture = LoadDistanceImage();
                if (backgroundTexture != null)
                {
                    // Create sprite from texture
                    Sprite backgroundSprite = Sprite.Create(backgroundTexture,
                        new Rect(0, 0, backgroundTexture.width, backgroundTexture.height),
                        new Vector2(0.5f, 0.5f));

                    backgroundImage.sprite = backgroundSprite;
                    backgroundImage.type = Image.Type.Sliced;
                    backgroundImage.preserveAspect = false;
                }
                else
                {
                    // Fallback to solid color if embedded image loading fails
                    backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, backgroundOpacity);
                }

                // Set background position and size
                backgroundRect.sizeDelta = new Vector2(backgroundWidth, backgroundHeight);
                backgroundRect.anchorMin = new Vector2(0f, 1f);
                backgroundRect.anchorMax = new Vector2(0f, 1f);
                backgroundRect.pivot = new Vector2(0f, 1f);
                backgroundRect.anchoredPosition = new Vector2(backgroundX, -backgroundY);

                // Add text component
                GameObject textObj = new GameObject("DistanceText");
                textObj.transform.SetParent(distanceDisplay.transform, false);

                distanceText = textObj.AddComponent<TextMeshProUGUI>();
                textRect = textObj.GetComponent<RectTransform>();

                distanceText.alignment = TextAlignmentOptions.Center;
                distanceText.fontSize = textSize;
                distanceText.color = new Color(1f, 1f, 1f, textOpacity);
                distanceText.text = "Lava Distance: Calculating...";
                distanceText.overflowMode = TextOverflowModes.Overflow;
                distanceText.enableAutoSizing = false;
                distanceText.enableWordWrapping = true;

                // Use the same font as the leaderboard
                UpdateFont();

                // Set text position (relative to background)
                textRect.sizeDelta = new Vector2(backgroundWidth - 40f, backgroundHeight - 40f);
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.anchoredPosition = new Vector2(textX, textY);

                // Apply initial text scaling
                textRect.localScale = new Vector3(textScaleX, textScaleY, 1f);
            }

            isInitialized = true;
        }

        public static void EnsureInitialized()
        {
            if (distanceDisplay == null)
            {
                InitializeDistanceDisplay();
            }
        }

        // Method to load embedded image from assembly resources
        private static Texture2D LoadEmbeddedDistanceImage()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string[] possibleResourceNames = {
                    "UnknownCasting.Core.distance.png",
                    "UnknownCasting.distance.png",
                    "distance.png",
                    "Resources.distance.png",
                    assembly.GetName().Name + ".distance.png"
                };

                var resourceNames = assembly.GetManifestResourceNames();
                string actualResourceName = null;

                foreach (string possibleName in possibleResourceNames)
                {
                    actualResourceName = resourceNames.FirstOrDefault(name =>
                        name.Equals(possibleName) || name.EndsWith("." + possibleName));
                    if (actualResourceName != null) break;
                }

                if (actualResourceName != null)
                {
                    using (Stream stream = assembly.GetManifestResourceStream(actualResourceName))
                    {
                        if (stream != null)
                        {
                            byte[] imageData = new byte[stream.Length];
                            stream.Read(imageData, 0, imageData.Length);

                            Texture2D texture = new Texture2D(2, 2);
                            if (texture.LoadImage(imageData))
                            {
                                return texture;
                            }
                        }
                    }
                }
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading embedded distance.png: {e.Message}");
                return null;
            }
        }

        // Method to load image from file system path
        private static Texture2D LoadDistanceImageFromPath()
        {
            try
            {
                string[] possiblePaths = {
                    System.IO.Path.Combine(Application.dataPath, "distance.png"),
                    System.IO.Path.Combine(Application.streamingAssetsPath, "distance.png"),
                    System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "distance.png"),
                    "distance.png"
                };

                foreach (string filePath in possiblePaths)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                        Texture2D texture = new Texture2D(2, 2);
                        if (texture.LoadImage(fileData))
                        {
                            return texture;
                        }
                    }
                }
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading distance.png from path: {e.Message}");
                return null;
            }
        }

        // Combined approach - tries all methods
        private static Texture2D LoadDistanceImage()
        {
            Texture2D texture = LoadEmbeddedDistanceImage();
            if (texture != null) return texture;

            texture = LoadDistanceImageFromPath();
            if (texture != null) return texture;

            return null;
        }

        // Method to update the font to match the leaderboard
        public static void UpdateFont()
        {
            if (distanceText == null) return;

            if (Plugin.silkScreen != null)
            {
                try
                {
                    var fontAsset = TMP_FontAsset.CreateFontAsset(Plugin.silkScreen);
                    if (fontAsset != null)
                    {
                        distanceText.font = fontAsset;
                    }
                }
                catch
                {
                    // Fallback to default font
                }
            }
        }

        // This method should be called every frame to update the display
        public static void UpdateDistanceDisplay()
        {
            if (distanceDisplay == null || distanceText == null) return;

            if (backgroundWidth <= 0 || backgroundHeight <= 0)
            {
                if (distanceDisplay.activeSelf)
                {
                    distanceDisplay.SetActive(false);
                }
                return;
            }
            else
            {
                if (!distanceDisplay.activeSelf)
                {
                    distanceDisplay.SetActive(true);
                }
            }

            float distance = GetDistanceToNearestLavaForSpectatedPlayer();
            string displayText;

            if (distance < 0)
            {
                displayText = "No lava nearby";
                distanceText.color = new Color(1f, 1f, 1f, textOpacity);
            }
            else if (colorIndicationEnabled)
            {
                if (distance < dangerThreshold)
                {
                    displayText = $"{distance:F1} ft";
                    distanceText.color = new Color(1f, 0f, 0f, textOpacity);
                }
                else if (distance < warningThreshold)
                {
                    displayText = $"{distance:F1} ft";
                    distanceText.color = new Color(1f, 1f, 0f, textOpacity);
                }
                else
                {
                    displayText = $"{distance:F1} ft";
                    distanceText.color = new Color(0f, 1f, 0f, textOpacity);
                }
            }
            else
            {
                displayText = $"{distance:F1} ft";
                distanceText.color = new Color(1f, 1f, 1f, textOpacity);
            }

            distanceText.text = $"Lava Distance: {displayText}";
        }

        public static void ToggleColorIndication()
        {
            colorIndicationEnabled = !colorIndicationEnabled;
            UpdateDistanceDisplay();
        }

        public static void ShowDistanceDisplay()
        {
            if (distanceDisplay != null)
            {
                if (backgroundWidth > 0 && backgroundHeight > 0)
                {
                    distanceDisplay.SetActive(true);
                }
            }
            else
            {
                InitializeDistanceDisplay();
            }
        }

        public static void HideDistanceDisplay()
        {
            if (distanceDisplay != null)
            {
                distanceDisplay.SetActive(false);
            }
        }

        public static void ToggleDistanceDisplay()
        {
            if (distanceDisplay != null)
            {
                distanceDisplay.SetActive(!distanceDisplay.activeSelf);
            }
            else
            {
                InitializeDistanceDisplay();
            }
        }

        // Background customization methods
        public static void UpdateBackgroundPosition()
        {
            if (backgroundRect != null)
            {
                backgroundRect.anchoredPosition = new Vector2(backgroundX, -backgroundY);
            }
        }

        public static void UpdateBackgroundSize()
        {
            if (backgroundRect != null)
            {
                backgroundRect.sizeDelta = new Vector2(backgroundWidth, backgroundHeight);

                // Update text size to match new background size
                if (textRect != null)
                {
                    textRect.sizeDelta = new Vector2(Mathf.Max(0, backgroundWidth - 40f), Mathf.Max(0, backgroundHeight - 40f));
                }

                if (backgroundWidth <= 0 || backgroundHeight <= 0)
                {
                    if (distanceDisplay != null)
                        distanceDisplay.SetActive(false);
                }
                else
                {
                    if (distanceDisplay != null)
                        distanceDisplay.SetActive(true);
                }
            }
        }

        public static void UpdateBackgroundOpacity()
        {
            if (backgroundImage != null)
            {
                Color color = backgroundImage.color;
                color.a = backgroundOpacity;
                backgroundImage.color = color;
            }
        }

        // Text customization methods
        public static void UpdateTextPosition()
        {
            if (textRect != null)
            {
                textRect.anchoredPosition = new Vector2(textX, textY);
            }
        }

        public static void UpdateTextSize()
        {
            if (textRect != null)
            {
                textRect.localScale = new Vector3(textScaleX, textScaleY, 1f);
            }

            if (distanceText != null)
            {
                distanceText.fontSize = textSize;

                // Force refresh
                string currentText = distanceText.text;
                distanceText.text = "";
                distanceText.text = currentText;
            }
        }

        public static void UpdateTextOpacity()
        {
            if (distanceText != null)
            {
                Color color = distanceText.color;
                color.a = textOpacity;
                distanceText.color = color;
            }
        }

        public static void ForceRefreshDisplay()
        {
            if (distanceDisplay == null)
            {
                InitializeDistanceDisplay();
                return;
            }

            UpdateBackgroundPosition();
            UpdateBackgroundSize();
            UpdateBackgroundOpacity();
            UpdateTextPosition();
            UpdateTextSize();
            UpdateTextOpacity();
            UpdateDistanceDisplay();
        }

        public static void SetWarningThreshold(float threshold) => warningThreshold = threshold;
        public static void SetDangerThreshold(float threshold) => dangerThreshold = threshold;

        public static float GetDistanceToNearestLavaForSpectatedPlayer()
        {
            if (CameraUpdater.rig == null) return -1f;

            VRRig localPlayerRig = GetLocalPlayerRig();
            if (CameraUpdater.rig == localPlayerRig) return -1f;

            return GetDistanceToNearestLavaInFeet(CameraUpdater.rig.transform.position);
        }

        private static float GetDistanceToNearestLava(Vector3 position)
        {
            float minDistance = float.MaxValue;
            List<VRRig> allRigs = GetAllVRRigs();
            VRRig localPlayerRig = GetLocalPlayerRig();

            foreach (VRRig rig in allRigs)
            {
                if (rig == null || rig.transform == null) continue;
                if (rig == localPlayerRig) continue;

                if (IsLavaPerson(rig))
                {
                    float distance = Vector3.Distance(position, rig.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }

            return minDistance == float.MaxValue ? -1f : minDistance;
        }

        private static float GetDistanceToNearestLavaInFeet(Vector3 position)
        {
            float distanceInMeters = GetDistanceToNearestLava(position);
            if (distanceInMeters < 0) return -1f;
            return distanceInMeters * 3.28084f;
        }

        private static bool IsLavaPerson(VRRig rig)
        {
            if (rig == null) return false;

            VRRig localPlayerRig = GetLocalPlayerRig();
            if (rig == localPlayerRig) return false;

            try
            {
                if (rig.mainSkin != null && rig.mainSkin.material != null)
                {
                    string matName = rig.mainSkin.material.name.ToLower();
                    return matName.Contains("infected") || matName.Contains("lava") || matName.Contains("tagged");
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static List<VRRig> GetAllVRRigs()
        {
            List<VRRig> rigs = new List<VRRig>();

            try
            {
                rigs.AddRange(GetVRRigs());

                if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
                {
                    if (!rigs.Contains(GorillaTagger.Instance.offlineVRRig))
                    {
                        rigs.Add(GorillaTagger.Instance.offlineVRRig);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting VRRigs: {e.Message}");
            }

            return rigs;
        }

        private static VRRig GetLocalPlayerRig()
        {
            try
            {
                return GorillaTagger.Instance?.offlineVRRig;
            }
            catch
            {
                return null;
            }
        }

        public static void DestroyDisplay()
        {
            if (distanceDisplay != null)
            {
                GameObject.Destroy(distanceDisplay);
                distanceDisplay = null;
                distanceText = null;
                backgroundImage = null;
                backgroundRect = null;
                textRect = null;
            }

            if (canvas != null)
            {
                GameObject.Destroy(canvas.gameObject);
                canvas = null;
            }

            isInitialized = false;
        }

        public static void ResetToDefaults()
        {
            backgroundX = 100f;
            backgroundY = 200f;
            backgroundWidth = 1920f;
            backgroundHeight = 300f;
            backgroundOpacity = 0.9f;

            textX = 0f;
            textY = 0f;
            textSize = 22f;
            textOpacity = 1f;
            textScaleX = 1f;
            textScaleY = 1f;

            warningThreshold = 10f;
            dangerThreshold = 5f;
            colorIndicationEnabled = false;

            ForceRefreshDisplay();
        }
    }
}
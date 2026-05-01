using UnknownCasting.Core;
using Dev;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Stats.Core
{
    public static class LeaderBoard
    {
        private static Texture2D darkGrayTexture;
        private static Texture2D greenTexture;
        private static Texture2D modernBoxTexture;
        private static Texture2D whiteTexture;
        private static Texture2D blackTexture;
        private static Texture2D lavaTexture;
        private static Texture2D leaderBoardBackground;
        private static GUIStyle boxStyle;
        private static GUIStyle nameStyle;
        private static GUIStyle localPlayerStyle;
        private static GUIStyle taggedPlayerStyle;
        private static GUIStyle modernBoxStyle;
        private static GUIStyle leftPlayerStyle;

        // Store tagged players
        private static HashSet<VRRig> taggedPlayers = new HashSet<VRRig>();

        // Store left players - keeps them at their position but marks them as left
        private static Dictionary<int, LeftPlayerInfo> leftPlayers = new Dictionary<int, LeftPlayerInfo>();
        private static int leftPlayerCounter = 0;

        // Track current players for detecting who left
        private static Dictionary<int, PlayerInfo> currentPlayerIds = new Dictionary<int, PlayerInfo>();

        private class LeftPlayerInfo
        {
            public string name;
            public Color color;
            public int originalIndex;
            public bool isTagged;
        }

        private class PlayerInfo
        {
            public string name;
            public Color color;
            public int index;
            public bool isTagged;
        }

        // Leaderboard style
        public enum LeaderboardStyle { Classic, Modern }
        public static LeaderboardStyle CurrentStyle = LeaderboardStyle.Modern;

        // Cache for color textures
        private static Dictionary<Color, Texture2D> colorTextureCache = new Dictionary<Color, Texture2D>();

        // Cache for anti-aliased textures
        private static Dictionary<Color, Texture2D> antiAliasedTextureCache = new Dictionary<Color, Texture2D>();

        // Screen safe area tracking
        private static int lastScreenWidth = 0;
        private static int lastScreenHeight = 0;

        // Default player colors for fallback
        private static Color[] defaultColors = new Color[]
        {
            Color.red, Color.blue, Color.green, Color.yellow,
            Color.cyan, Color.magenta, Color.gray, Color.white
        };

        // Customization settings
        public static float leaderboardX = 0f;
        public static float leaderboardY = 645f; // Default Y position
        public static float leaderboardScale = 1.3f;
        public static float textScale = 1.05f;
        public static float leaderboardOpacity = 0.75f;
        public static float modernSpacing = 8f;
        public static float colorBoxSize = 18f;
        public static float angleCutSize = 0.06f; // Half of 0.12f
        public static float imageExposure = 1f;
        public static bool rankNumberInFront = true;

        // Font settings
        public static string[] leaderboardFonts = new string[] { "Default", "Default Bold", "SilkScreen", "Pixel", "GorillaTag", "2P", "DayDream", "Upheavtt", "Designer", "PaytoneOne" };
        public static int leaderboardFontIndex = 1; // Changed to 1 (Default Bold) for Unity default bold font

        // Store loaded fonts
        private static Dictionary<string, Font> loadedFonts = new Dictionary<string, Font>();
        private static bool fontsLoaded = false;

        // Default Y position
        public static float defaultLeaderboardY = 645f;

        public static void Initialize()
        {
            LoadFonts(); // Make sure fonts are loaded
            CreateTextures();
            UpdateStyles();

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        private static void CreateTextures()
        {
            // Darker gray for background boxes
            darkGrayTexture = CreateAntiAliasedAngledTexture(new Color(0.05f, 0.05f, 0.05f, leaderboardOpacity));
            greenTexture = CreateAntiAliasedAngledTexture(new Color(0f, 0.25f, 0f, leaderboardOpacity));
            modernBoxTexture = CreateAntiAliasedAngledTexture(new Color(0.08f, 0.08f, 0.08f, leaderboardOpacity)); // Darker modern boxes
            whiteTexture = CreateSolidTexture(Color.white);
            blackTexture = CreateSolidTexture(Color.black);

            // Load LeaderBoardPart.png from embedded resources
            leaderBoardBackground = LoadEmbeddedTexture("LeaderBoardPart.png");
            if (leaderBoardBackground == null)
            {
                // Fallback to dark gray if image can't be loaded
                leaderBoardBackground = CreateAntiAliasedAngledTexture(new Color(0.05f, 0.05f, 0.05f, leaderboardOpacity));
                Debug.Log("LeaderBoardPart.png not found, using fallback");
            }
            else
            {
                ApplyOpacityAndExposureToTexture(leaderBoardBackground);
            }

            // Load lava texture for tagged players from embedded resources
            lavaTexture = LoadEmbeddedTexture("Lava.png");
            if (lavaTexture == null)
            {
                // Fallback: create an anti-aliased angled orange texture if Lava.png can't be loaded
                lavaTexture = CreateAntiAliasedAngledTexture(new Color(1f, 0.5f, 0f));
                Debug.Log("Lava texture not found, using fallback anti-aliased angled color");
            }
            else
            {
                Debug.Log("Lava texture loaded successfully");
                // Make the lava texture anti-aliased angled by applying it to an anti-aliased mask
                lavaTexture = ApplyAntiAliasedAngleToTexture(lavaTexture);
            }
        }

        private static Texture2D LoadEmbeddedTexture(string textureName)
        {
            try
            {
                // Get the current assembly
                Assembly assembly = Assembly.GetExecutingAssembly();

                // Try to find the embedded resource
                string resourceName = null;
                foreach (string name in assembly.GetManifestResourceNames())
                {
                    if (name.EndsWith(textureName))
                    {
                        resourceName = name;
                        break;
                    }
                }

                if (resourceName != null)
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            byte[] buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);

                            Texture2D texture = new Texture2D(2, 2);
                            if (texture.LoadImage(buffer))
                            {
                                return texture;
                            }
                        }
                    }
                }

                // Alternative: try loading from Resources folder
                Texture2D resourcesTexture = Resources.Load<Texture2D>(textureName.Replace(".png", ""));
                if (resourcesTexture != null)
                {
                    return resourcesTexture;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading embedded texture: {e.Message}");
            }

            return null;
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateAntiAliasedAngledTexture(Color color)
        {
            int width = 512; // Double the pixels (was 256)
            int height = 256; // Double the pixels (was 128)
            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

            // Create a high-resolution texture with anti-aliased angled edges
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float xNorm = (float)x / width;
                    float yNorm = (float)y / height;

                    // Calculate distance from left and right edges with anti-aliasing (half the slant)
                    // Ensure the slant goes all the way from top to bottom
                    float leftEdge = angleCutSize * yNorm;
                    float rightEdge = 1f - angleCutSize * (1f - yNorm);

                    // Calculate alpha based on distance from edges with smooth transition
                    float leftAlpha = SmoothStep(-4f / width, 4f / width, xNorm - leftEdge);
                    float rightAlpha = SmoothStep(-4f / width, 4f / width, rightEdge - xNorm);

                    // Combine both edge alphas
                    float finalAlpha = Mathf.Min(leftAlpha, rightAlpha);

                    if (finalAlpha > 0.01f)
                    {
                        Color finalColor = color;
                        finalColor.a *= finalAlpha;
                        texture.SetPixel(x, y, finalColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            return texture;
        }

        private static Texture2D ApplyAntiAliasedAngleToTexture(Texture2D sourceTexture)
        {
            int width = Mathf.Max(sourceTexture.width * 8, 512); // Double the resolution (was 4x)
            int height = Mathf.Max(sourceTexture.height * 8, 256); // Double the resolution (was 4x)
            Texture2D antiAliasedTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);

            // Create anti-aliased mask and apply to source texture
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float xNorm = (float)x / width;
                    float yNorm = (float)y / height;

                    // Calculate distance from left and right edges with anti-aliasing (half the slant)
                    // Ensure the slant goes all the way from top to bottom
                    float leftEdge = angleCutSize * yNorm;
                    float rightEdge = 1f - angleCutSize * (1f - yNorm);

                    // Calculate alpha based on distance from edges with smooth transition
                    float leftAlpha = SmoothStep(-4f / width, 4f / width, xNorm - leftEdge);
                    float rightAlpha = SmoothStep(-4f / width, 4f / width, rightEdge - xNorm);

                    // Combine both edge alphas
                    float finalAlpha = Mathf.Min(leftAlpha, rightAlpha);

                    if (finalAlpha > 0.01f)
                    {
                        // Sample from source texture with normalized coordinates
                        int srcX = Mathf.RoundToInt(xNorm * (sourceTexture.width - 1));
                        int srcY = Mathf.RoundToInt(yNorm * (sourceTexture.height - 1));
                        srcX = Mathf.Clamp(srcX, 0, sourceTexture.width - 1);
                        srcY = Mathf.Clamp(srcY, 0, sourceTexture.height - 1);

                        Color sampledColor = sourceTexture.GetPixel(srcX, srcY);
                        sampledColor.a *= finalAlpha;
                        antiAliasedTexture.SetPixel(x, y, sampledColor);
                    }
                    else
                    {
                        antiAliasedTexture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            antiAliasedTexture.Apply();
            return antiAliasedTexture;
        }

        private static void ApplyOpacityAndExposureToTexture(Texture2D texture)
        {
            if (texture == null) return;

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color pixel = texture.GetPixel(x, y);
                    pixel.a *= leaderboardOpacity;
                    pixel.r = Mathf.Clamp01(pixel.r * imageExposure);
                    pixel.g = Mathf.Clamp01(pixel.g * imageExposure);
                    pixel.b = Mathf.Clamp01(pixel.b * imageExposure);
                    texture.SetPixel(x, y, pixel);
                }
            }
            texture.Apply();
        }

        public static void RefreshTextureOpacity()
        {
            if (leaderBoardBackground != null)
            {
                ApplyOpacityAndExposureToTexture(leaderBoardBackground);
            }
        }

        // Smooth step function for anti-aliasing
        private static float SmoothStep(float edge0, float edge1, float x)
        {
            x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return x * x * (3f - 2f * x);
        }

        private static Texture2D GetAntiAliasedColorTexture(Color color)
        {
            if (!antiAliasedTextureCache.TryGetValue(color, out Texture2D texture))
            {
                texture = CreateAntiAliasedAngledTexture(color);
                antiAliasedTextureCache[color] = texture;
            }
            return texture;
        }

        private static void UpdateStyles()
        {
            // Update textures with current opacity
            CreateTextures();

            Texture2D backgroundTexture = CurrentStyle == LeaderboardStyle.Modern ? greenTexture : darkGrayTexture;
            Color textColor = Color.white;

            int baseFontSize = CurrentStyle == LeaderboardStyle.Modern ? 18 : 20;
            int scaledFontSize = Mathf.RoundToInt(baseFontSize * textScale);

            Font leaderboardFont = GetLeaderboardFont();

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                font = leaderboardFont,
                fontSize = scaledFontSize,
                alignment = TextAnchor.UpperLeft,
                normal = { background = backgroundTexture, textColor = textColor }
            };

            nameStyle = new GUIStyle(GUI.skin.label)
            {
                font = leaderboardFont,
                fontSize = scaledFontSize,
                normal = { textColor = textColor }
            };

            localPlayerStyle = new GUIStyle(GUI.skin.label)
            {
                font = leaderboardFont,
                fontSize = scaledFontSize,
                normal = { textColor = textColor }
            };

            taggedPlayerStyle = new GUIStyle(GUI.skin.label)
            {
                font = leaderboardFont,
                fontSize = scaledFontSize,
                normal = { textColor = new Color(1f, 0.5f, 0f) }
            };

            modernBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = null }
            };
        }

        public static void ToggleStyle()
        {
            CurrentStyle = CurrentStyle == LeaderboardStyle.Classic ? LeaderboardStyle.Modern : LeaderboardStyle.Classic;
            UpdateStyles();
        }

        public static void PlayerTagged(VRRig taggedRig)
        {
            if (taggedRig != null) taggedPlayers.Add(taggedRig);
        }

        public static void PlayerUntagged(VRRig rig)
        {
            if (rig != null) taggedPlayers.Remove(rig);
        }

        public static void ClearAllTagged()
        {
            taggedPlayers.Clear();
            leftPlayers.Clear();
            currentPlayerIds.Clear();
        }

        public static void SetFont(int fontIndex)
        {
            if (fontIndex >= 0 && fontIndex < leaderboardFonts.Length)
            {
                leaderboardFontIndex = fontIndex;
                UpdateStyles(); // Refresh styles to apply new font
                Debug.Log($"Font changed to: {leaderboardFonts[fontIndex]}");
            }
        }

        public static void SetFont(string fontName)
        {
            int index = Array.IndexOf(leaderboardFonts, fontName);
            if (index >= 0)
            {
                leaderboardFontIndex = index;
                UpdateStyles(); // Refresh styles to apply new font
                Debug.Log($"Font changed to: {leaderboardFonts[index]}");
            }
        }

        public static void NextFont()
        {
            leaderboardFontIndex = (leaderboardFontIndex + 1) % leaderboardFonts.Length;
            UpdateStyles();
            Debug.Log($"Font changed to: {leaderboardFonts[leaderboardFontIndex]}");
        }

        public static void PreviousFont()
        {
            leaderboardFontIndex--;
            if (leaderboardFontIndex < 0) leaderboardFontIndex = leaderboardFonts.Length - 1;
            UpdateStyles();
            Debug.Log($"Font changed to: {leaderboardFonts[leaderboardFontIndex]}");
        }

        public static void RefreshStyles()
        {
            UpdateStyles();
        }

        public static Font GetLeaderboardFont()
        {
            // Ensure fonts are loaded
            if (!fontsLoaded)
            {
                LoadFonts();
            }

            string fontName = leaderboardFonts[leaderboardFontIndex];

            // Check if we have this font loaded
            if (loadedFonts.ContainsKey(fontName) && loadedFonts[fontName] != null)
            {
                return loadedFonts[fontName];
            }

            // Return default font if specific font isn't available
            if (fontName == "Default")
                return null; // Will use GUI.skin.label default font

            if (fontName == "Default Bold")
            {
                // Try to get a bold font
                Font defaultFont = GUI.skin.font;
                if (defaultFont != null) return defaultFont;
                return null;
            }

            // Fallback to silk screen if available
            if (loadedFonts.ContainsKey("SilkScreen") && loadedFonts["SilkScreen"] != null)
                return loadedFonts["SilkScreen"];

            return null; // Use system default
        }

        private static Font GetFontByName(string fontName)
        {
            try
            {
                // First check if we already loaded it
                if (loadedFonts.ContainsKey(fontName) && loadedFonts[fontName] != null)
                    return loadedFonts[fontName];

                // Try to find in Resources
                Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
                foreach (Font f in fonts)
                {
                    if (f.name.Equals(fontName, StringComparison.OrdinalIgnoreCase))
                    {
                        loadedFonts[fontName] = f;
                        return f;
                    }
                }

                // Try to load from embedded resources
                Font embeddedFont = LoadFontFromEmbeddedResource(fontName);
                if (embeddedFont != null)
                {
                    loadedFonts[fontName] = embeddedFont;
                    return embeddedFont;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading font {fontName}: {e.Message}");
            }
            return null;
        }

        private static Font LoadFontFromEmbeddedResource(string fontName)
        {
            try
            {
                // Map font names to resource names
                string resourceName = fontName.ToLower();
                switch (resourceName)
                {
                    case "silkscreen":
                        resourceName = "silkscreen.ttf";
                        break;
                    case "pixel":
                        resourceName = "Minecraft.ttf";
                        break;
                    case "gorillatag":
                        resourceName = "Minecraft.ttf";
                        break;
                    case "2p":
                        resourceName = "PressStart2P.ttf";
                        break;
                    case "daydream":
                        resourceName = "Daydream.ttf";
                        break;
                    case "upheavtt":
                        resourceName = "upheavtt.ttf";
                        break;
                    case "designer":
                        resourceName = "designer.otf";
                        break;
                    case "paytoneone":
                        resourceName = "PaytoneOne.ttf";
                        break;
                    default:
                        return null;
                }

                var assembly = Assembly.GetExecutingAssembly();
                string fullResourceName = null;

                // Find the resource
                foreach (string name in assembly.GetManifestResourceNames())
                {
                    if (name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase))
                    {
                        fullResourceName = name;
                        break;
                    }
                }

                if (fullResourceName != null)
                {
                    using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
                    {
                        if (stream != null)
                        {
                            byte[] data = new byte[stream.Length];
                            stream.Read(data, 0, data.Length);

                            // Create a temporary game object to load the font
                            Font font = new Font(fontName);
                            font.name = fontName;

                            // For TTF/OTF files, we need to use a different approach
                            // This is a simplified version - you might need to use Font.CreateDynamicFontFromOSFont
                            // or other methods depending on your Unity version
                            return font;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading font from embedded resource: {e.Message}");
            }
            return null;
        }

        public static void LoadFonts()
        {
            if (fontsLoaded) return;

            try
            {
                // Try to load each font
                foreach (string fontName in leaderboardFonts)
                {
                    if (fontName == "Default" || fontName == "Default Bold")
                        continue;

                    Font font = GetFontByName(fontName);
                    if (font != null && !loadedFonts.ContainsKey(fontName))
                    {
                        loadedFonts[fontName] = font;
                        Debug.Log($"Loaded font: {fontName}");
                    }
                }

                fontsLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading fonts: {e.Message}");
            }
        }

        public static void DrawLeaderboard(List<VRRig> rigs)
        {
            if (rigs == null) return;

            // Detect who left the lobby
            DetectLeftPlayers(rigs);

            // Reinitialize if screen size changed significantly or settings changed
            if (Mathf.Abs(Screen.width - lastScreenWidth) > 50 || Mathf.Abs(Screen.height - lastScreenHeight) > 50)
            {
                Cleanup();
                Initialize();
            }

            if (darkGrayTexture == null || greenTexture == null || modernBoxTexture == null ||
                boxStyle == null || nameStyle == null || localPlayerStyle == null ||
                taggedPlayerStyle == null || modernBoxStyle == null || whiteTexture == null || blackTexture == null ||
                lavaTexture == null || leaderBoardBackground == null)
            {
                Initialize();
            }

            // Apply current font
            Font currentFont = GetLeaderboardFont();
            string currentFontName = leaderboardFonts[leaderboardFontIndex];
            bool isBold = currentFontName == "Default Bold";

            if (nameStyle != null)
            {
                nameStyle.font = currentFont;
                if (isBold) nameStyle.fontStyle = FontStyle.Bold;
                else nameStyle.fontStyle = FontStyle.Normal;
            }
            if (localPlayerStyle != null)
            {
                localPlayerStyle.font = currentFont;
                if (isBold) localPlayerStyle.fontStyle = FontStyle.Bold;
                else localPlayerStyle.fontStyle = FontStyle.Normal;
            }
            if (taggedPlayerStyle != null)
            {
                taggedPlayerStyle.font = currentFont;
                if (isBold) taggedPlayerStyle.fontStyle = FontStyle.Bold;
                else taggedPlayerStyle.fontStyle = FontStyle.Normal;
            }
            if (boxStyle != null)
            {
                boxStyle.font = currentFont;
                if (isBold) boxStyle.fontStyle = FontStyle.Bold;
                else boxStyle.fontStyle = FontStyle.Normal;
            }

            if (CurrentStyle == LeaderboardStyle.Classic)
            {
                DrawClassicLeaderboard(rigs);
            }
            else
            {
                DrawModernLeaderboard(rigs);
            }
        }

        private static void DetectLeftPlayers(List<VRRig> rigs)
        {
            // Build map of current player IDs to their info
            Dictionary<int, PlayerInfo> currentPlayers = new Dictionary<int, PlayerInfo>();

            if (rigs != null)
            {
                int index = 0;
                foreach (VRRig rig in rigs)
                {
                    if (rig == null || string.IsNullOrWhiteSpace(rig.playerText1?.text)) continue;

                    int playerId = GetPlayerId(rig);
                    currentPlayers[playerId] = new PlayerInfo
                    {
                        name = rig.playerText1.text.ToUpper(),
                        color = rig.playerColor,
                        index = index,
                        isTagged = taggedPlayers.Contains(rig)
                    };
                    index++;
                }
            }

            // Find players who left (compare with previous frame's currentPlayers)
            foreach (var kvp in currentPlayerIds)
            {
                int playerId = kvp.Key;
                PlayerInfo playerInfo = kvp.Value;
                if (!currentPlayers.ContainsKey(playerId) && !leftPlayers.ContainsKey(playerId))
                {
                    // This player just left - store their info
                    leftPlayers[playerId] = new LeftPlayerInfo
                    {
                        name = playerInfo.name,
                        color = playerInfo.color,
                        originalIndex = playerInfo.index,
                        isTagged = playerInfo.isTagged
                    };
                }
            }

            // Update current player IDs for next frame
            currentPlayerIds = currentPlayers;
        }

        private static int GetPlayerId(VRRig rig)
        {
            try
            {
                if (rig.Creator != null)
                {
                    return rig.Creator.GetHashCode();
                }
            }
            catch { }
            return rig.GetHashCode();
        }

        private static bool IsLocalPlayer(VRRig rig)
        {
            if (rig == null) return false;

            try
            {
                // Check by PhotonPlayer - most reliable method
                if (rig.Creator != null && PhotonNetwork.LocalPlayer != null)
                {
                    if (rig.Creator.Equals(PhotonNetwork.LocalPlayer))
                        return true;
                }

                // Fallback: check instance reference
                var localRig = GorillaTagger.Instance?.offlineVRRig;
                if (localRig != null && rig == localRig) return true;
            }
            catch
            { }

            return false;
        }

        public static void Cleanup()
        {
            try
            {
                if (darkGrayTexture != null) Object.Destroy(darkGrayTexture);
                if (greenTexture != null) Object.Destroy(greenTexture);
                if (modernBoxTexture != null) Object.Destroy(modernBoxTexture);
                if (whiteTexture != null) Object.Destroy(whiteTexture);
                if (blackTexture != null) Object.Destroy(blackTexture);

                // Only destroy lava texture if it's not an embedded resource
                if (lavaTexture != null && lavaTexture.name != "Lava.png")
                {
                    Object.Destroy(lavaTexture);
                }

                foreach (var kvp in colorTextureCache)
                {
                    if (kvp.Value != null) Object.Destroy(kvp.Value);
                }
                colorTextureCache.Clear();

                foreach (var kvp in antiAliasedTextureCache)
                {
                    if (kvp.Value != null) Object.Destroy(kvp.Value);
                }
                antiAliasedTextureCache.Clear();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during cleanup: {e.Message}");
            }
        }

        private static void DrawClassicLeaderboard(List<VRRig> rigs)
        {
            float scaledWidth = 300f * leaderboardScale;
            float scaledHeight = 330f * leaderboardScale;
            Rect mainBoxRect = new Rect(leaderboardX, leaderboardY, scaledWidth, scaledHeight);
            GUI.Box(mainBoxRect, "Leaderboard", boxStyle);

            float scaledColorBoxSize = colorBoxSize * leaderboardScale;
            float colorBoxPadding = 5f * leaderboardScale;
            float startX = mainBoxRect.x + 50f * leaderboardScale;
            float startY = mainBoxRect.y + 40f * leaderboardScale;
            float nameHeight = 25f * leaderboardScale;
            float boxSpacing = 5f * leaderboardScale;

            DrawPlayerList(rigs, startX, startY, nameHeight, boxSpacing, scaledColorBoxSize, colorBoxPadding, mainBoxRect.width);
        }

        private static void DrawModernLeaderboard(List<VRRig> rigs)
        {
            float scaledColorBoxSize = colorBoxSize * leaderboardScale;
            float colorBoxPadding = 5f * leaderboardScale;
            float nameHeight = 25f * leaderboardScale;
            float boxSpacing = modernSpacing * leaderboardScale;
            float boxWidth = 280f * leaderboardScale;

            int visiblePlayerCount = 0;
            if (rigs != null)
            {
                foreach (VRRig rig in rigs)
                {
                    if (rig != null && !string.IsNullOrWhiteSpace(rig.playerText1?.text))
                    {
                        visiblePlayerCount++;
                        if (visiblePlayerCount >= 10) break;
                    }
                }
            }

            VRRig localRig = GetLocalRig();
            if (localRig != null && !string.IsNullOrWhiteSpace(localRig.playerText1?.text))
            {
                bool isLocalInList = false;
                if (rigs != null)
                {
                    foreach (VRRig rig in rigs)
                    {
                        if (rig == localRig)
                        {
                            isLocalInList = true;
                            break;
                        }
                    }
                }
                if (!isLocalInList)
                {
                    visiblePlayerCount++;
                }
            }

            float totalHeight = visiblePlayerCount * (nameHeight + boxSpacing);
            float startY = leaderboardY;
            float startX = leaderboardX + 35f;

            DrawPlayerList(rigs, startX, startY, nameHeight, boxSpacing, scaledColorBoxSize, colorBoxPadding, 280f * leaderboardScale);
        }

        private static VRRig GetLocalRig()
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

        private static void DrawPlayerList(List<VRRig> rigs, float startX, float startY, float nameHeight, float boxSpacing, float colorBoxSize, float colorBoxPadding, float boxWidth)
        {
            int displayCount = 0;
            int actualRank = 0; // Start at 0 (top position)
            VRRig localRig = GetLocalRig();

            // Don't show left players - they are removed from leaderboard
            leftPlayers.Clear();

            // Count total players first
            int totalPlayers = 0;
            if (rigs != null)
            {
                foreach (VRRig rig in rigs)
                {
                    if (rig == null || string.IsNullOrWhiteSpace(rig.playerText1?.text)) continue;
                    totalPlayers++;
                }
            }

            // Check if local player is in the list
            bool localInList = false;
            if (rigs != null)
            {
                foreach (VRRig rig in rigs)
                {
                    if (IsLocalPlayer(rig))
                    {
                        localInList = true;
                        break;
                    }
                }
            }

            // Calculate starting Y position (bottom of leaderboard)
            // 10 slots max, each slot is nameHeight + boxSpacing
            int maxSlots = 10;
            float listHeight = maxSlots * (nameHeight + boxSpacing);
            float currentY = startY + listHeight - (nameHeight + boxSpacing); // Start at bottom

            if (rigs != null)
            {
                foreach (VRRig rig in rigs)
                {
                    if (rig == null || string.IsNullOrWhiteSpace(rig.playerText1?.text)) continue;
                    if (displayCount >= maxSlots) break;

                    string playerName = rig.playerText1.text.ToUpper();
                    float posY = currentY - displayCount * (nameHeight + boxSpacing);

                    if (posY < -nameHeight || posY > Screen.height + nameHeight)
                    {
                        displayCount++;
                        continue;
                    }

                    string displayRank = actualRank.ToString();

                    bool isLocalPlayer = IsLocalPlayer(rig);
                    bool isTagged = taggedPlayers.Contains(rig);

                    // Draw individual background for each player in modern style
                    if (CurrentStyle == LeaderboardStyle.Modern && leaderBoardBackground != null)
                    {
                        Rect bgRect = new Rect(startX - 5f * leaderboardScale, posY - 2.0f * leaderboardScale, boxWidth + 40f * leaderboardScale, nameHeight + 8f * leaderboardScale);
                        GUI.DrawTexture(bgRect, leaderBoardBackground, ScaleMode.StretchToFill);
                    }

                    if (CurrentStyle == LeaderboardStyle.Modern)
                    {
                        Rect playerBoxRect = new Rect(startX - 8f * leaderboardScale, posY - 2f * leaderboardScale, boxWidth, nameHeight + 4f * leaderboardScale);
                        GUI.Box(playerBoxRect, "", modernBoxStyle);
                    }

                    if (rankNumberInFront)
                    {
                        DrawPlayerColorBox(startX, posY + 2f * leaderboardScale, colorBoxSize, nameHeight, rig, displayCount, isTagged);
                    }

                    GUIStyle styleToUse;
                    if (isTagged)
                        styleToUse = taggedPlayerStyle;
                    else if (isLocalPlayer)
                        styleToUse = localPlayerStyle;
                    else
                        styleToUse = nameStyle;

                    float fontOffset = leaderboardFonts[leaderboardFontIndex] != "SilkScreen" ? 4f * leaderboardScale : 0f;

                    if (rankNumberInFront)
                    {
                        float textX = startX + colorBoxSize + colorBoxPadding + 10f; // Moved right more
                        GUI.Label(new Rect(textX, posY + 2.0f * leaderboardScale + fontOffset, boxWidth - 20f * leaderboardScale - colorBoxSize - colorBoxPadding, nameHeight),
                                 displayRank + "   " + playerName, styleToUse); // Added extra space
                    }
                    else
                    {
                        float rankWidth = 30f * leaderboardScale;
                        GUI.Label(new Rect(startX + 5f, posY + 1.0f * leaderboardScale + fontOffset, rankWidth, nameHeight), displayRank, styleToUse); // Moved right
                        DrawPlayerColorBox(startX + rankWidth - 5f * leaderboardScale, posY + 2f * leaderboardScale, colorBoxSize, nameHeight, rig, displayCount, isTagged);
                        float textX = startX + rankWidth + 15f * leaderboardScale; // Moved right
                        float textWidth = boxWidth - colorBoxSize - rankWidth;
                        GUI.Label(new Rect(textX, posY + 1.0f * leaderboardScale + fontOffset, textWidth, nameHeight), playerName, styleToUse);
                    }

                    displayCount++;
                    actualRank++;
                }
            }

            // Handle local player if not in list - add at the end (higher number)
            if (!localInList && localRig != null && !string.IsNullOrWhiteSpace(localRig.playerText1?.text))
            {
                if (displayCount < maxSlots)
                {
                    string localPlayerName = localRig.playerText1.text.ToUpper();
                    string localDisplayRank = displayCount.ToString();
                    float posY = currentY - displayCount * (nameHeight + boxSpacing);

                    if (posY >= -nameHeight && posY <= Screen.height + nameHeight)
                    {
                        bool isLocalTagged = taggedPlayers.Contains(localRig);

                        // Draw individual background for local player in modern style
                        if (CurrentStyle == LeaderboardStyle.Modern && leaderBoardBackground != null)
                        {
                            Rect bgRect = new Rect(startX - 5f * leaderboardScale, posY - 2.0f * leaderboardScale, boxWidth + 40f * leaderboardScale, nameHeight + 8f * leaderboardScale);
                            GUI.DrawTexture(bgRect, leaderBoardBackground, ScaleMode.StretchToFill);
                        }

                        if (CurrentStyle == LeaderboardStyle.Modern)
                        {
                            Rect playerBoxRect = new Rect(startX - 8f * leaderboardScale, posY - 2f * leaderboardScale, boxWidth, nameHeight + 4f * leaderboardScale);
                            GUI.Box(playerBoxRect, "", modernBoxStyle);
                        }

                        if (rankNumberInFront)
                        {
                            DrawPlayerColorBox(startX, posY + 2f * leaderboardScale, colorBoxSize, nameHeight, localRig, displayCount, isLocalTagged);
                        }

                        GUIStyle localStyle = isLocalTagged ? taggedPlayerStyle : localPlayerStyle;

                        float fontOffset = leaderboardFonts[leaderboardFontIndex] != "SilkScreen" ? 4f * leaderboardScale : 0f;

                        if (rankNumberInFront)
                        {
                            GUI.Label(new Rect(startX + colorBoxSize + colorBoxPadding, posY + 2.0f * leaderboardScale + fontOffset, boxWidth - 20f * leaderboardScale - colorBoxSize - colorBoxPadding, nameHeight),
                                     localDisplayRank + " " + localPlayerName, localStyle);
                        }
                        else
                        {
                            float rankWidth = 30f * leaderboardScale;
                            GUI.Label(new Rect(startX + 5f * leaderboardScale, posY + 1.0f * leaderboardScale + fontOffset, rankWidth, nameHeight), localDisplayRank, localStyle);
                            DrawPlayerColorBox(startX + rankWidth - 10f * leaderboardScale, posY + 2f * leaderboardScale, colorBoxSize, nameHeight, localRig, displayCount, isLocalTagged);
                            float textX = startX + rankWidth + 10f * leaderboardScale;
                            float textWidth = boxWidth - colorBoxSize - rankWidth;
                            GUI.Label(new Rect(textX, posY + 1.0f * leaderboardScale + fontOffset, textWidth, nameHeight), localPlayerName, localStyle);
                        }
                    }
                }
            }
        }

        private static void DrawPlayerColorBox(float startX, float startY, float boxSize, float nameHeight, VRRig rig, int displayCount, bool isTagged)
        {
            Color playerColor;

            if (isTagged && lavaTexture != null)
            {
                playerColor = new Color(1f, 0.5f, 0f);
            }
            else
            {
                playerColor = rig?.playerColor ?? defaultColors[displayCount % defaultColors.Length];
            }

            Texture2D colorTexture = GetAntiAliasedColorTexture(playerColor);
            float centeredY = startY - (boxSize / 2f) + (nameHeight / 2f);
            Rect colorRect = new Rect(startX, centeredY, boxSize, boxSize);
            GUI.DrawTexture(colorRect, colorTexture);
        }
    }
}
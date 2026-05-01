using UnknownCasting;
using UnknownCasting.Core.Assets;
using BepInEx;
using Dev;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Stats.Core;
using TMPro;
using UnityEngine;

namespace UnknownCasting.Core
{
    public static class NameTags
    {
        // Outline settings
        public static float outlineWidth = 0.2f; // Outline thickness (smaller value works better)
        public static Color outlineColor = Color.black;
        public static bool matchPlayerColor = true; // ON by default
        public static bool showName = true; // ON by default
        public static bool autoOutlineWidth = true; // Auto-adjust outline width based on tag size

        // Custom colors when matchPlayerColor is OFF
        public static Color customNameColor = Color.white;
        public static Color customFPSColor = Color.white;
        public static Color customPlatformColor = Color.white;
        public static Color customSpeedColor = Color.white;

        // FPS indicator colors
        public static Color fpsGreen = Color.green; // 72+ FPS
        public static Color fpsYellow = Color.yellow; // 60-71 FPS
        public static Color fpsRed = Color.red; // <60 FPS
        public static bool useFPSIndicator = true; // Use color indicator for FPS

        // Additional tag toggles
        public static bool showFPSTags = true; // ON by default
        public static bool showPlatformTags = true; // ON by default
        public static bool showSpeedTags = true; // ON by default
        public static bool showPingTags = true; // ON by default

        // Ping tag settings
        public static float pingTagOffset = 0.5f;
        public static float pingTagSize = 0.6f;
        public static Color pingColor = Color.cyan;

        // Match color toggles per tag type
        public static bool matchFPSColor = true; // Use FPS indicator or player color
        public static bool matchPlatformColor = true;
        public static bool matchSpeedColor = true;

        // Platform checker settings
        public static float platformTagOffset = 0.2f;
        public static float platformTagSize = 0.8f;

        // FPS tag settings
        public static float fpsTagOffset = 0.10f;
        public static float fpsTagSize = 0.60f;

        // Speed tag settings
        public static float speedTagOffset = 0.4f;
        public static float speedTagSize = 0.8f;
        
        // Background settings - DISABLED
        public static bool showBackground = false;
        public static float backgroundOpacity = 0.5f;
        
        // Animation settings - DISABLED
        public static bool enablePulse = false;
        
        // private static Dictionary<VRRig, float> tagAnimationPhases = new Dictionary<VRRig, float>();

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

        private static Dictionary<VRRig, TextMeshPro> nameTagMap = new Dictionary<VRRig, TextMeshPro>();
        private static Dictionary<VRRig, TextMeshPro> fpsTagMap = new Dictionary<VRRig, TextMeshPro>();
        private static Dictionary<VRRig, TextMeshPro> platformTagMap = new Dictionary<VRRig, TextMeshPro>();
        private static Dictionary<VRRig, TextMeshPro> speedTagMap = new Dictionary<VRRig, TextMeshPro>();
        private static Dictionary<VRRig, TextMeshPro> pingTagMap = new Dictionary<VRRig, TextMeshPro>();
        private static Dictionary<VRRig, Renderer> speakingTagMap = new Dictionary<VRRig, Renderer>();
        private static TMP_FontAsset embeddedFont;
        private static bool fontLoaded = false;
        private static bool fontChanged = false;

        private static string currentFontKey = "";
        public static string font = "Designer";

        public static Dictionary<string, TMP_FontAsset> customFonts = new Dictionary<string, TMP_FontAsset>();
        private static HashSet<VRRig> taggedPlayers = new HashSet<VRRig>();
        private static Dictionary<VRRig, Vector3> previousPositions = new Dictionary<VRRig, Vector3>();
        private static Dictionary<VRRig, float> playerSpeeds = new Dictionary<VRRig, float>();

        public static string antiCheatFont = "Default Bold";
        public static int antiCheatFontIndex = 1;
        public static string[] availableAntiCheatFonts = new string[] { "Default", "Default Bold", "SilkScreen", "Pixel", "GorillaTag", "2P", "DayDream", "Upheavtt", "Designer", "PaytoneOne" };

        public static void SetAntiCheatFont(string newFont)
        {
            if (antiCheatFont != newFont)
            {
                antiCheatFont = newFont;
                antiCheatFontIndex = Array.IndexOf(availableAntiCheatFonts, newFont);
                if (antiCheatFontIndex < 0) antiCheatFontIndex = 1;
            }
        }

        private static void SetupAntiCheatTextMesh(TextMeshPro tmp, float fontSize)
        {
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;

            ApplyOutline(tmp);
        }

        public static void SetFont(string newFont)
        {
            if (font != newFont)
            {
                font = newFont;
                fontChanged = true;
            }
        }

        private static readonly Dictionary<string, string> FontMap = new Dictionary<string, string>
        {
            { "Pixel", "TTF" },
            { "GorillaTag", "Minecraft.ttf" },
            { "2P", "PressStart2P.ttf" },
            { "DayDream", "Daydream.ttf" },
            { "Upheavtt", "upheavtt.ttf" },
            { "Designer", "designer.otf" }
        };

        public static void PlayerTagged(VRRig taggedRig)
        {
            if (taggedRig != null)
            {
                taggedPlayers.Add(taggedRig);
                UpdateTagColor(taggedRig);
            }
        }

        public static void PlayerUntagged(VRRig rig)
        {
            if (rig != null)
            {
                taggedPlayers.Remove(rig);
                UpdateTagColor(rig);
            }
        }

        public static void ClearAllTagged()
        {
            var previousTagged = new List<VRRig>(taggedPlayers);
            taggedPlayers.Clear();

            foreach (var rig in previousTagged)
            {
                if (rig != null)
                    UpdateTagColor(rig);
            }
        }

        private static void UpdateTagColor(VRRig rig)
        {
            if (nameTagMap.TryGetValue(rig, out var nameTag) && nameTag != null)
            {
                nameTag.color = GetTagColor(rig, Color.white);
                ApplyOutline(nameTag); // Re-apply outline when color changes
            }

            if (fpsTagMap.TryGetValue(rig, out var fpsTag) && fpsTag != null)
            {
                fpsTag.color = GetTagColor(rig, Color.white);
                ApplyOutline(fpsTag); // Re-apply outline when color changes
            }

            if (platformTagMap.TryGetValue(rig, out var platformTag) && platformTag != null)
            {
                platformTag.color = GetTagColor(rig, Color.yellow);
                ApplyOutline(platformTag); // Re-apply outline when color changes
            }

            if (speedTagMap.TryGetValue(rig, out var speedTag) && speedTag != null)
            {
                speedTag.color = GetTagColor(rig, Color.cyan);
                ApplyOutline(speedTag); // Re-apply outline when color changes
            }
        }

        public static void LoadCustomFonts()
        {
            customFonts.Clear();

            string customFontsPath = Path.Combine(Paths.ConfigPath, "UnknownCastingFonts");

            if (!Directory.Exists(customFontsPath))
            {
                Directory.CreateDirectory(customFontsPath);
                return;
            }

            string[] fontFiles = Directory.GetFiles(customFontsPath, "*.ttf");
            string[] otfFiles = Directory.GetFiles(customFontsPath, "*.otf");

            string[] allFontFiles = new string[fontFiles.Length + otfFiles.Length];
            fontFiles.CopyTo(allFontFiles, 0);
            otfFiles.CopyTo(allFontFiles, fontFiles.Length);

            foreach (string fontPath in allFontFiles)
            {
                try
                {
                    byte[] fontData = File.ReadAllBytes(fontPath);
                    Font unityFont = new Font(fontPath);
                    TMP_FontAsset tmpFont = TMP_FontAsset.CreateFontAsset(unityFont);

                    if (tmpFont != null)
                    {
                        string fontName = Path.GetFileNameWithoutExtension(fontPath);
                        customFonts.Add(fontName, tmpFont);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load custom font {fontPath}: {e.Message}");
                }
            }
        }

        public static void DestroyAllNameTags()
        {
            DestroyTagMap(nameTagMap);
            DestroyTagMap(fpsTagMap);
            DestroyTagMap(platformTagMap);
            DestroyTagMap(speedTagMap);
            DestroyRendererTagMap(speakingTagMap);

            nameTagMap.Clear();
            fpsTagMap.Clear();
            platformTagMap.Clear();
            speedTagMap.Clear();
            speakingTagMap.Clear();
            taggedPlayers.Clear();
            previousPositions.Clear();
            playerSpeeds.Clear();
        }

        private static void DestroyTagMap(Dictionary<VRRig, TextMeshPro> tagMap)
        {
            foreach (var tag in tagMap.Values)
            {
                if (tag != null)
                    UnityEngine.Object.Destroy(tag.gameObject);
            }
        }

        private static void DestroyRendererTagMap(Dictionary<VRRig, Renderer> dict)
        {
            List<VRRig> toRemove = new List<VRRig>();
            foreach (var kvp in dict)
            {
                if (kvp.Key == null || kvp.Key.transform == null)
                {
                    if (kvp.Value != null)
                        UnityEngine.Object.Destroy(kvp.Value.gameObject);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var rig in toRemove)
                dict.Remove(rig);
        }

        private static void EnsureFontLoadedIfChanged()
        {
            if (!fontChanged && fontLoaded && font == currentFontKey) return;

            if (customFonts.ContainsKey(font))
            {
                embeddedFont = customFonts[font];
                fontLoaded = embeddedFont != null;
                currentFontKey = font;
                fontChanged = false;

                UpdateAllFonts();
                return;
            }

            if (FontMap.TryGetValue(font, out string fontPath))
            {
                embeddedFont = FontCreator.LoadEmbeddedFontAsset(fontPath);
                fontLoaded = embeddedFont != null;
                currentFontKey = font;
                fontChanged = false;

                UpdateAllFonts();
            }
            else
            {
                Debug.LogWarning($"Font \"{font}\" not found in FontMap or custom fonts.");
                fontLoaded = false;
            }
        }

        private static void UpdateAllFonts()
        {
            foreach (var tmp in nameTagMap.Values)
            {
                if (tmp != null)
                {
                    tmp.font = embeddedFont;
                    ApplyOutline(tmp);
                }
            }

            foreach (var tmp in fpsTagMap.Values)
            {
                if (tmp != null)
                {
                    tmp.font = embeddedFont;
                    ApplyOutline(tmp);
                }
            }

            foreach (var tmp in platformTagMap.Values)
            {
                if (tmp != null)
                {
                    tmp.font = embeddedFont;
                    ApplyOutline(tmp);
                }
            }

            foreach (var tmp in speedTagMap.Values)
            {
                if (tmp != null)
                {
                    tmp.font = embeddedFont;
                    ApplyOutline(tmp);
                }
            }
        }

        // Add this method to update outlines on all existing tags
        public static void UpdateAllOutlines()
        {
            foreach (var tmp in nameTagMap.Values)
            {
                if (tmp != null)
                {
                    ApplyOutline(tmp);
                }
            }

            foreach (var tmp in fpsTagMap.Values)
            {
                if (tmp != null)
                {
                    ApplyOutline(tmp);
                }
            }

            foreach (var tmp in platformTagMap.Values)
            {
                if (tmp != null)
                {
                    ApplyOutline(tmp);
                }
            }

            foreach (var tmp in speedTagMap.Values)
            {
                if (tmp != null)
                {
                    ApplyOutline(tmp);
                }
            }
        }

        private static bool IsPlayerTagged(VRRig rig)
        {
            return rig != null && taggedPlayers.Contains(rig);
        }

        private static Color GetTagColor(VRRig rig, Color defaultColor)
        {
            if (rig == null) return defaultColor;

            if (IsPlayerTagged(rig))
            {
                return new Color(1f, 0.5f, 0f);
            }

            if (matchPlayerColor)
            {
                return rig.playerColor;
            }

            return defaultColor;
        }

        private static Color GetCustomTagColor(string tagType)
        {
            switch (tagType)
            {
                case "name": return customNameColor;
                case "fps": return customFPSColor;
                case "platform": return customPlatformColor;
                case "speed": return customSpeedColor;
                default: return Color.white;
            }
        }

        private static Color GetFPSColor(float fps)
        {
            if (fps >= 72) return fpsGreen;
            if (fps >= 60) return fpsYellow;
            return fpsRed;
        }

        private static string GetPlatformText(VRRig rig)
        {
            if (rig == null) return "[PC]";

            // Try to get cosmetics string using reflection
            try
            {
                var prop = typeof(VRRig).GetProperty("concatStringOfCosmeticsAllowed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    string concat = prop.GetValue(rig) as string;
                    if (concat != null)
                    {
                        if (concat.Contains("S. FIRST LOGIN")) return "[STEAM]";
                        if (concat.Contains("FIRST LOGIN") || (rig.Creator?.GetPlayerRef()?.CustomProperties?.Count >= 2))
                            return "[PC]";
                    }
                }
            }
            catch { }

            // Default to [META] for Quest users
            return "[META]";
        }

        // Add speed calculation method
        private static void UpdatePlayerSpeed(VRRig rig)
        {
            if (rig == null) return;

            Vector3 currentPosition = rig.transform.position;

            if (previousPositions.ContainsKey(rig))
            {
                Vector3 previousPosition = previousPositions[rig];
                float distance = Vector3.Distance(currentPosition, previousPosition);
                float timeDelta = Time.deltaTime;

                if (timeDelta > 0)
                {
                    float speed = distance / timeDelta; // m/s
                    playerSpeeds[rig] = speed;
                }
            }

            previousPositions[rig] = currentPosition;
        }

        public static void Nametags(float size, float offset, bool showFPS, bool showName, bool showPlatform, bool showSpeed, bool showPing)
        {
            if (!PhotonNetwork.InRoom) return;
            EnsureFontLoadedIfChanged();
            if (!fontLoaded) return;

            foreach (VRRig rig in GetVRRigs())
            {
                if (rig == null || (Dev.Plugin.selftags && rig == GorillaTagger.Instance.offlineVRRig)) continue;

                if (showName && !nameTagMap.ContainsKey(rig))
                {
                    GameObject tagObj = new GameObject("NameTag");
                    tagObj.transform.SetParent(rig.transform);
                    tagObj.transform.localPosition = Vector3.up * offset;
                    TextMeshPro tmp = tagObj.AddComponent<TextMeshPro>();
                    SetupTextMesh(tmp, size * 1.2f);
                    nameTagMap[rig] = tmp;
                    
                    // Background quad - DISABLED
                    // if (showBackground)
                    // {
                    //     GameObject bgObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    //     bgObj.name = "NameTag_BG";
                    //     bgObj.transform.SetParent(rig.transform);
                    //     bgObj.transform.localPosition = Vector3.up * offset + Vector3.back * 0.01f;
                    //     bgObj.transform.localScale = new Vector3(size * 2f, size * 0.5f, 1f);
                    //     Renderer bgRenderer = bgObj.GetComponent<Renderer>();
                    //     bgRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
                    //     bgRenderer.material.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, backgroundOpacity);
                    // }

                    GameObject arrowObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    arrowObj.name = "NameTag_Arrow";
                    arrowObj.transform.SetParent(rig.transform);
                    arrowObj.transform.localPosition = Vector3.up * (offset + size) * 2;
                    arrowObj.transform.localScale = Vector3.one * 0.3f;
                    Renderer arrowRenderer = arrowObj.GetComponent<Renderer>();
                    var arrowShader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                    if (arrowShader != null)
                    {
                        arrowRenderer.material = new Material(arrowShader)
                        {
                            mainTexture = LoadTexture("Arrow.png")
                        };
                    }
                }

                if (showFPS && !fpsTagMap.ContainsKey(rig))
                {
                    GameObject fpsObj = new GameObject("FPSTag");
                    fpsObj.transform.SetParent(rig.transform);
                    fpsObj.transform.localPosition = Vector3.up * (offset + fpsTagOffset);
                    TextMeshPro fpsTMP = fpsObj.AddComponent<TextMeshPro>();
                    SetupAntiCheatTextMesh(fpsTMP, size * fpsTagSize);
                    fpsTagMap[rig] = fpsTMP;
                }

                if (showPlatform && !platformTagMap.ContainsKey(rig))
                {
                    GameObject platformObj = new GameObject("PlatformTag");
                    platformObj.transform.SetParent(rig.transform);
                    platformObj.transform.localPosition = Vector3.up * (offset - platformTagOffset);
                    TextMeshPro platformTMP = platformObj.AddComponent<TextMeshPro>();
                    SetupAntiCheatTextMesh(platformTMP, size * platformTagSize);
                    platformTagMap[rig] = platformTMP;
                }

                if (showSpeed && !speedTagMap.ContainsKey(rig))
                {
                    GameObject speedObj = new GameObject("SpeedTag");
                    speedObj.transform.SetParent(rig.transform);
                    speedObj.transform.localPosition = Vector3.up * (offset + speedTagOffset);
                    TextMeshPro speedTMP = speedObj.AddComponent<TextMeshPro>();
                    SetupAntiCheatTextMesh(speedTMP, size * speedTagSize);
                    speedTagMap[rig] = speedTMP;
                }

                if (showPingTags && !pingTagMap.ContainsKey(rig))
                {
                    GameObject pingObj = new GameObject("PingTag");
                    pingObj.transform.SetParent(rig.transform);
                    pingObj.transform.localPosition = Vector3.up * (offset - pingTagOffset);
                    TextMeshPro pingTMP = pingObj.AddComponent<TextMeshPro>();
                    SetupAntiCheatTextMesh(pingTMP, size * pingTagSize);
                    pingTagMap[rig] = pingTMP;
                }

                // Calculate and update speed
                UpdatePlayerSpeed(rig);

                if (showName && nameTagMap.TryGetValue(rig, out var nameTag) && nameTag != null)
                {
                    nameTag.transform.localPosition = Vector3.up * offset;
                    nameTag.transform.rotation = Quaternion.LookRotation(Plugin.GetCameraPos());
                    nameTag.text = rig.playerText1?.text.ToUpper() ?? "UNKNOWN";
                    nameTag.fontSize = size;
                    nameTag.color = GetTagColor(rig, Color.white);
                    
                    // Pulse animation - DISABLED
                    // if (enablePulse)
                    // {
                    //     if (!tagAnimationPhases.ContainsKey(rig))
                    //         tagAnimationPhases[rig] = 0;
                    //     tagAnimationPhases[rig] += Time.deltaTime * pulseSpeed;
                    //     float pulse = 1f + Mathf.Sin(tagAnimationPhases[rig]) * pulseAmount;
                    //     nameTag.transform.localScale = Vector3.one * pulse;
                    // }
                    // else
                    // {
                    //     nameTag.transform.localScale = Vector3.one;
                    // }
                    nameTag.transform.localScale = Vector3.one;
                    
                    ApplyOutline(nameTag, size); // Ensure outline is applied
                    nameTag.gameObject.SetActive(true);
                }
                else if (nameTagMap.TryGetValue(rig, out var nameTagDisable))
                {
                    nameTagDisable.gameObject.SetActive(false);
                }

                if (showFPS && fpsTagMap.TryGetValue(rig, out var fpsTag) && fpsTag != null)
                {
                    fpsTag.transform.localPosition = Vector3.up * (offset + fpsTagOffset);
                    fpsTag.transform.rotation = Quaternion.LookRotation(Plugin.GetCameraPos());
                    var fpsField = Traverse.Create(rig).Field("fps");
                    float fps = 0;
                    if (fpsField != null)
                    {
                        var fpsValue = fpsField.GetValue();
                        if (fpsValue != null) float.TryParse(fpsValue.ToString(), out fps);
                    }
                    fpsTag.text = "FPS: " + (int)fps;
                    fpsTag.fontSize = size * fpsTagSize;
                    
                    // Use FPS indicator colors if enabled, otherwise use custom color or player color
                    if (useFPSIndicator && matchFPSColor)
                        fpsTag.color = GetFPSColor(fps);
                    else if (matchPlayerColor && matchFPSColor)
                        fpsTag.color = GetTagColor(rig, customFPSColor);
                    else
                        fpsTag.color = customFPSColor;
                    
                    ApplyOutline(fpsTag, size * fpsTagSize); // Ensure outline is applied
                    fpsTag.gameObject.SetActive(true);
                }
                else if (fpsTagMap.TryGetValue(rig, out var fpsTagDisable))
                {
                    fpsTagDisable.gameObject.SetActive(false);
                }

                if (showPlatform && platformTagMap.TryGetValue(rig, out var platformTag) && platformTag != null)
                {
                    platformTag.transform.localPosition = Vector3.up * (offset - platformTagOffset);
                    platformTag.transform.rotation = Quaternion.LookRotation(Plugin.GetCameraPos());
                    platformTag.text = GetPlatformText(rig);
                    platformTag.fontSize = size * platformTagSize;
                    
                    // Use custom color or player color based on matchPlatformColor
                    if (matchPlayerColor && matchPlatformColor)
                        platformTag.color = GetTagColor(rig, customPlatformColor);
                    else
                        platformTag.color = customPlatformColor;
                    
                    ApplyOutline(platformTag, size * platformTagSize); // Ensure outline is applied
                    platformTag.gameObject.SetActive(true);
                }
                else if (platformTagMap.TryGetValue(rig, out var platformTagDisable))
                {
                    platformTagDisable.gameObject.SetActive(false);
                }

                if (showSpeed && speedTagMap.TryGetValue(rig, out var speedTag) && speedTag != null)
                {
                    speedTag.transform.localPosition = Vector3.up * (offset + speedTagOffset);
                    speedTag.transform.rotation = Quaternion.LookRotation(Plugin.GetCameraPos());

                    float speed = playerSpeeds.ContainsKey(rig) ? playerSpeeds[rig] : 0f;
                    speedTag.text = $"SPD: {speed:F1}m/s";
                    speedTag.fontSize = size * speedTagSize;
                    
                    // Use custom color or player color based on matchSpeedColor
                    if (matchPlayerColor && matchSpeedColor)
                        speedTag.color = GetTagColor(rig, customSpeedColor);
                    else
                        speedTag.color = customSpeedColor;
                    
                    ApplyOutline(speedTag, size * speedTagSize);
                    speedTag.gameObject.SetActive(true);
                }
                else if (speedTagMap.TryGetValue(rig, out var speedTagDisable))
                {
                    speedTagDisable.gameObject.SetActive(false);
                }

                // Ping tag
                if (showPingTags && pingTagMap.TryGetValue(rig, out var pingTag) && pingTag != null)
                {
                    pingTag.transform.localPosition = Vector3.up * (offset - pingTagOffset);
                    pingTag.transform.rotation = Quaternion.LookRotation(Plugin.GetCameraPos());
                    
                    // Get local player's ping
                    int ping = PhotonNetwork.GetPing();
                    
                    pingTag.text = ping + "ms";
                    pingTag.fontSize = size * pingTagSize;
                    pingTag.color = pingColor;
                    
                    ApplyOutline(pingTag, size * pingTagSize);
                    pingTag.gameObject.SetActive(true);
                }
                else if (pingTagMap.TryGetValue(rig, out var pingTagDisable))
                {
                    pingTagDisable.gameObject.SetActive(false);
                }
            }

            CleanupNullRigs();
        }

        private static Texture2D LoadTexture(string embeddedName)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedName))
            {
                byte[] buffer = new byte[s.Length];
                s.Read(buffer, 0, buffer.Length);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(buffer);
                return tex;
            }
        }

        private static void SetupTextMesh(TextMeshPro tmp, float fontSize)
        {
            tmp.font = embeddedFont;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;

            ApplyOutline(tmp);
        }

        // UPDATED OUTLINE IMPLEMENTATION - Now uses outlineColor with auto-adjust for smaller tags
        public static void ApplyOutline(TextMeshPro tmp, float tagSize = 1f)
        {
            if (tmp == null) return;

            // Enable outline and set properties directly
            tmp.enableVertexGradient = false;

            // Calculate effective outline width
            float effectiveOutlineWidth = outlineWidth;
            if (autoOutlineWidth)
            {
                // Auto-adjust: smaller tags get proportionally wider outline
                // Base width is for tagSize = 1f, scales inversely with size
                effectiveOutlineWidth = outlineWidth / Mathf.Max(tagSize, 0.1f);
                // Clamp to reasonable range to prevent extreme values
                effectiveOutlineWidth = Mathf.Clamp(effectiveOutlineWidth, 0.02f, 0.5f);
            }

            // Set outline color and width using the actual outlineColor variable
            tmp.outlineColor = outlineColor;
            tmp.outlineWidth = effectiveOutlineWidth;

            // Force material update with the correct outline color
            if (tmp.fontMaterial != null)
            {
                tmp.fontMaterial.SetColor("_OutlineColor", outlineColor);
                tmp.fontMaterial.SetFloat("_OutlineWidth", effectiveOutlineWidth);
                tmp.fontMaterial.EnableKeyword("OUTLINE_ON");
            }

            // Force mesh update
            tmp.ForceMeshUpdate();
        }

        private static void CleanupNullRigs()
        {
            CleanupTMPMap(nameTagMap);
            CleanupTMPMap(fpsTagMap);
            CleanupTMPMap(platformTagMap);
            CleanupTMPMap(speedTagMap);
            CleanupRendererMap(speakingTagMap);

            // Clean up speed tracking dictionaries
            List<VRRig> toRemoveSpeed = new List<VRRig>();
            foreach (var rig in previousPositions.Keys)
            {
                if (rig == null || rig.transform == null)
                    toRemoveSpeed.Add(rig);
            }
            foreach (var rig in toRemoveSpeed)
            {
                previousPositions.Remove(rig);
                playerSpeeds.Remove(rig);
            }

            List<VRRig> toRemove = new List<VRRig>();
            foreach (var rig in taggedPlayers)
            {
                if (rig == null || rig.transform == null)
                {
                    toRemove.Add(rig);
                }
            }
            foreach (var rig in toRemove)
            {
                taggedPlayers.Remove(rig);
            }
        }

        private static void CleanupTMPMap(Dictionary<VRRig, TextMeshPro> dict)
        {
            List<VRRig> toRemove = new List<VRRig>();
            foreach (var kvp in dict)
            {
                if (kvp.Key == null || kvp.Key.transform == null)
                {
                    if (kvp.Value != null)
                        UnityEngine.Object.Destroy(kvp.Value.gameObject);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var rig in toRemove) dict.Remove(rig);
        }

        private static void CleanupRendererMap(Dictionary<VRRig, Renderer> dict)
        {
            List<VRRig> toRemove = new List<VRRig>();
            foreach (var kvp in dict)
            {
                if (kvp.Key == null || kvp.Key.transform == null)
                {
                    if (kvp.Value != null)
                        UnityEngine.Object.Destroy(kvp.Value.gameObject);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var rig in toRemove) dict.Remove(rig);
        }
    }
}
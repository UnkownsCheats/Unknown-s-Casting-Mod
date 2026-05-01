using UnknownCasting;
using UnknownCasting.Core;
using UnknownCasting.Core.Keybinds;
using BepInEx;
using GorillaNetworking;
using GorillaGameModes;
using Photon.Pun;
using Stats.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Dev
{
    [BepInPlugin("com.UnknownCasting.Devs", "Devs Cam", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static Font silkScreen;
        private GUIStyle label;
        private GUIStyle offsetLabel;
        private Texture2D blackTex;
        private Texture2D purpleTex;
        private Texture2D grayTex;
        private Texture2D buttonTex;
        private Texture2D sliderBackTex;
        private Texture2D sliderFillTex;
        private Texture2D whiteTex;
        private bool nametags = true; // ON by default
        public static bool selftags = false;
        public static bool talkingindictor = false;
        private float tagssize = 1.15f;
        private float tagsoffset = 0.5f;
        private float menuAnimationProgress = 0f;
        private float animationSpeed = 5f;
        private Texture2D CreditsPage;
        private Texture2D NameTagsOptions;
        private Texture2D CameraOptions;
        private Texture2D CamMods;
        private Texture2D PhotonNetworkingOptions;
        private Texture2D AnitCheatsOptions;
        private Texture2D ExtraFeaturesTab;
        private Texture2D KeybindsTab;
        private Texture2D LeaderBoardTab;
        private Texture2D MiniMapTab;
        private Texture2D ConfigTab;
        private int currentTab = 0;
        public bool ShowMenu = true;
        private int antiCheatSelectedTab = 0;
        private bool HandCam;
        public static bool ShowCameraObj;
        private bool EnableSmoothing = true;
        private bool WASD = false;
        private string roomtojoin = "";
        private string nametochange = "";
        private bool hideRoomCode = false;
        public static bool showPlatforms = false; // OFF by default
        private bool showFPS = true; // ON by default
        private bool showSpeedTags = false; // OFF by default
        private bool showPingTags = false; // OFF by default
        private bool fpsCapEnabled = false; // FPS cap toggle
        public static bool freecam = false;
        private bool awaitingKey = false;
        private string currentKeyTarget = "";
        private int lastTab = 0;
        private float heightAnimationProgress = 1f;
        private float currentHeight = 550f;
        private float targetHeight = 550f;
        private Vector2 dragOffset = Vector2.zero;
        private bool isDragging = false;
        private bool cursorEnabled = false;
        private bool micToggleEnabled = false;
        private bool fpsUnlocked = false;
        private float fpsCap = 144f;
        private float nearClip = 0.03f;
        private Texture2D LavaDistanceTab;
        private bool lavaDistanceEnabled = true;
        private float lavaWarningThreshold = 10f;
        private float lavaDangerThreshold = 5f;
        private bool manualCameraControls = false;
        private float manualMoveSpeed = 0.1f;
        private int viewMode = 0; // 0 = Third Person, 1 = Front Third Person, 2 = First Person
        private bool rigSettingsExpanded = false;
        private int currentGameModeIndex = 0;
        
        // Environment settings
        private int timeOfDayIndex = 0; // 0=Normal, 1=Day, 2=Night
        private int weatherIndex = 0; // 0=Normal, 1=Clear, 2=Rainy
        
        // Dropdown state variables
        private bool gameModeDropdownOpen = false;
        private bool timeChangerDropdownOpen = false;
        private Vector2 gameModeScrollPos = Vector2.zero;
        private Vector2 timeChangerScrollPos = Vector2.zero;
        private bool gameModeDropdownHover = false;
        private bool timeChangerDropdownHover = false;
        
        // Toast Notifications settings
        private bool showJoinNotifications = true;
        private bool showLeaveNotifications = true;
        private float toastDuration = 3f;
        
        // FPS Counter settings
        private bool showFPSCounter = true;
        private int fpsPosition = 0; // 0=TopLeft, 1=TopRight, 2=BottomLeft, 3=BottomRight
        private int fpsStyle = 0; // 0=Simple, 1=Detailed, 2=Minimal, 3=Fancy
        
        // Kill Feed settings
        private bool showKillFeed = true;
        private float killFeedDuration = 5f;
        
        // Player Stats settings
        private bool showStatsTags = true;
        
        // Name tag background and animation settings
        private bool showNameTagBackground = false;
        private float nameTagBackgroundOpacity = 0.5f;
        private bool enableNameTagPulse = false;
        private float nameTagPulseSpeed = 2f;
        private float nameTagPulseAmount = 0.2f;

        // Nametag outline settings
        private Color outlineColor = Color.black;
        private float outlineWidth = 0.17f;
        private bool matchOutlineToPlayer = false;
        private Vector3 outlineColorSlider = Vector3.zero; // RGB sliders
        private bool autoOutlineWidth = true; // Auto-adjust outline width based on tag size

        // Config tab variables
        private string newPresetName = "NewPreset";
        private string selectedPreset = "";
        private List<string> presetList = new List<string>();
        private Vector2 presetScrollPosition = Vector2.zero;

        private readonly Dictionary<int, float> tabHeights = new Dictionary<int, float>
        {
            { 0, 600f },
            { 1, 600f },
            { 2, 600f },
            { 3, 600f },
            { 4, 600f },
            { 5, 600f },
            { 6, 600f },
            { 7, 600f },
            { 8, 600f },
            { 9, 600f },
            { 10, 600f },
            { 11, 600f }
        };

        private string[] availableFonts = new string[] { "Pixel", "GorillaTag", "2P", "DayDream", "Upheavtt", "Designer" };
        private int selectedFontIndex = 0;
        private bool showLeaderboard = false;
        private List<VRRig> vrrigs;
        
        // UI Scroll positions
        private Vector2 nameTagsScrollPosition = Vector2.zero;
        private Vector2 statsScrollPosition = Vector2.zero;

        // MiniMap settings
        public static bool showMiniMap = false;
        private float miniMapSize = 200f;
        private float miniMapZoom = 50f;
        private float miniMapOpacity = 0.8f;
        private float miniMapX = 20f;
        private float miniMapY = 20f;

        // Leaderboard settings
        private float leaderboardX = 15f;
        private float leaderboardY = 100f;
        private float leaderboardScale = 1f;
        private float leaderboardOpacity = 0.9f;
        private float modernSpacing = 15f;
        private float colorBoxSize = 18f;
        private float textScale = 1f;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            LoadSettings();
            gameObject.AddComponent<UnknownCasting.Core.Cursor.CursorMod>();
            gameObject.AddComponent<UnknownCasting.Core.MicToggle.MicToggle>();
            gameObject.AddComponent<UnknownCasting.Core.HandCam>();

            // Wait for game to fully load before creating camera
            StartCoroutine(InitializeCameraWhenReady());
            CameraUpdater.x = saved.camX;
            CameraUpdater.y = saved.camY;
            CameraUpdater.z = saved.camZ;
            CameraUpdater.FOV = saved.camFOV;
            CameraUpdater.positionSmoothing = saved.camSmoothing;
            CameraUpdater.rotationSmoothing = saved.camRotationSmoothing;
             CameraUpdater.enableSmoothing = saved.camEnableSmoothing;
             CameraUpdater.smoothingType = saved.camSmoothingType;
             CameraUpdater.rollLock = saved.rollLock;
             CameraUpdater.enableSelfRigSmoothing = saved.enableSelfRigSmoothing;
             CameraUpdater.selfRigLerpAmount = saved.selfRigLerpAmount;
            blackTex = MakeTex(new Color(0.188f, 0.188f, 0.188f));
            whiteTex = MakeTex(Color.white);
            purpleTex = MakeTex(new Color(0.6f, 0.2f, 1f));
            grayTex = MakeTex(new Color(0.251f, 0.251f, 0.251f));
            if (buttonTex == null) buttonTex = MakeTex(new Color(0.2f, 0.2f, 0.2f, 0.8f));
            if (sliderBackTex == null) sliderBackTex = MakeTex(new Color(0.251f, 0.251f, 0.251f));
            if (sliderFillTex == null) sliderFillTex = MakeTex(new Color(0.5f, 0.5f, 0.5f));
            NameTagsOptions = LoadTexture("NametagOptions.png");
            CameraOptions = LoadTexture("CameraOptions.png");
            CamMods = LoadTexture("CamMods.png");
            PhotonNetworkingOptions = LoadTexture("PhotonNetworking.png");
            AnitCheatsOptions = LoadTexture("AntiCheats.png");
            ExtraFeaturesTab = LoadTexture("ExtraOptions.png");
            CreditsPage = LoadTexture("Credits.png");
            KeybindsTab = LoadTexture("Keybinds.png");
            LeaderBoardTab = LoadTexture("LeaderBoard.png");
            MiniMapTab = LoadTexture("MiniMap.png");
            ConfigTab = LoadTexture("ConfigTab.png");
            LavaDistanceTab = LoadTexture("LavaDistance.png");
            fpsUnlocked = FpsUnLock.FpsUnlocked;
            CameraUpdater.nearClip = saved.nearClip;

            // Load custom fonts
            NameTags.LoadCustomFonts();
            UpdateAvailableFonts();

            // Load the font first
            if (silkScreen == null) LoadFont("silkscreen", ref silkScreen);

            // Initialize leaderboard (this will use the silkScreen font)
            Stats.Core.LeaderBoard.Initialize();

            // Update the distance display font to match leaderboard
            UnknownCasting.Core.Distance.UpdateFont();

            if (GorillaTagger.Instance != null)
            {
                CameraUpdater.rig = GorillaTagger.Instance.offlineVRRig;
            }

            vrrigs = new List<VRRig>();

            // Apply initial leaderboard settings
            ApplyLeaderboardSettings();

            // Initialize mini map with proper setup
            if (MiniMap.Instance == null)
            {
                GameObject miniMapObj = new GameObject("MiniMapManager");
                miniMapObj.AddComponent<MiniMap>();
                DontDestroyOnLoad(miniMapObj);
            }

            // Apply initial mini map settings
            MiniMap.ShowMiniMap = showMiniMap;
            MiniMap.MiniMapSize = miniMapSize;
            MiniMap.MiniMapZoom = miniMapZoom;
            MiniMap.MiniMapOpacity = miniMapOpacity;
            MiniMap.MiniMapPosition = new Vector2(miniMapX, miniMapY);

            if (MiniMap.Instance != null)
            {
                MiniMap.Instance.UpdateMiniMapDisplay();
            }

            // Load preset list
            RefreshPresetList();

            // Initialize Toast Notifications
            GameObject toastObj = new GameObject("ToastNotificationManager");
            toastObj.AddComponent<UnknownCasting.Core.ToastNotification>();
            DontDestroyOnLoad(toastObj);
            
            if (UnknownCasting.Core.ToastNotification.Instance != null)
            {
                Debug.Log("[UnknownCasting] Toast Notifications initialized");
            }

            // Initialize FPS Counter
            GameObject fpsObj = new GameObject("FPSCounterManager");
            fpsObj.AddComponent<UnknownCasting.Core.FPSCounter>();
            DontDestroyOnLoad(fpsObj);
            
            // Wait a frame for Awake to run, then initialize
            StartCoroutine(InitializeFPSCounter());
        }
        
        private System.Collections.IEnumerator InitializeFPSCounter()
        {
            yield return null; // Wait one frame

            if (UnknownCasting.Core.FPSCounter.Instance != null)
            {
                Debug.Log("[UnknownCasting] Initializing FPS Counter");
                UnknownCasting.Core.FPSCounter.Instance.SetEnabled(true); // Always on by default
                UnknownCasting.Core.FPSCounter.Instance.SetPosition(UnknownCasting.Core.FPSCounter.FPSPosition.TopLeft);
            }
            else
            {
                Debug.LogError("[UnknownCasting] FPSCounter.Instance is null after initialization!");
            }

            // Initialize Kill Feed
            GameObject killFeedObj = new GameObject("KillFeedManager");
            killFeedObj.AddComponent<UnknownCasting.Core.KillFeed>();
            DontDestroyOnLoad(killFeedObj);

            if (UnknownCasting.Core.KillFeed.Instance != null)
            {
                Debug.Log("[UnknownCasting] Kill Feed initialized");
                UnknownCasting.Core.KillFeed.Instance.SetEnabled(true); // Always on by default
            }

            // Initialize Player Stats Manager
            GameObject statsObj = new GameObject("PlayerStatsManager");
            statsObj.AddComponent<UnknownCasting.Core.PlayerStatsManager>();
            DontDestroyOnLoad(statsObj);

            // Initialize showStatsTags to true
            showStatsTags = true;
            if (UnknownCasting.Core.PlayerStatsManager.Instance != null)
            {
                UnknownCasting.Core.PlayerStatsManager.Instance.SetShowStatsTags(true);
            }

            // Initialize Player Join/Leave Detector
            GameObject joinLeaveObj = new GameObject("PlayerJoinLeaveDetector");
            joinLeaveObj.AddComponent<UnknownCasting.Core.PlayerJoinLeaveDetector>();
            DontDestroyOnLoad(joinLeaveObj);
        }

        private System.Collections.IEnumerator InitializeCameraWhenReady()
        {
            // Wait until GorillaTagger.Instance is available (game fully loaded)
            while (GorillaTagger.Instance == null)
            {
                yield return null;
            }

            // Wait for offlineVRRig to be available
            while (GorillaTagger.Instance.offlineVRRig == null)
            {
                yield return null;
            }

            // Wait a few more frames to ensure everything is fully initialized
            yield return null;
            yield return null;
            yield return null;

            // Force desktop mode - camera will only be created if not in VR
            CameraUpdater.ForceDesktopMode(true);

            // Create camera (StartCamera will check VR mode internally)
            CameraUpdater.StartCamera();

            // Set camera settings (only if camera was created)
            if (CameraUpdater.cam != null)
            {
                CameraUpdater.x = saved.camX;
                CameraUpdater.y = saved.camY;
                CameraUpdater.z = saved.camZ;
                CameraUpdater.FOV = saved.camFOV;
                CameraUpdater.positionSmoothing = saved.camSmoothing;
                CameraUpdater.rotationSmoothing = saved.camRotationSmoothing;
                CameraUpdater.enableSmoothing = saved.camEnableSmoothing;
                CameraUpdater.smoothingType = saved.camSmoothingType;
                CameraUpdater.rollLock = saved.rollLock;
                CameraUpdater.enableSelfRigSmoothing = saved.enableSelfRigSmoothing;
                CameraUpdater.selfRigLerpAmount = saved.selfRigLerpAmount;

                // Set rig reference
                CameraUpdater.rig = GorillaTagger.Instance.offlineVRRig;
                CameraUpdater.nearClip = saved.nearClip;

                Debug.Log("[UnknownCasting] Desktop camera initialized after game load");
            }
        }

        private void UpdateAvailableFonts()
        {
            List<string> fontList = new List<string>();

            // Add built-in fonts first
            fontList.AddRange(new string[] { "Pixel", "GorillaTag", "2P", "DayDream", "Upheavtt", "Designer" });

            // Add custom fonts from BC_Fonts folder
            if (NameTags.customFonts != null)
            {
                foreach (string customFont in NameTags.customFonts.Keys)
                {
                    if (!fontList.Contains(customFont))
                    {
                        fontList.Add(customFont);
                        Debug.Log($"[UnknownCasting] Added custom font to list: {customFont}");
                    }
                }
            }

            availableFonts = fontList.ToArray();

            // Reset selected index if it's out of bounds
            if (selectedFontIndex >= availableFonts.Length)
            {
                selectedFontIndex = 0;
            }

            Debug.Log($"[UnknownCasting] Total available fonts: {availableFonts.Length}");
            Debug.Log($"[UnknownCasting] Fonts: {string.Join(", ", availableFonts)}");
        }

        private void Update()
        {
            // Helper function to check if a specific key was pressed
            bool CheckKeyPressed(Key key)
            {
                switch (key)
                {
                    case Key.Tab: return Keyboard.current.tabKey.wasPressedThisFrame;
                    case Key.Space: return Keyboard.current.spaceKey.wasPressedThisFrame;
                    case Key.Escape: return Keyboard.current.escapeKey.wasPressedThisFrame;
                    case Key.Enter: return Keyboard.current.enterKey.wasPressedThisFrame;
                    case Key.Backspace: return Keyboard.current.backspaceKey.wasPressedThisFrame;
                    case Key.Delete: return Keyboard.current.deleteKey.wasPressedThisFrame;
                    case Key.Insert: return Keyboard.current.insertKey.wasPressedThisFrame;
                    case Key.Home: return Keyboard.current.homeKey.wasPressedThisFrame;
                    case Key.End: return Keyboard.current.endKey.wasPressedThisFrame;
                    case Key.PageUp: return Keyboard.current.pageUpKey.wasPressedThisFrame;
                    case Key.PageDown: return Keyboard.current.pageDownKey.wasPressedThisFrame;
                    case Key.UpArrow: return Keyboard.current.upArrowKey.wasPressedThisFrame;
                    case Key.DownArrow: return Keyboard.current.downArrowKey.wasPressedThisFrame;
                    case Key.LeftArrow: return Keyboard.current.leftArrowKey.wasPressedThisFrame;
                    case Key.RightArrow: return Keyboard.current.rightArrowKey.wasPressedThisFrame;
                    case Key.A: return Keyboard.current.aKey.wasPressedThisFrame;
                    case Key.B: return Keyboard.current.bKey.wasPressedThisFrame;
                    case Key.C: return Keyboard.current.cKey.wasPressedThisFrame;
                    case Key.D: return Keyboard.current.dKey.wasPressedThisFrame;
                    case Key.E: return Keyboard.current.eKey.wasPressedThisFrame;
                    case Key.F: return Keyboard.current.fKey.wasPressedThisFrame;
                    case Key.G: return Keyboard.current.gKey.wasPressedThisFrame;
                    case Key.H: return Keyboard.current.hKey.wasPressedThisFrame;
                    case Key.I: return Keyboard.current.iKey.wasPressedThisFrame;
                    case Key.J: return Keyboard.current.jKey.wasPressedThisFrame;
                    case Key.K: return Keyboard.current.kKey.wasPressedThisFrame;
                    case Key.L: return Keyboard.current.lKey.wasPressedThisFrame;
                    case Key.M: return Keyboard.current.mKey.wasPressedThisFrame;
                    case Key.N: return Keyboard.current.nKey.wasPressedThisFrame;
                    case Key.O: return Keyboard.current.oKey.wasPressedThisFrame;
                    case Key.P: return Keyboard.current.pKey.wasPressedThisFrame;
                    case Key.Q: return Keyboard.current.qKey.wasPressedThisFrame;
                    case Key.R: return Keyboard.current.rKey.wasPressedThisFrame;
                    case Key.S: return Keyboard.current.sKey.wasPressedThisFrame;
                    case Key.T: return Keyboard.current.tKey.wasPressedThisFrame;
                    case Key.U: return Keyboard.current.uKey.wasPressedThisFrame;
                    case Key.V: return Keyboard.current.vKey.wasPressedThisFrame;
                    case Key.W: return Keyboard.current.wKey.wasPressedThisFrame;
                    case Key.X: return Keyboard.current.xKey.wasPressedThisFrame;
                    case Key.Y: return Keyboard.current.yKey.wasPressedThisFrame;
                    case Key.Z: return Keyboard.current.zKey.wasPressedThisFrame;
                    case Key.F1: return Keyboard.current.f1Key.wasPressedThisFrame;
                    case Key.F2: return Keyboard.current.f2Key.wasPressedThisFrame;
                    case Key.F3: return Keyboard.current.f3Key.wasPressedThisFrame;
                    case Key.F4: return Keyboard.current.f4Key.wasPressedThisFrame;
                    case Key.F5: return Keyboard.current.f5Key.wasPressedThisFrame;
                    case Key.F6: return Keyboard.current.f6Key.wasPressedThisFrame;
                    case Key.F7: return Keyboard.current.f7Key.wasPressedThisFrame;
                    case Key.F8: return Keyboard.current.f8Key.wasPressedThisFrame;
                    case Key.F9: return Keyboard.current.f9Key.wasPressedThisFrame;
                    case Key.F10: return Keyboard.current.f10Key.wasPressedThisFrame;
                    case Key.F11: return Keyboard.current.f11Key.wasPressedThisFrame;
                    case Key.F12: return Keyboard.current.f12Key.wasPressedThisFrame;
                    default: return false;
                }
            }

            // Mic toggle keybind (T key)
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame && micToggleEnabled)
            {
                if (UnknownCasting.Core.MicToggle.MicToggle.Instance != null)
                {
                    UnknownCasting.Core.MicToggle.MicToggle.Instance.ToggleMicrophone();
                }
            }

            if (Keyboard.current != null)
            {
                Key menuKey = GuiKeybinds.GuiKey;
                bool keyPressed = false;
                switch (menuKey)
                {
                    case Key.Tab: keyPressed = Keyboard.current.tabKey.wasPressedThisFrame; break;
                    case Key.Space: keyPressed = Keyboard.current.spaceKey.wasPressedThisFrame; break;
                    case Key.Escape: keyPressed = Keyboard.current.escapeKey.wasPressedThisFrame; break;
                    case Key.Enter: keyPressed = Keyboard.current.enterKey.wasPressedThisFrame; break;
                    case Key.Backspace: keyPressed = Keyboard.current.backspaceKey.wasPressedThisFrame; break;
                    case Key.Delete: keyPressed = Keyboard.current.deleteKey.wasPressedThisFrame; break;
                    case Key.Insert: keyPressed = Keyboard.current.insertKey.wasPressedThisFrame; break;
                    case Key.Home: keyPressed = Keyboard.current.homeKey.wasPressedThisFrame; break;
                    case Key.End: keyPressed = Keyboard.current.endKey.wasPressedThisFrame; break;
                    case Key.PageUp: keyPressed = Keyboard.current.pageUpKey.wasPressedThisFrame; break;
                    case Key.PageDown: keyPressed = Keyboard.current.pageDownKey.wasPressedThisFrame; break;
                    case Key.UpArrow: keyPressed = Keyboard.current.upArrowKey.wasPressedThisFrame; break;
                    case Key.DownArrow: keyPressed = Keyboard.current.downArrowKey.wasPressedThisFrame; break;
                    case Key.LeftArrow: keyPressed = Keyboard.current.leftArrowKey.wasPressedThisFrame; break;
                    case Key.RightArrow: keyPressed = Keyboard.current.rightArrowKey.wasPressedThisFrame; break;
                    case Key.A: keyPressed = Keyboard.current.aKey.wasPressedThisFrame; break;
                    case Key.B: keyPressed = Keyboard.current.bKey.wasPressedThisFrame; break;
                    case Key.C: keyPressed = Keyboard.current.cKey.wasPressedThisFrame; break;
                    case Key.D: keyPressed = Keyboard.current.dKey.wasPressedThisFrame; break;
                    case Key.E: keyPressed = Keyboard.current.eKey.wasPressedThisFrame; break;
                    case Key.F: keyPressed = Keyboard.current.fKey.wasPressedThisFrame; break;
                    case Key.G: keyPressed = Keyboard.current.gKey.wasPressedThisFrame; break;
                    case Key.H: keyPressed = Keyboard.current.hKey.wasPressedThisFrame; break;
                    case Key.I: keyPressed = Keyboard.current.iKey.wasPressedThisFrame; break;
                    case Key.J: keyPressed = Keyboard.current.jKey.wasPressedThisFrame; break;
                    case Key.K: keyPressed = Keyboard.current.kKey.wasPressedThisFrame; break;
                    case Key.L: keyPressed = Keyboard.current.lKey.wasPressedThisFrame; break;
                    case Key.M: keyPressed = Keyboard.current.mKey.wasPressedThisFrame; break;
                    case Key.N: keyPressed = Keyboard.current.nKey.wasPressedThisFrame; break;
                    case Key.O: keyPressed = Keyboard.current.oKey.wasPressedThisFrame; break;
                    case Key.P: keyPressed = Keyboard.current.pKey.wasPressedThisFrame; break;
                    case Key.Q: keyPressed = Keyboard.current.qKey.wasPressedThisFrame; break;
                    case Key.R: keyPressed = Keyboard.current.rKey.wasPressedThisFrame; break;
                    case Key.S: keyPressed = Keyboard.current.sKey.wasPressedThisFrame; break;
                    case Key.T: keyPressed = Keyboard.current.tKey.wasPressedThisFrame; break;
                    case Key.U: keyPressed = Keyboard.current.uKey.wasPressedThisFrame; break;
                    case Key.V: keyPressed = Keyboard.current.vKey.wasPressedThisFrame; break;
                    case Key.W: keyPressed = Keyboard.current.wKey.wasPressedThisFrame; break;
                    case Key.X: keyPressed = Keyboard.current.xKey.wasPressedThisFrame; break;
                    case Key.Y: keyPressed = Keyboard.current.yKey.wasPressedThisFrame; break;
                    case Key.Z: keyPressed = Keyboard.current.zKey.wasPressedThisFrame; break;
                    case Key.F1: keyPressed = Keyboard.current.f1Key.wasPressedThisFrame; break;
                    case Key.F2: keyPressed = Keyboard.current.f2Key.wasPressedThisFrame; break;
                    case Key.F3: keyPressed = Keyboard.current.f3Key.wasPressedThisFrame; break;
                    case Key.F4: keyPressed = Keyboard.current.f4Key.wasPressedThisFrame; break;
                    case Key.F5: keyPressed = Keyboard.current.f5Key.wasPressedThisFrame; break;
                    case Key.F6: keyPressed = Keyboard.current.f6Key.wasPressedThisFrame; break;
                    case Key.F7: keyPressed = Keyboard.current.f7Key.wasPressedThisFrame; break;
                    case Key.F8: keyPressed = Keyboard.current.f8Key.wasPressedThisFrame; break;
                    case Key.F9: keyPressed = Keyboard.current.f9Key.wasPressedThisFrame; break;
                    case Key.F10: keyPressed = Keyboard.current.f10Key.wasPressedThisFrame; break;
                    case Key.F11: keyPressed = Keyboard.current.f11Key.wasPressedThisFrame; break;
                    case Key.F12: keyPressed = Keyboard.current.f12Key.wasPressedThisFrame; break;
                }
                if (keyPressed)
                    ToggleMenu();

                // View Changer key to cycle through Third Person -> Front Third Person -> First Person
                if (Keyboard.current != null && CheckKeyPressed(GuiKeybinds.ViewChangerKey) && !ShowMenu)
                {
                    CycleViewMode();
                }
            }

            if (nametags)
                NameTags.Nametags(tagssize, tagsoffset, NameTags.showFPSTags, NameTags.showName, NameTags.showPlatformTags, NameTags.showSpeedTags, NameTags.showPingTags);
            else
                NameTags.DestroyAllNameTags();

             CameraUpdater.Update();
             UnknownCasting.Core.RigLerp.Update();
             AntiAFK.AntiAFKKick();

            // WASD runs after CameraUpdater so it uses the spectating camera
            if (WASD && !AutoSpec.AutoPilotEnabled && !Dev.Plugin.freecam)
                UnknownCasting.Core.WASD.Wasd();
            
            // Fly mode (key-based fly from head position)
            if (UnknownCasting.Core.WASD.fly && !AutoSpec.AutoPilotEnabled && !Dev.Plugin.freecam)
                UnknownCasting.Core.WASD.Fly();

            // Get VRRigs using GorillaGameManager
            vrrigs = GetVRRigs();

            if (showMiniMap && MiniMap.Instance != null)
            {
                MiniMap.Instance.UpdateCameraPosition();
            }

            if (AutoSpec.AutoPilotEnabled)
            {
                AutoSpec.Update();
            }

            // Update lava distance display every frame if enabled
            if (lavaDistanceEnabled)
            {
                UnknownCasting.Core.Distance.UpdateDistanceDisplay();
            }

            // Apply FPS cap if enabled
            if (fpsCapEnabled)
            {
                Application.targetFrameRate = (int)fpsCap;
            }
            else if (!fpsUnlocked)
            {
                Application.targetFrameRate = 144;
            }
        }

        private void LateUpdate()
        {
            UnknownCasting.Core.RigLerp.LateUpdate();
        }

        private         void OnGUI()
        {
            // Draw camera render texture to screen when active (desktop only)
            if (CameraUpdater.renderTexture != null && !CameraUpdater.isVRMode)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), CameraUpdater.renderTexture);
            }

            label = new GUIStyle(GUI.skin.label)
            {
                font = GUI.skin.label.font,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                fontSize = 14
            };
            offsetLabel = new GUIStyle(GUI.skin.label)
            {
                font = GUI.skin.label.font,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                fontSize = 10
            };

            // Draw the leaderboard regardless of whether the menu is open or not
            if (showLeaderboard && vrrigs != null)
            {
                LeaderBoard.DrawLeaderboard(vrrigs);
            }

            if (ShowMenu || menuAnimationProgress > 0f)
            {
                menuAnimationProgress = Mathf.Lerp(menuAnimationProgress, ShowMenu ? 1f : 0f, Time.deltaTime * animationSpeed);
                if (currentTab != lastTab)
                {
                    heightAnimationProgress = 0f;
                    lastTab = currentTab;
                    targetHeight = tabHeights.ContainsKey(currentTab) ? tabHeights[currentTab] : 600;
                }

                heightAnimationProgress = Mathf.Lerp(heightAnimationProgress + Time.deltaTime * 5f, 1f, Time.deltaTime * 5f);
                currentHeight = Mathf.Lerp(currentHeight, targetHeight, heightAnimationProgress);

                float width = 350f * menuAnimationProgress;
                float height = currentHeight * menuAnimationProgress;
                float xPos = 10f;
                float yPos = 15f;

                // Draggable title bar area
                Rect titleBarRect = new Rect(xPos, yPos, width, 25f);
                if (Event.current.type == EventType.MouseDown && titleBarRect.Contains(Event.current.mousePosition))
                {
                    dragOffset = Event.current.mousePosition - new Vector2(xPos, yPos);
                    isDragging = true;
                }
                if (Event.current.type == EventType.MouseUp)
                {
                    isDragging = false;
                }
                if (isDragging && Event.current.type == EventType.MouseDrag)
                {
                    xPos = Event.current.mousePosition.x - dragOffset.x;
                    yPos = Event.current.mousePosition.y - dragOffset.y;
                }

                if (menuAnimationProgress < 1f && ShowMenu)
                {
                    width *= 1.1f;
                    height *= 1.1f;
                    width = Mathf.Lerp(width, 350f * menuAnimationProgress, 0.5f);
                    height = Mathf.Lerp(height, currentHeight * menuAnimationProgress, 0.1f);
                }

                Texture tabTexture = currentTab switch
                {
                    0 => NameTagsOptions,
                    1 => CameraOptions,
                    2 => CamMods,
                    3 => PhotonNetworkingOptions,
                    4 => AnitCheatsOptions,
                    5 => ExtraFeaturesTab,
                    6 => KeybindsTab,
                    7 => CreditsPage,
                    8 => LeaderBoardTab,
                    9 => MiniMapTab,
                    10 => ConfigTab,
                    11 => LavaDistanceTab,
                    _ => CameraOptions
                };

            if (tabTexture == null)
            {
                tabTexture = CameraOptions ?? blackTex ?? grayTex ?? new Texture2D(2, 2);
            }

                if (tabTexture != null)
                {
                    GUI.DrawTexture(new Rect(xPos, yPos, width, height), tabTexture, ScaleMode.StretchToFill);
                }

                GUI.BeginGroup(new Rect(xPos, yPos, width, height));
                if (menuAnimationProgress > 0.7f)
                {
                    float sliderX = 20f;
                    float sliderY = currentTab == 1 ? 40 : 60f;
                    float sliderWidth = width - 40f;
                    float sliderHeight = 14f;

                    currentTab = eUI.TabSliderInt(
                        currentTab,
                        0,
                    11,
                    new Rect(sliderX, sliderY, sliderWidth, sliderHeight),
                        blackTex,
                        whiteTex,
                        grayTex,
                        true,
                        6,
                        2,
                        25);

                    float contentY = sliderY + sliderHeight + 20f;
                    GUI.BeginGroup(new Rect(10, contentY, width - 20, 500));
                    {
                        switch (currentTab)
                        {
                            case 0: DrawNameTagsTab(); break;
                            case 1: DrawCameraSettingsTab(); break;
                            case 2: CameraMods(); break;
                            case 3: DrawPhotonNetworkTab(); break;
                            case 4: DrawAntiCheatSection(); break;
                            case 5: DrawExtraFeaturesTab(); break;
                            case 6: DrawKeyBindsTab(); break;
                            case 7: DrawCreditsTab(); break;
                            case 8: DrawLeaderBoardTab(); break;
                            case 9: DrawMiniMapTab(); break;
                            case 10: DrawConfigTab(); break;
                            case 11: DrawLavaDistanceTab(); break;
                        }
                    }
                    GUI.EndGroup();
                }
                GUI.EndGroup();
            }

            if (awaitingKey && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                foreach (KeyControl keyControl in Keyboard.current.allKeys)
                {
                    if (keyControl.wasPressedThisFrame)
                    {
                        AssignKeybind(currentKeyTarget, keyControl.keyCode);
                        awaitingKey = false;
                        currentKeyTarget = "";
                        break;
                    }
                }
            }
        }

        private void DrawLavaDistanceTab()
        {
            float tabWidth = 350f;
            float controlWidth = 300f;
            float controlHeight = 20f;
            float controlX = (tabWidth - controlWidth) / 2f;
            float currentY = 30f;
            float spacing = 15f;

            Vector2 mousePosition = Event.current.mousePosition;

            // Lava Distance Display Toggle
            Rect displayToggle = new Rect(controlX, currentY, controlWidth, controlHeight);
            float scale = displayToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            displayToggle.width *= scale;
            displayToggle.height *= scale;
            displayToggle.x = (tabWidth - displayToggle.width) / 2f;

            if (GUI.Button(displayToggle, lavaDistanceEnabled ? "Lava Distance <color=green>[ON]</color>" : "Lava Distance <color=red>[OFF]</color>", label))
            {
                lavaDistanceEnabled = !lavaDistanceEnabled;
                if (lavaDistanceEnabled)
                {
                    UnknownCasting.Core.Distance.InitializeDistanceDisplay();
                    UnknownCasting.Core.Distance.ShowDistanceDisplay();
                }
                else
                {
                    UnknownCasting.Core.Distance.HideDistanceDisplay();
                }
            }
            currentY += controlHeight * scale + spacing;

            // Color Indication Toggle
            Rect colorToggle = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = colorToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            colorToggle.width *= scale;
            colorToggle.height *= scale;
            colorToggle.x = (tabWidth - colorToggle.width) / 2f;

            if (GUI.Button(colorToggle, UnknownCasting.Core.Distance.colorIndicationEnabled ? "Color Indication <color=green>[ON]</color>" : "Color Indication <color=red>[OFF]</color>", label))
            {
                UnknownCasting.Core.Distance.ToggleColorIndication();
            }
            currentY += controlHeight * scale + spacing;

            // Ensure display is initialized if enabled
            if (lavaDistanceEnabled)
            {
                UnknownCasting.Core.Distance.EnsureInitialized();
            }

            // BACKGROUND CUSTOMIZATION SECTION
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "BACKGROUND SETTINGS", label);
            currentY += 25f;

            // Background X Position
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Background X: " + UnknownCasting.Core.Distance.backgroundX.ToString("F0"), label);
            currentY += 20f;
            float newBackgroundX = eUI.RoundedSlider(UnknownCasting.Core.Distance.backgroundX, 0, Screen.width, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newBackgroundX != UnknownCasting.Core.Distance.backgroundX)
            {
                UnknownCasting.Core.Distance.backgroundX = newBackgroundX;
                UnknownCasting.Core.Distance.UpdateBackgroundPosition();
            }
            currentY += 20f;

            // Background Y Position
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Background Y: " + UnknownCasting.Core.Distance.backgroundY.ToString("F0"), label);
            currentY += 20f;
            float newBackgroundY = eUI.RoundedSlider(UnknownCasting.Core.Distance.backgroundY, 0, Screen.height, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newBackgroundY != UnknownCasting.Core.Distance.backgroundY)
            {
                UnknownCasting.Core.Distance.backgroundY = newBackgroundY;
                UnknownCasting.Core.Distance.UpdateBackgroundPosition();
            }
            currentY += 20f;

            // Background Width
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Background Width: " + UnknownCasting.Core.Distance.backgroundWidth.ToString("F0"), label);
            currentY += 20f;
            float newBackgroundWidth = eUI.RoundedSlider(UnknownCasting.Core.Distance.backgroundWidth, 50, 2000, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newBackgroundWidth != UnknownCasting.Core.Distance.backgroundWidth)
            {
                UnknownCasting.Core.Distance.backgroundWidth = newBackgroundWidth;
                UnknownCasting.Core.Distance.UpdateBackgroundSize();
            }
            currentY += 20f;

            // Background Height
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Background Height: " + UnknownCasting.Core.Distance.backgroundHeight.ToString("F0"), label);
            currentY += 20f;
            float newBackgroundHeight = eUI.RoundedSlider(UnknownCasting.Core.Distance.backgroundHeight, 20, 600, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newBackgroundHeight != UnknownCasting.Core.Distance.backgroundHeight)
            {
                UnknownCasting.Core.Distance.backgroundHeight = newBackgroundHeight;
                UnknownCasting.Core.Distance.UpdateBackgroundSize();
            }
            currentY += 20f;

            // Background Opacity
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Background Opacity: " + UnknownCasting.Core.Distance.backgroundOpacity.ToString("F2"), label);
            currentY += 20f;
            float newBackgroundOpacity = eUI.RoundedSlider(UnknownCasting.Core.Distance.backgroundOpacity, 0f, 1f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newBackgroundOpacity != UnknownCasting.Core.Distance.backgroundOpacity)
            {
                UnknownCasting.Core.Distance.backgroundOpacity = newBackgroundOpacity;
                UnknownCasting.Core.Distance.UpdateBackgroundOpacity();
            }
            currentY += 20f;

            // TEXT CUSTOMIZATION SECTION
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "TEXT SETTINGS", label);
            currentY += 25f;

            // Text X Offset
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Text X Offset: " + UnknownCasting.Core.Distance.textX.ToString("F0"), label);
            currentY += 20f;
            float newTextX = eUI.RoundedSlider(UnknownCasting.Core.Distance.textX, -500, 500, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newTextX != UnknownCasting.Core.Distance.textX)
            {
                UnknownCasting.Core.Distance.textX = newTextX;
                UnknownCasting.Core.Distance.UpdateTextPosition();
            }
            currentY += 20f;

            // Text Y Offset
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Text Y Offset: " + UnknownCasting.Core.Distance.textY.ToString("F0"), label);
            currentY += 20f;
            float newTextY = eUI.RoundedSlider(UnknownCasting.Core.Distance.textY, -500, 500, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newTextY != UnknownCasting.Core.Distance.textY)
            {
                UnknownCasting.Core.Distance.textY = newTextY;
                UnknownCasting.Core.Distance.UpdateTextPosition();
            }
            currentY += 20f;

            // TEXT SCALE X SLIDER
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Text Scale X: " + UnknownCasting.Core.Distance.textScaleX.ToString("F2"), label);
            currentY += 20f;
            float newTextScaleX = eUI.RoundedSlider(UnknownCasting.Core.Distance.textScaleX, 0.1f, 3f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newTextScaleX != UnknownCasting.Core.Distance.textScaleX)
            {
                UnknownCasting.Core.Distance.textScaleX = newTextScaleX;
                UnknownCasting.Core.Distance.UpdateTextSize();
            }
            currentY += 20f;

            // TEXT SCALE Y SLIDER
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Text Scale Y: " + UnknownCasting.Core.Distance.textScaleY.ToString("F2"), label);
            currentY += 20f;
            float newTextScaleY = eUI.RoundedSlider(UnknownCasting.Core.Distance.textScaleY, 0.1f, 3f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newTextScaleY != UnknownCasting.Core.Distance.textScaleY)
            {
                UnknownCasting.Core.Distance.textScaleY = newTextScaleY;
                UnknownCasting.Core.Distance.UpdateTextSize();
            }
            currentY += 20f;

            // TEXT SIZE SLIDER
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Text Size: " + UnknownCasting.Core.Distance.textSize.ToString("F0"), label);
            currentY += 20f;
            float newTextSize = eUI.RoundedSlider(UnknownCasting.Core.Distance.textSize, 10, 100, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newTextSize != UnknownCasting.Core.Distance.textSize)
            {
                UnknownCasting.Core.Distance.textSize = newTextSize;
                UnknownCasting.Core.Distance.UpdateTextSize();
            }
            currentY += 20f;

            // Text Opacity
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Text Opacity: " + UnknownCasting.Core.Distance.textOpacity.ToString("F2"), label);
            currentY += 20f;
            float newTextOpacity = eUI.RoundedSlider(UnknownCasting.Core.Distance.textOpacity, 0f, 1f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newTextOpacity != UnknownCasting.Core.Distance.textOpacity)
            {
                UnknownCasting.Core.Distance.textOpacity = newTextOpacity;
                UnknownCasting.Core.Distance.UpdateTextOpacity();
            }
            currentY += 20f;

            // Distance thresholds information
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "DISTANCE THRESHOLDS", label);
            currentY += 25f;

            // Warning Threshold Slider
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Warning Threshold: " + lavaWarningThreshold.ToString("F1") + " ft", label);
            currentY += 20f;
            float newWarningThreshold = eUI.RoundedSlider(lavaWarningThreshold, 0f, 30f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newWarningThreshold != lavaWarningThreshold)
            {
                lavaWarningThreshold = newWarningThreshold;
                UnknownCasting.Core.Distance.SetWarningThreshold(lavaWarningThreshold);
            }
            currentY += 20f;

            // Danger Threshold Slider
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Danger Threshold: " + lavaDangerThreshold.ToString("F1") + " ft", label);
            currentY += 20f;
            float newDangerThreshold = eUI.RoundedSlider(lavaDangerThreshold, 0f, 15f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newDangerThreshold != lavaDangerThreshold)
            {
                lavaDangerThreshold = newDangerThreshold;
                UnknownCasting.Core.Distance.SetDangerThreshold(lavaDangerThreshold);
            }

            // Reset to Defaults Button
            currentY += 25f;
            Rect resetButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = resetButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            resetButton.width *= scale;
            resetButton.height *= scale;
            resetButton.x = (tabWidth - resetButton.width) / 2f;

            if (GUI.Button(resetButton, "<color=purple>Reset to Defaults</color>", label))
            {
                UnknownCasting.Core.Distance.ResetToDefaults();
                lavaWarningThreshold = 10f;
                lavaDangerThreshold = 5f;
                UnknownCasting.Core.Distance.SetWarningThreshold(lavaWarningThreshold);
                UnknownCasting.Core.Distance.SetDangerThreshold(lavaDangerThreshold);
            }
        }

        private void DrawStatsTab()
        {
            float tabWidth = 350f;
            float buttonWidth = 300f;
            float buttonHeight = 25f;
            float buttonX = (tabWidth - buttonWidth) / 2f;
            float currentY = 30f;
            float spacing = 10f;

            Vector2 mousePosition = Event.current.mousePosition;

            // Stats Tags Toggle
            Rect statsToggle = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            float scale = statsToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            statsToggle.width *= scale;
            statsToggle.height *= scale;
            statsToggle.x = (tabWidth - statsToggle.width) / 2f;

            if (GUI.Button(statsToggle, showStatsTags ? "Stats Tags <color=green>[ON]</color>" : "Stats Tags <color=red>[OFF]</color>", label))
            {
                showStatsTags = !showStatsTags;
                UnknownCasting.Core.PlayerStatsManager.Instance?.SetShowStatsTags(showStatsTags);
            }
            currentY += buttonHeight * scale + spacing;

            // Player stats list with scroll
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "PLAYER STATS", label);
            currentY += 25f;

            var statsList = UnknownCasting.Core.PlayerStatsManager.Instance?.GetAllStats();
            float listHeight = 300f;
            Rect scrollViewRect = new Rect(buttonX - 10, currentY, buttonWidth + 20, listHeight);
            Rect scrollContentRect = new Rect(0, 0, buttonWidth, (statsList?.Count ?? 0) * 35f + 20);

            statsScrollPosition = GUI.BeginScrollView(scrollViewRect, statsScrollPosition, scrollContentRect);
            {
                if (statsList != null && statsList.Count > 0)
                {
                    for (int i = 0; i < statsList.Count; i++)
                    {
                        var stats = statsList[i];
                        Rect playerRect = new Rect(0, i * 35f, buttonWidth, 30f);
                        
                        // Alternate background
                        if (i % 2 == 0)
                            GUI.DrawTexture(playerRect, grayTex);
                        
                        string statsText = $"{stats.playerName}: Tags={stats.tags}";
                        GUI.Label(playerRect, statsText, label);
                    }
                }
                else
                {
                    GUI.Label(new Rect(0, 0, buttonWidth, 30f), "No players in lobby", label);
                }
            }
            GUI.EndScrollView();
            currentY += listHeight + spacing;

            // Tag input section
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "SET PLAYER TAG", label);
            currentY += 25f;

            // Note: Player tag editing would require additional UI
            // For now just show the toggle
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "Stats tags show custom tags above players", offsetLabel);
        }

        private void DrawKillFeedTab()
        {
            float tabWidth = 350f;
            float buttonWidth = 300f;
            float buttonHeight = 25f;
            float buttonX = (tabWidth - buttonWidth) / 2f;
            float currentY = 30f;
            float spacing = 10f;

            Vector2 mousePosition = Event.current.mousePosition;

            // Kill Feed Toggle
            Rect killFeedToggle = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            float scale = killFeedToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            killFeedToggle.width *= scale;
            killFeedToggle.height *= scale;
            killFeedToggle.x = (tabWidth - killFeedToggle.width) / 2f;

            if (GUI.Button(killFeedToggle, showKillFeed ? "Kill Feed <color=green>[ON]</color>" : "Kill Feed <color=red>[OFF]</color>", label))
            {
                showKillFeed = !showKillFeed;
                UnknownCasting.Core.KillFeed.Instance?.SetEnabled(showKillFeed);
            }
            currentY += buttonHeight * scale + spacing;

            // Duration slider
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "Duration: " + killFeedDuration.ToString("F1") + "s", label);
            currentY += 20f;
            killFeedDuration = eUI.RoundedSlider(killFeedDuration, 1f, 10f, new Rect(buttonX - 5, currentY, buttonWidth, 12), sliderBackTex, sliderFillTex);
            UnknownCasting.Core.KillFeed.Instance?.SetEntryDuration(killFeedDuration);
            currentY += 18f;

            // Position section
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "POSITION", label);
            currentY += 25f;

            string[] positions = { "Top Left", "Top Right", "Bottom Left", "Bottom Right" };
            for (int i = 0; i < positions.Length; i++)
            {
                Rect posButton = new Rect(buttonX + (i % 2) * (buttonWidth / 2 + 5), currentY + (i / 2) * (buttonHeight + 5), buttonWidth / 2 - 5, buttonHeight);
                if (GUI.Button(posButton, positions[i], label))
                {
                    float x = (i == 0 || i == 2) ? 20f : Screen.width - 370f;
                    float y = (i == 0 || i == 1) ? 150f : Screen.height - 100f;
                    UnknownCasting.Core.KillFeed.Instance?.SetPosition(x, y);
                }
            }
            currentY += buttonHeight * 2 + spacing + 10;

            // Clear button
            Rect clearButton = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            scale = clearButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            clearButton.width *= scale;
            clearButton.height *= scale;
            clearButton.x = (tabWidth - clearButton.width) / 2f;

            if (GUI.Button(clearButton, "Clear Feed", label))
            {
                UnknownCasting.Core.KillFeed.Instance?.ClearFeed();
            }
        }

        private void DrawNotificationsTab()
        {
            float tabWidth = 350f;
            float buttonWidth = 300f;
            float buttonHeight = 25f;
            float buttonX = (tabWidth - buttonWidth) / 2f;
            float currentY = 30f;
            float spacing = 10f;

            Vector2 mousePosition = Event.current.mousePosition;

            // Section: Join Notifications
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "PLAYER NOTIFICATIONS", label);
            currentY += 25f;

            // Join toggle
            Rect joinToggle = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            float scale = joinToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            joinToggle.width *= scale;
            joinToggle.height *= scale;
            joinToggle.x = (tabWidth - joinToggle.width) / 2f;

            if (GUI.Button(joinToggle, showJoinNotifications ? "Join Notifications <color=green>[ON]</color>" : "Join Notifications <color=red>[OFF]</color>", label))
            {
                showJoinNotifications = !showJoinNotifications;
                UnknownCasting.Core.ToastNotification.Instance?.SetShowJoinNotifications(showJoinNotifications);
            }
            currentY += buttonHeight * scale + spacing;

            // Leave toggle
            Rect leaveToggle = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            scale = leaveToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            leaveToggle.width *= scale;
            leaveToggle.height *= scale;
            leaveToggle.x = (tabWidth - leaveToggle.width) / 2f;

            if (GUI.Button(leaveToggle, showLeaveNotifications ? "Leave Notifications <color=green>[ON]</color>" : "Leave Notifications <color=red>[OFF]</color>", label))
            {
                showLeaveNotifications = !showLeaveNotifications;
                UnknownCasting.Core.ToastNotification.Instance?.SetShowLeaveNotifications(showLeaveNotifications);
            }
            currentY += buttonHeight * scale + spacing + 10;

            // Toast Duration slider
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "Toast Duration: " + toastDuration.ToString("F1") + "s", label);
            currentY += 20f;
            toastDuration = eUI.RoundedSlider(toastDuration, 1f, 10f, new Rect(buttonX - 5, currentY, buttonWidth, 12), sliderBackTex, sliderFillTex);
            UnknownCasting.Core.ToastNotification.Instance?.SetToastDuration(toastDuration);
            currentY += 18f + spacing;

            // Section: FPS Counter
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "FPS COUNTER", label);
            currentY += 25f;

            // FPS Counter toggle
            Rect fpsToggle = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            scale = fpsToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            fpsToggle.width *= scale;
            fpsToggle.height *= scale;
            fpsToggle.x = (tabWidth - fpsToggle.width) / 2f;

            if (GUI.Button(fpsToggle, showFPSCounter ? "FPS Counter <color=green>[ON]</color>" : "FPS Counter <color=red>[OFF]</color>", label))
            {
                showFPSCounter = !showFPSCounter;
                UnknownCasting.Core.FPSCounter.Instance?.SetEnabled(showFPSCounter);
            }
            currentY += buttonHeight * scale + spacing;

            // FPS Position
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "Position:", label);
            currentY += 20f;

            string[] fpsPositions = { "Top Left", "Top Right", "Bottom Left", "Bottom Right" };
            for (int i = 0; i < fpsPositions.Length; i++)
            {
                Rect posButton = new Rect(buttonX + (i % 2) * (buttonWidth / 2 + 5), currentY + (i / 2) * (buttonHeight + 5), buttonWidth / 2 - 5, buttonHeight);
                if (GUI.Button(posButton, fpsPositions[i], label))
                {
                    fpsPosition = i;
                    var fpsInstance = UnknownCasting.Core.FPSCounter.Instance;
                    if (fpsInstance != null)
                    {
                        fpsInstance.SetPosition((UnknownCasting.Core.FPSCounter.FPSPosition)i);
                    }
                }
            }
            currentY += buttonHeight * 2 + spacing;

            // FPS Style
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "Style:", label);
            currentY += 20f;

            string[] fpsStyles = { "Simple", "Detailed", "Minimal", "Fancy" };
            for (int i = 0; i < fpsStyles.Length; i++)
            {
                Rect styleButton = new Rect(buttonX + (i % 2) * (buttonWidth / 2 + 5), currentY + (i / 2) * (buttonHeight + 5), buttonWidth / 2 - 5, buttonHeight);
                if (GUI.Button(styleButton, fpsStyles[i], label))
                {
                    fpsStyle = i;
                    UnknownCasting.Core.FPSCounter.Instance?.SetStyle((UnknownCasting.Core.FPSCounter.FPSStyle)i);
                }
            }
            currentY += buttonHeight * 2 + spacing;
            
            // Match FPS Color toggle
            Rect matchColorToggle = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            scale = matchColorToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            matchColorToggle.width *= scale;
            matchColorToggle.height *= scale;
            matchColorToggle.x = (tabWidth - matchColorToggle.width) / 2f;
            
            if (GUI.Button(matchColorToggle, "Match FPS Color", label))
            {
                UnknownCasting.Core.FPSCounter.Instance?.ToggleMatchFPSColor();
            }
        }

        private VRRig GetLocalPlayerRig()
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

        private List<VRRig> GetVRRigs()
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

        private void DrawConfigTab()
        {
            float tabWidth = 350f;
            float controlWidth = 300f;
            float controlHeight = 20f;
            float controlX = (tabWidth - controlWidth) / 2f;
            float currentY = 30f;
            float spacing = 15f;

            Vector2 mousePosition = Event.current.mousePosition;

            // Draw gray background for this tab
            GUI.DrawTexture(new Rect(0, 0, tabWidth, 600f), grayTex);

            // Save Current Config button
            Rect saveButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            float scale = saveButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            saveButton.width *= scale;
            saveButton.height *= scale;
            saveButton.x = (tabWidth - saveButton.width) / 2f;

            if (GUI.Button(saveButton, "Save Current Config", label))
            {
                SaveSettings();
            }
            currentY += controlHeight * scale + spacing;

            // Load Default Config button
            Rect loadButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = loadButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            loadButton.width *= scale;
            loadButton.height *= scale;
            loadButton.x = (tabWidth - loadButton.width) / 2f;

            if (GUI.Button(loadButton, "Load Default Config", label))
            {
                LoadSettings();
            }
            currentY += controlHeight * scale + spacing + 10;

            // Preset Management Section
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "PRESET MANAGEMENT", label);
            currentY += 25f;

            // New Preset Name Field
            Rect nameField = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            newPresetName = eUI.TextField(newPresetName, nameField, sliderBackTex, 10);
            currentY += controlHeight + spacing;

            // Create New Preset button
            Rect createButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = createButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            createButton.width *= scale;
            createButton.height *= scale;
            createButton.x = (tabWidth - createButton.width) / 2f;

            if (GUI.Button(createButton, "Create New Preset", label))
            {
                if (!string.IsNullOrEmpty(newPresetName))
                {
                    SavePreset(newPresetName);
                    RefreshPresetList();
                    newPresetName = "NewPreset";
                }
            }
            currentY += controlHeight * scale + spacing;

            // Preset List Label
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Available Presets:", label);
            currentY += 25f;

            // Preset List Scroll View
            float listHeight = 150f;
            Rect scrollViewRect = new Rect(controlX, currentY, controlWidth, listHeight);
            Rect scrollContentRect = new Rect(0, 0, controlWidth - 20, presetList.Count * 35f);

            presetScrollPosition = GUI.BeginScrollView(scrollViewRect, presetScrollPosition, scrollContentRect);
            {
                for (int i = 0; i < presetList.Count; i++)
                {
                    Rect presetItemRect = new Rect(0, i * 35f, controlWidth - 40, 30f);
                    bool isSelected = selectedPreset == presetList[i];

                    // Highlight selected preset
                    if (isSelected)
                    {
                        GUI.DrawTexture(presetItemRect, purpleTex);
                    }

                    if (GUI.Button(presetItemRect, presetList[i], label))
                    {
                        selectedPreset = presetList[i];
                    }
                }
            }
            GUI.EndScrollView();
            currentY += listHeight + spacing;

            // Load/Delete Preset buttons
            Rect loadPresetButton = new Rect(controlX, currentY, controlWidth / 2f - 5f, controlHeight);
            scale = loadPresetButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            loadPresetButton.width *= scale;
            loadPresetButton.height *= scale;

            if (GUI.Button(loadPresetButton, "<color=green>Load Preset</color>", label))
            {
                if (!string.IsNullOrEmpty(selectedPreset))
                {
                    LoadPreset(selectedPreset);
                }
            }

            Rect deletePresetButton = new Rect(controlX + controlWidth / 2f + 5f, currentY, controlWidth / 2f - 5f, controlHeight);
            scale = deletePresetButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            deletePresetButton.width *= scale;
            deletePresetButton.height *= scale;

            if (GUI.Button(deletePresetButton, "<color=red>Delete Preset</color>", label))
            {
                if (!string.IsNullOrEmpty(selectedPreset))
                {
                    DeletePreset(selectedPreset);
                    RefreshPresetList();
                    selectedPreset = "";
                }
            }
        }

        // Preset management methods
        private string GetPresetsFolderPath()
        {
            return Path.Combine(Paths.ConfigPath, "UnknownCastingPresets");
        }

        private void EnsurePresetsFolderExists()
        {
            string presetsFolder = GetPresetsFolderPath();
            if (!Directory.Exists(presetsFolder))
            {
                Directory.CreateDirectory(presetsFolder);
            }
        }

        private void RefreshPresetList()
        {
            presetList.Clear();
            string presetsFolder = GetPresetsFolderPath();

            if (Directory.Exists(presetsFolder))
            {
                string[] presetFiles = Directory.GetFiles(presetsFolder, "*.json");
                foreach (string filePath in presetFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    presetList.Add(fileName);
                }
            }
            presetList.Sort();
        }

        private void SavePreset(string presetName)
        {
            try
            {
                EnsurePresetsFolderExists();
                string presetPath = Path.Combine(GetPresetsFolderPath(), presetName + ".json");

                PresetData preset = new PresetData
                {
                    // Nametag settings
                    nametags = nametags,
                    selftags = selftags,
                    talkingindictor = talkingindictor,
                    matchPlayerColor = NameTags.matchPlayerColor,
                    tagssize = tagssize,
                    tagsoffset = tagsoffset,

                    // Outline settings
                    outlineWidth = outlineWidth,
                    outlineColor = outlineColor,
                    matchOutlineToPlayer = matchOutlineToPlayer,

                    // Font settings
                    selectedFont = availableFonts[selectedFontIndex],

                    // FPS tag settings
                    showFPS = showFPS,
                    fpsTagSize = NameTags.fpsTagSize,
                    fpsTagOffset = NameTags.fpsTagOffset,

                    // Platform tag settings
                    showPlatforms = showPlatforms,
                    platformTagSize = NameTags.platformTagSize,
                    platformTagOffset = NameTags.platformTagOffset,

                    // Speed tag settings
                    showSpeedTags = showSpeedTags,
                    speedTagSize = NameTags.speedTagSize,
                    speedTagOffset = NameTags.speedTagOffset,

                    // Camera settings
                    HandCam = HandCam,
                    ShowCameraObj = ShowCameraObj,
                    EnableSmoothing = EnableSmoothing,
                    WASD = WASD,
                     camX = CameraUpdater.x,
                     camY = CameraUpdater.y,
                     camZ = CameraUpdater.z,
                     camFOV = CameraUpdater.FOV,
                     nearClip = nearClip,
                     camSmoothing = CameraUpdater.positionSmoothing,
                     camEnableSmoothing = CameraUpdater.enableSmoothing,
                     camSmoothingType = CameraUpdater.smoothingType,
                       camRotationSmoothing = CameraUpdater.rotationSmoothing,
                        LerpValue = RigLerp.targetLerpValue,
                        rollLock = CameraUpdater.rollLock,
                        enableSelfRigSmoothing = CameraUpdater.enableSelfRigSmoothing,
                        selfRigLerpAmount = CameraUpdater.selfRigLerpAmount,

                    // Other settings
                    freecam = freecam,
                    cursorEnabled = cursorEnabled,
                    micToggleEnabled = micToggleEnabled,
                    fpsUnlocked = fpsUnlocked,
                    leaderboardX = leaderboardX,
                    leaderboardY = leaderboardY,
                    leaderboardScale = leaderboardScale,
                    leaderboardOpacity = leaderboardOpacity,
                    modernSpacing = modernSpacing,
                    colorBoxSize = colorBoxSize,
                    textScale = textScale,
                    showMiniMap = showMiniMap,
                    miniMapSize = miniMapSize,
                    miniMapZoom = miniMapZoom,
                    miniMapOpacity = miniMapOpacity,
                    miniMapX = miniMapX,
                    miniMapY = miniMapY
                };

                string json = JsonUtility.ToJson(preset, true);
                File.WriteAllText(presetPath, json);

                Logger.LogInfo($"Preset '{presetName}' saved successfully!");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error saving preset: {ex.Message}");
            }
        }

        private void LoadPreset(string presetName)
        {
            try
            {
                string presetPath = Path.Combine(GetPresetsFolderPath(), presetName + ".json");

                if (File.Exists(presetPath))
                {
                    string json = File.ReadAllText(presetPath);
                    PresetData preset = JsonUtility.FromJson<PresetData>(json);

                    // Nametag settings
                    nametags = preset.nametags;
                    selftags = preset.selftags;
                    talkingindictor = preset.talkingindictor;
                    NameTags.matchPlayerColor = preset.matchPlayerColor;
                    tagssize = preset.tagssize;
                    tagsoffset = preset.tagsoffset;

                    // Outline settings
                    outlineWidth = preset.outlineWidth;
                    outlineColor = preset.outlineColor;
                    matchOutlineToPlayer = preset.matchOutlineToPlayer;
                    NameTags.outlineWidth = outlineWidth;
                    NameTags.outlineColor = outlineColor;

                    // Font settings
                    if (!string.IsNullOrEmpty(preset.selectedFont))
                    {
                        int fontIndex = Array.IndexOf(availableFonts, preset.selectedFont);
                        if (fontIndex >= 0)
                        {
                            selectedFontIndex = fontIndex;
                            NameTags.SetFont(preset.selectedFont);
                        }
                    }

                    // FPS tag settings
                    showFPS = preset.showFPS;
                    NameTags.fpsTagSize = preset.fpsTagSize;
                    NameTags.fpsTagOffset = preset.fpsTagOffset;

                    // Platform tag settings
                    showPlatforms = preset.showPlatforms;
                    NameTags.platformTagSize = preset.platformTagSize;
                    NameTags.platformTagOffset = preset.platformTagOffset;

                    // Speed tag settings
                    showSpeedTags = preset.showSpeedTags;
                    NameTags.speedTagSize = preset.speedTagSize;
                    NameTags.speedTagOffset = preset.speedTagOffset;

                    // Camera settings
                    HandCam = preset.HandCam;
                    ShowCameraObj = preset.ShowCameraObj;
                    EnableSmoothing = preset.EnableSmoothing;
                    WASD = preset.WASD;
                    CameraUpdater.x = preset.camX;
                    CameraUpdater.y = preset.camY;
                    CameraUpdater.z = preset.camZ;
                    CameraUpdater.FOV = preset.camFOV;
                    nearClip = preset.nearClip;
                     CameraUpdater.nearClip = preset.nearClip;
                     CameraUpdater.positionSmoothing = preset.camSmoothing;
                     CameraUpdater.rotationSmoothing = preset.camRotationSmoothing;
                      CameraUpdater.enableSmoothing = preset.camEnableSmoothing;
                      CameraUpdater.smoothingType = preset.camSmoothingType;
                       RigLerp.targetLerpValue = preset.LerpValue;
                       CameraUpdater.rollLock = preset.rollLock;
                       CameraUpdater.enableSelfRigSmoothing = preset.enableSelfRigSmoothing;
                       CameraUpdater.selfRigLerpAmount = preset.selfRigLerpAmount;

                    // Other settings
                    freecam = preset.freecam;
                    cursorEnabled = preset.cursorEnabled;
                    micToggleEnabled = preset.micToggleEnabled;
                    fpsUnlocked = preset.fpsUnlocked;
                    FpsUnLock.SetFPSUnlock(fpsUnlocked);

                    if (UnknownCasting.Core.Cursor.CursorMod.Instance != null)
                    {
                        UnknownCasting.Core.Cursor.CursorMod.Instance.cursorEnabled = cursorEnabled;
                        if (!cursorEnabled)
                        {
                            UnknownCasting.Core.Cursor.CursorMod.Instance.DestroyPointer();
                        }
                    }

                    leaderboardX = preset.leaderboardX;
                    leaderboardY = preset.leaderboardY;
                    leaderboardScale = preset.leaderboardScale;
                    leaderboardOpacity = preset.leaderboardOpacity;
                    modernSpacing = preset.modernSpacing;
                    colorBoxSize = preset.colorBoxSize;
                    textScale = preset.textScale;
                    ApplyLeaderboardSettings();

                    showMiniMap = preset.showMiniMap;
                    miniMapSize = preset.miniMapSize;
                    miniMapZoom = preset.miniMapZoom;
                    miniMapOpacity = preset.miniMapOpacity;
                    miniMapX = preset.miniMapX;
                    miniMapY = preset.miniMapY;
                    MiniMap.ShowMiniMap = showMiniMap;
                    MiniMap.MiniMapSize = miniMapSize;
                    MiniMap.MiniMapZoom = miniMapZoom;
                    MiniMap.MiniMapOpacity = miniMapOpacity;
                    MiniMap.MiniMapPosition = new Vector2(miniMapX, miniMapY);

                    Logger.LogInfo($"Preset '{presetName}' loaded successfully!");
                }
                else
                {
                    Logger.LogError($"Preset file not found: {presetPath}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading preset: {ex.Message}");
            }
        }

        private void DeletePreset(string presetName)
        {
            try
            {
                string presetPath = Path.Combine(GetPresetsFolderPath(), presetName + ".json");

                if (File.Exists(presetPath))
                {
                    File.Delete(presetPath);
                    Logger.LogInfo($"Preset '{presetName}' deleted successfully!");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error deleting preset: {ex.Message}");
            }
        }

        [Serializable]
        public class PresetData
        {
            // Nametag settings
            public bool nametags = false;
            public bool selftags = false;
            public bool talkingindictor = false;
            public bool matchPlayerColor = false;
            public float tagssize = 1;
            public float tagsoffset = 0.3f;

            // Outline settings
            public float outlineWidth = 0.07f;
            public Color outlineColor = Color.black;
            public bool matchOutlineToPlayer = false;

            // Font settings
            public string selectedFont = "Pixel";

            // FPS tag settings
            public bool showFPS = false;
            public float fpsTagSize = 0.8f;
            public float fpsTagOffset = 0.2f;

            // Platform tag settings
            public bool showPlatforms = false;
            public float platformTagSize = 0.8f;
            public float platformTagOffset = 0.2f;

            // Speed tag settings
            public bool showSpeedTags = false;
            public float speedTagSize = 0.8f;
            public float speedTagOffset = 0.4f;

            // Camera settings
            public bool HandCam;
            public bool ShowCameraObj;
            public bool EnableSmoothing = true;
            public bool WASD = false;
            public float camX = 0f;
            public float camY = 0.6f;
            public float camZ = 2f;
            public float camFOV = 110f;
            public float nearClip = 0.03f;
            public float camSmoothing = 0.2f;
            public bool camEnableSmoothing = true;
            public int camSmoothingType = 1;
            public float camRotationSmoothing = 0.5f;
            public bool rollLock = false;
            public bool enableSelfRigSmoothing = false;
            public float selfRigLerpAmount = 0f;
            public float LerpValue = 20f;

            // Other settings
            public bool freecam = false;
            public bool cursorEnabled = false;
            public bool micToggleEnabled = false;
            public bool fpsUnlocked = false;
            public float fpsCap = 144f;
            public float leaderboardX = 22f;
            public float leaderboardY = 692f;
            public float leaderboardScale = 1.13f;
            public float leaderboardOpacity = 0.54f;
            public float modernSpacing = 8f;
            public float colorBoxSize = 14f;
            public float textScale = 1.10f;
            public bool showMiniMap = false;
            public float miniMapSize = 200f;
            public float miniMapZoom = 10f;
            public float miniMapOpacity = 1f;
            public float miniMapX = 808f;
            public float miniMapY = 387f;
        }

        private void ApplyLeaderboardSettings()
        {
            LeaderBoard.leaderboardX = leaderboardX;
            LeaderBoard.leaderboardY = leaderboardY;
            LeaderBoard.leaderboardScale = leaderboardScale;
            LeaderBoard.leaderboardOpacity = leaderboardOpacity;
            LeaderBoard.modernSpacing = modernSpacing;
            LeaderBoard.colorBoxSize = colorBoxSize;
            LeaderBoard.textScale = textScale;
        }

        void AssignKeybind(string target, Key newKey)
        {
            switch (target)
            {
                case "GuiKey":
                    GuiKeybinds.GuiKey = newKey;
                    break;
                case "ViewChangerKey":
                    GuiKeybinds.ViewChangerKey = newKey;
                    break;
                case "MicToggleKey":
                    GuiKeybinds.MicToggleKey = newKey;
                    break;
                case "CreateNestKey":
                    GuiKeybinds.CreateNestKey = newKey;
                    break;
                case "DestroyNestKey":
                    GuiKeybinds.DestroyNestKey = newKey;
                    break;
            }
        }

        private void DrawNameTagsTab()
        {
            float tabWidth = 350f;
            float buttonWidth = 300f;
            float buttonHeight = 15f;
            float buttonX = (tabWidth - buttonWidth) / 2f;
            float currentY = 30f;
            float spacing = 15f;

            Vector2 mousePosition = Event.current.mousePosition;

            Rect buttonRect = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            DrawToggleButton(ref nametags, "NameTags", buttonRect, ref mousePosition);
            currentY += buttonHeight + spacing;

            buttonRect = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            DrawToggleButton(ref selftags, "Self Tags", buttonRect, ref mousePosition);
            currentY += buttonHeight + spacing;

            // Match Color toggle
            buttonRect = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            DrawToggleButton(ref matchOutlineToPlayer, "Match Color", buttonRect, ref mousePosition);
            NameTags.matchPlayerColor = matchOutlineToPlayer;
            currentY += buttonHeight + spacing;

            // Combined font display and switch button
            Rect fontButtonRect = new Rect(buttonX, currentY, buttonWidth, 25f);
            float scale = fontButtonRect.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            fontButtonRect.width *= scale;
            fontButtonRect.height *= scale;
            fontButtonRect.x = (tabWidth - fontButtonRect.width) / 2f;

            if (GUI.Button(fontButtonRect, $"Font: {availableFonts[selectedFontIndex]}", label))
            {
                selectedFontIndex = (selectedFontIndex + 1) % availableFonts.Length;
                NameTags.SetFont(availableFonts[selectedFontIndex]);
            }
            currentY += 25f * scale + spacing;

            // Tag Size Slider - moved up under font
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "Tag Size: " + tagssize.ToString("F2"), label);
            currentY += 20f;
            tagssize = eUI.RoundedSlider(tagssize, 0.1f, 3f, new Rect(buttonX - 10, currentY, buttonWidth - 10, 12f), sliderBackTex, sliderFillTex);
            currentY += 20f;

            // Tag Offset Slider - moved up under tag size
            GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "Tag Offset: " + tagsoffset.ToString("F2"), label);
            currentY += 20f;
            tagsoffset = eUI.RoundedSlider(tagsoffset, 0.1f, 1f, new Rect(buttonX - 10, currentY, buttonWidth - 10, 12f), sliderBackTex, sliderFillTex);
            currentY += 20f;

            // Create scrollable area for remaining settings
            float scrollViewHeight = 280f;
            float totalContentHeight = 600f; // Approximate total content height
            Rect scrollViewRect = new Rect(0, currentY, tabWidth, scrollViewHeight);
            Rect scrollContentRect = new Rect(0, 0, tabWidth - 20, totalContentHeight);
            
            nameTagsScrollPosition = GUI.BeginScrollView(scrollViewRect, nameTagsScrollPosition, scrollContentRect);
            {
                float scrollY = currentY;
                
                // Outline settings
                GUI.Label(new Rect(buttonX, scrollY, buttonWidth, 20f), "OUTLINE SETTINGS", label);
                scrollY += 25f;

                // Outline Width Slider
                GUI.Label(new Rect(buttonX, scrollY, buttonWidth, 20f), "Outline Width: " + outlineWidth.ToString("F2"), label);
                scrollY += 20f;
                float newOutlineWidth = eUI.RoundedSlider(outlineWidth, 0.01f, 0.3f, new Rect(buttonX - 10, scrollY, buttonWidth - 10, 12f), sliderBackTex, sliderFillTex);
                if (newOutlineWidth != outlineWidth)
                {
                    outlineWidth = newOutlineWidth;
                    NameTags.outlineWidth = outlineWidth;
                    NameTags.UpdateAllOutlines();
                }
                scrollY += 20f;

                // Outline Color RGB Sliders
                GUI.Label(new Rect(buttonX, scrollY, buttonWidth, 20f), "Outline Red: " + Mathf.RoundToInt(outlineColor.r * 255), label);
                scrollY += 20f;
                float newRed = eUI.RoundedSlider(outlineColor.r, 0f, 1f, new Rect(buttonX - 10, scrollY, buttonWidth - 10, 12f), sliderBackTex, sliderFillTex);
                scrollY += 20f;

                GUI.Label(new Rect(buttonX, scrollY, buttonWidth, 20f), "Outline Green: " + Mathf.RoundToInt(outlineColor.g * 255), label);
                scrollY += 20f;
                float newGreen = eUI.RoundedSlider(outlineColor.g, 0f, 1f, new Rect(buttonX - 10, scrollY, buttonWidth - 10, 12f), sliderBackTex, sliderFillTex);
                scrollY += 20f;

                GUI.Label(new Rect(buttonX, scrollY, buttonWidth, 20f), "Outline Blue: " + Mathf.RoundToInt(outlineColor.b * 255), label);
                scrollY += 20f;
                float newBlue = eUI.RoundedSlider(outlineColor.b, 0f, 1f, new Rect(buttonX - 10, scrollY, buttonWidth - 10, 12f), sliderBackTex, sliderFillTex);
                scrollY += 20f;

                // Update outline color if any slider changed
                if (newRed != outlineColor.r || newGreen != outlineColor.g || newBlue != outlineColor.b)
                {
                    outlineColor = new Color(newRed, newGreen, newBlue, 1f);
                    NameTags.outlineColor = outlineColor;
                    NameTags.UpdateAllOutlines();
                }
                // Background and Animation settings removed
            }
            GUI.EndScrollView();
        }

        private void DrawToggleButtonNew(ref bool toggle, string labelText, Rect rect, ref Vector2 mousePosition)
        {
            float scaleFactor = rect.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            rect.width *= scaleFactor;
            rect.height *= scaleFactor;
            rect.x = (350f - rect.width) / 2f;

            string statusText = toggle ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>";
            if (GUI.Button(rect, $"{labelText} {statusText}", label))
                toggle = !toggle;
        }

        private void DrawToggleButton(ref bool toggle, string labelText, Rect rect, ref Vector2 mousePosition)
        {
            float scaleFactor = rect.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            rect.width *= scaleFactor;
            rect.height *= scaleFactor;
            rect.x = (350f - rect.width) / 2f;

            if (GUI.Button(rect, toggle ? $"{labelText}: <color=green>ON</color>" : $"{labelText}: <color=red>OFF</color>", label))
                toggle = !toggle;
        }

        private void DrawCameraSettingsTab()
        {
            float tabWidth = 350f;
            float buttonWidth = 300f;
            float buttonHeight = 30f;
            float buttonX = (tabWidth - buttonWidth) / 2f;
            float currentY = 20f;
            float spacing = 5f;

            Vector2 mousePosition = Event.current.mousePosition;

            // Manual Camera Controls button
            Rect buttonRect = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            float scaleFactor = buttonRect.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            buttonRect.width *= scaleFactor;
            buttonRect.height *= scaleFactor;
            buttonRect.x = (tabWidth - buttonRect.width) / 2f;
            if (GUI.Button(buttonRect, manualCameraControls ? "Manual Controls <color=green>[ON]</color>" : "Manual Controls <color=red>[OFF]</color>", label))
            {
                manualCameraControls = !manualCameraControls;
                CameraUpdater.SetManualControlsEnabled(manualCameraControls);
            }
            currentY += buttonHeight * scaleFactor + spacing;

            // Manual Move Speed slider (only show when manual controls are enabled)
            if (manualCameraControls)
            {
                GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "Manual Move Speed: " + manualMoveSpeed.ToString("F2"), label);
                currentY += 20f;
                manualMoveSpeed = eUI.RoundedSlider(
                    manualMoveSpeed, 0.01f, 1f,
                    new Rect(buttonX - 5, currentY, buttonWidth, 12),
                    sliderBackTex, sliderFillTex
                );
                CameraUpdater.SetManualMoveSpeed(manualMoveSpeed);
                currentY += 18f;
            }

            // FOV slider
            if (label != null)
            {
                GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "FOV: " + CameraUpdater.FOV.ToString("F0"), label);
            }
            currentY += 20f;

            CameraUpdater.FOV = eUI.RoundedSlider(
                CameraUpdater.FOV, 70, 200,
                new Rect(buttonX - 5, currentY, buttonWidth, 12),
                sliderBackTex, sliderFillTex
            );
            currentY += 18f;

            // Near Clip slider
            if (label != null)
            {
                GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "Near Clip: " + nearClip.ToString("F3"), label);
            }
            currentY += 20f;

            nearClip = eUI.RoundedSlider(
                nearClip, 0.001f, 0.5f,
                new Rect(buttonX - 5, currentY, buttonWidth, 12),
                sliderBackTex, sliderFillTex
            );
            CameraUpdater.nearClip = nearClip;
            currentY += 18f;

            // Offsets label
            if (offsetLabel != null)
            {
                GUI.Label(new Rect(buttonX, currentY, buttonWidth, 20f), "Offsets: X: " + CameraUpdater.x.ToString("F1") + " Y: " + CameraUpdater.y.ToString("F1") + " Z: " + CameraUpdater.z.ToString("F1"), offsetLabel);
            }
            currentY += 20f;

            // Offset sliders
            OffsetSliders(new Rect(buttonX - 5, currentY, buttonWidth, 50f));
            currentY += 55f;

            // Smoothing toggle
            buttonRect = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            scaleFactor = buttonRect.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            buttonRect.width *= scaleFactor;
            buttonRect.height *= scaleFactor;
            buttonRect.x = (tabWidth - buttonRect.width) / 2f;
            string smoothStatus = EnableSmoothing ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>";
            if (GUI.Button(buttonRect, $"Smoothing {smoothStatus}", label))
            {
                EnableSmoothing = !EnableSmoothing;
                CameraUpdater.enableSmoothing = EnableSmoothing;
            }
            currentY += buttonHeight * scaleFactor + spacing;

            // Rig Settings collapsible section
            string rigArrow = rigSettingsExpanded ? "▼ " : "► ";
            Rect rigBtnRect = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            float rigScale = rigBtnRect.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            rigBtnRect.width *= rigScale;
            rigBtnRect.height *= rigScale;
            rigBtnRect.x = (tabWidth - rigBtnRect.width) / 2f;
            if (GUI.Button(rigBtnRect, rigArrow + "Rig Settings", label))
            {
                rigSettingsExpanded = !rigSettingsExpanded;
            }
            currentY += buttonHeight * rigScale + spacing;

            if (rigSettingsExpanded)
            {
                float contentX = buttonX + 10f;
                float contentWidth = buttonWidth - 20f;

                // Rig Lerp
                GUI.Label(new Rect(contentX, currentY, contentWidth, 20f), "Rig Lerp: " + RigLerp.targetLerpValue.ToString("F0") + "%", label);
                currentY += 20f;
                RigLerp.targetLerpValue = eUI.RoundedSlider(
                    RigLerp.targetLerpValue, 0f, 100f,
                    new Rect(contentX - 5, currentY, contentWidth, 12),
                    sliderBackTex, sliderFillTex
                );
                currentY += 18f;

                // Self-Rig Smoothing
                GUI.Label(new Rect(contentX, currentY, contentWidth, 20f), "Self-Rig Smoothing", label);
                currentY += 20f;
                Rect selfRigBtnRect = new Rect(contentX, currentY, contentWidth, buttonHeight);
                string srStatus = CameraUpdater.enableSelfRigSmoothing ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>";
                if (GUI.Button(selfRigBtnRect, $"Enabled {srStatus}", label))
                {
                    CameraUpdater.enableSelfRigSmoothing = !CameraUpdater.enableSelfRigSmoothing;
                }
                currentY += buttonHeight + spacing;

                // Self-Rig Amount
                GUI.Label(new Rect(contentX, currentY, contentWidth, 20f), "Self-Rig Amount: " + CameraUpdater.selfRigLerpAmount.ToString("F2"), label);
                currentY += 20f;
                CameraUpdater.selfRigLerpAmount = eUI.RoundedSlider(
                    CameraUpdater.selfRigLerpAmount, -5f, 5f,
                    new Rect(contentX - 5, currentY, contentWidth, 12),
                    sliderBackTex, sliderFillTex
                );
                currentY += 18f;

                // Cam Pos Smoothing
                GUI.Label(new Rect(contentX, currentY, contentWidth, 20f), "Cam Pos Smoothing: " + CameraUpdater.positionSmoothing.ToString("F2"), label);
                currentY += 20f;
                CameraUpdater.positionSmoothing = eUI.RoundedSlider(
                    CameraUpdater.positionSmoothing, 0.01f, 2f,
                    new Rect(contentX - 5, currentY, contentWidth, 12),
                    sliderBackTex, sliderFillTex
                );
                currentY += 18f;

                // Cam Rot Smoothing
                GUI.Label(new Rect(contentX, currentY, contentWidth, 20f), "Cam Rot Smoothing: " + CameraUpdater.rotationSmoothing.ToString("F2"), label);
                currentY += 20f;
                CameraUpdater.rotationSmoothing = eUI.RoundedSlider(
                    CameraUpdater.rotationSmoothing, 0.01f, 2f,
                    new Rect(contentX - 5, currentY, contentWidth, 12),
                    sliderBackTex, sliderFillTex
                );
                currentY += 18f;

                // Roll Lock
                Rect rollLockRect = new Rect(contentX, currentY, contentWidth, buttonHeight);
                string rlStatus = CameraUpdater.rollLock ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>";
                if (GUI.Button(rollLockRect, $"Roll Lock {rlStatus}", label))
                {
                    CameraUpdater.rollLock = !CameraUpdater.rollLock;
                }
                currentY += buttonHeight + spacing;
            }

            // View Mode cycling (click label to cycle)
            string vmDisplay = viewMode == 0 ? "Third Person" : (viewMode == 1 ? "Front Third" : "First Person");
            Rect vmRect = new Rect(buttonX, currentY, buttonWidth, 20f);
            if (GUI.Button(vmRect, "View Mode: " + vmDisplay, label))
            {
                viewMode = (viewMode + 1) % 3;
                // Apply immediately
                CameraUpdater.FP = (viewMode == 2);
                CameraUpdater.TPFront = (viewMode == 1);
            }
            currentY += 25f;

            // Follow Head button
            buttonRect = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            scaleFactor = buttonRect.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            buttonRect.width *= scaleFactor;
            buttonRect.height *= scaleFactor;
            buttonRect.x = (tabWidth - buttonRect.width) / 2f;
            string followStatus = CameraUpdater.followhead ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>";
            if (GUI.Button(buttonRect, $"Follow Head {followStatus}", label))
                CameraUpdater.followhead = !CameraUpdater.followhead;
            currentY += buttonHeight * scaleFactor + spacing;

            // Auto Pilot button
            buttonRect = new Rect(buttonX, currentY, buttonWidth, buttonHeight);
            scaleFactor = buttonRect.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            buttonRect.width *= scaleFactor;
            buttonRect.height *= scaleFactor;
            buttonRect.x = (tabWidth - buttonRect.width) / 2f;
            string apStatus = AutoSpec.AutoPilotEnabled ? "<color=green>[ON]</color>" : "<color=red>[OFF]</color>";
            if (GUI.Button(buttonRect, $"Auto Pilot {apStatus}", label))
                AutoSpec.ToggleAutoPilot();
            currentY += buttonHeight * scaleFactor + spacing;
        }

        void OffsetSliders(Rect rect)
        {
            CameraUpdater.x = eUI.RoundedSlider(CameraUpdater.x, -5, 5, new Rect(rect.x, rect.y, rect.width, 12), sliderBackTex, sliderFillTex, grayTex, true, 10, 1);
            rect.y += 15;
            CameraUpdater.y = eUI.RoundedSlider(CameraUpdater.y, -5, 5, new Rect(rect.x, rect.y, rect.width, 12), sliderBackTex, sliderFillTex, grayTex, true, 10, 1);
            rect.y += 15;
            CameraUpdater.z = eUI.RoundedSlider(CameraUpdater.z, -10, 5, new Rect(rect.x, rect.y, rect.width, 12), sliderBackTex, sliderFillTex, grayTex, true, 10, 1);
        }

        private void CameraMods()
        {
            float tabWidth = 350f;
            float controlWidth = 300f;
            float controlHeight = 20f;
            float controlX = (tabWidth - controlWidth) / 2f;
            float currentY = 30f;
            float spacing = 15f;

            Vector2 mousePosition = Event.current.mousePosition;
            
            // ===== WASD MOVEMENT =====
            Rect WASDButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            float scale = WASDButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            WASDButton.width *= scale;
            WASDButton.height *= scale;
            WASDButton.x = (tabWidth - WASDButton.width) / 2f;
            if (GUI.Button(WASDButton, WASD ? "WASD <color=green>[ON]</color>" : "WASD <color=red>[OFF]</color>", label))
                WASD = !WASD;
            currentY += controlHeight * scale + spacing;
            
            // ===== WASD SPEED SLIDER =====
            if (WASD)
            {
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "WASD Speed: " + UnknownCasting.Core.WASD.maxSpeed.ToString("F1"), label);
                currentY += 20f;
                float newSpeed = eUI.RoundedSlider(UnknownCasting.Core.WASD.maxSpeed, 1f, 20f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex, grayTex, true, 10, 1);
                if (newSpeed != UnknownCasting.Core.WASD.maxSpeed)
                {
                    UnknownCasting.Core.WASD.maxSpeed = newSpeed;
                }
                currentY += 20f;
                
                // ===== WASD SMOOTHING SLIDER =====
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "WASD Smoothing: " + (UnknownCasting.Core.WASD.smoothing * 100).ToString("F0") + "%", label);
                currentY += 20f;
                float newSmoothing = eUI.RoundedSlider(UnknownCasting.Core.WASD.smoothing, 0.01f, 1f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex, grayTex, true, 10, 1);
                if (newSmoothing != UnknownCasting.Core.WASD.smoothing)
                {
                    UnknownCasting.Core.WASD.smoothing = newSmoothing;
                }
                currentY += 20f;
            }
            
            // ===== CURSOR =====
            Rect cursorButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = cursorButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            cursorButton.width *= scale;
            cursorButton.height *= scale;
            cursorButton.x = (tabWidth - cursorButton.width) / 2f;
            if (GUI.Button(cursorButton, cursorEnabled ? "Cursor <color=green>[ON]</color>" : "Cursor <color=red>[OFF]</color>", label))
            {
                cursorEnabled = !cursorEnabled;
                if (UnknownCasting.Core.Cursor.CursorMod.Instance != null)
                {
                    UnknownCasting.Core.Cursor.CursorMod.Instance.cursorEnabled = cursorEnabled;
                    if (!cursorEnabled)
                        UnknownCasting.Core.Cursor.CursorMod.Instance.DestroyPointer();
                }
            }
            currentY += controlHeight * scale + spacing;

            // ===== FPS UNLOCK =====
            Rect fpsUnlockButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = fpsUnlockButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            fpsUnlockButton.width *= scale;
            fpsUnlockButton.height *= scale;
            fpsUnlockButton.x = (tabWidth - fpsUnlockButton.width) / 2f;
            if (GUI.Button(fpsUnlockButton, fpsUnlocked ? "FPS Unlock <color=green>[ON]</color>" : "FPS Unlock <color=red>[OFF]</color>", label))
            {
                fpsUnlocked = !fpsUnlocked;
                FpsUnLock.SetFPSUnlock(fpsUnlocked);
            }
            currentY += controlHeight * scale + spacing;

            // ===== FPS CAP ENABLE/DISABLE =====
            Rect fpsCapToggle = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = fpsCapToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            fpsCapToggle.width *= scale;
            fpsCapToggle.height *= scale;
            fpsCapToggle.x = (tabWidth - fpsCapToggle.width) / 2f;
            if (GUI.Button(fpsCapToggle, fpsCapEnabled ? "Cap FPS <color=green>[ON]</color>" : "Cap FPS <color=red>[OFF]</color>", label))
            {
                fpsCapEnabled = !fpsCapEnabled;
            }
            currentY += controlHeight * scale + spacing;

            // ===== FPS CAP SLIDER =====
            if (fpsCapEnabled)
            {
                if (label != null)
                {
                    GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Cap FPS: " + fpsCap.ToString("F0"), label);
                }
                currentY += 20f;
                    fpsCap = eUI.RoundedSlider(fpsCap, 30f, 144f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex, grayTex, true, 10, 1);
                currentY += 18f;
            }

            // ===== MIC TOGGLE =====
            Rect micButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = micButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            micButton.width *= scale;
            micButton.height *= scale;
            micButton.x = (tabWidth - micButton.width) / 2f;
            if (GUI.Button(micButton, micToggleEnabled ? "Mic Toggle <color=green>[ON]</color>" : "Mic Toggle <color=red>[OFF]</color>", label))
            {
                micToggleEnabled = !micToggleEnabled;
                if (UnknownCasting.Core.MicToggle.MicToggle.Instance != null)
                {
                    UnknownCasting.Core.MicToggle.MicToggle.Instance.SetMicToggleEnabled(micToggleEnabled);
                }
            }
            currentY += controlHeight * scale + spacing;
            
            // ===== FLY TOGGLE =====
            Rect flyButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            float flyScale = flyButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            flyButton.width *= flyScale;
            flyButton.height *= flyScale;
            flyButton.x = (tabWidth - flyButton.width) / 2f;
            if (GUI.Button(flyButton, UnknownCasting.Core.WASD.fly ? "Fly <color=green>[ON]</color>" : "Fly <color=red>[OFF]</color>", label))
            {
                UnknownCasting.Core.WASD.fly = !UnknownCasting.Core.WASD.fly;
            }
            currentY += controlHeight * flyScale + spacing;
            
            // ===== FLY CONTROLS (Speed, Smoothing, Sensitivity) =====
            if (UnknownCasting.Core.WASD.fly)
            {
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Fly Speed: " + UnknownCasting.Core.WASD.maxSpeed.ToString("F1"), label);
                currentY += 20f;
                float newFlySpeed = eUI.RoundedSlider(UnknownCasting.Core.WASD.maxSpeed, 1f, 20f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex, grayTex, true, 10, 1);
                if (newFlySpeed != UnknownCasting.Core.WASD.maxSpeed)
                {
                    UnknownCasting.Core.WASD.maxSpeed = newFlySpeed;
                }
                currentY += 20f;
                
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Fly Smoothing: " + (UnknownCasting.Core.WASD.smoothing * 100).ToString("F0") + "%", label);
                currentY += 20f;
                float newFlySmoothing = eUI.RoundedSlider(UnknownCasting.Core.WASD.smoothing, 0.01f, 1f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex, grayTex, true, 10, 1);
                if (newFlySmoothing != UnknownCasting.Core.WASD.smoothing)
                {
                    UnknownCasting.Core.WASD.smoothing = newFlySmoothing;
                }
                currentY += 20f;
                
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Fly Sensitivity: " + UnknownCasting.Core.WASD.sensitivity.ToString("F2"), label);
                currentY += 20f;
                float newFlySensitivity = eUI.RoundedSlider(UnknownCasting.Core.WASD.sensitivity, 0.1f, 3f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex, grayTex, true, 10, 1);
                if (newFlySensitivity != UnknownCasting.Core.WASD.sensitivity)
                {
                    UnknownCasting.Core.WASD.sensitivity = newFlySensitivity;
                }
                currentY += 20f;
                
                GUI.Label(new Rect(controlX, currentY, controlWidth, 40f), "Controls: W/S/A/D = Move, Space = Up, Ctrl = Down, E/Q = Rotate (Hold RMB)", label);
                currentY += 25f;
            }
        }

        private void DrawPhotonNetworkTab()
        {
            float tabWidth = 350f;
            float controlWidth = 300f;
            float controlHeight = 20f;
            float controlX = (tabWidth - controlWidth) / 2f;
            float currentY = 30f;
            float spacing = 15f;
            Vector2 mousePosition = Event.current.mousePosition;

            // Add code visibility toggle button
            Rect codeVisibilityRect = new Rect(controlX, currentY, controlWidth, controlHeight);
            float scale = codeVisibilityRect.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            codeVisibilityRect.width *= scale;
            codeVisibilityRect.height *= scale;
            codeVisibilityRect.x = (tabWidth - codeVisibilityRect.width) / 2f;

            if (GUI.Button(codeVisibilityRect, hideRoomCode ? "Show Room Code <color=green>[ON]</color>" : "Hide Room Code <color=red>[OFF]</color>", label))
                hideRoomCode = !hideRoomCode;

            currentY += controlHeight * scale + spacing;

            // Room code field
            Rect roomField = new Rect(controlX - 8, currentY, controlWidth, controlHeight);

            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                font = silkScreen,
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            if (hideRoomCode)
            {
                GUI.DrawTexture(roomField, sliderBackTex);
                string displayText = new string('#', roomtojoin.Length);
                GUI.Label(roomField, displayText, textFieldStyle);

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None)
                {
                    if (Event.current.keyCode == KeyCode.Backspace && roomtojoin.Length > 0)
                    {
                        roomtojoin = roomtojoin.Substring(0, roomtojoin.Length - 1);
                        Event.current.Use();
                    }
                    else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                    {
                        if (PhotonNetwork.InRoom)
                            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomtojoin.ToUpper(), JoinType.Solo);
                        Event.current.Use();
                    }
                    else if (Event.current.character != 0 && char.IsLetterOrDigit(Event.current.character))
                    {
                        roomtojoin += Event.current.character.ToString().ToUpper();
                        Event.current.Use();
                    }
                }
            }
            else
            {
                roomtojoin = eUI.TextField(roomtojoin.ToUpper(), roomField, sliderBackTex, 10);
            }

            currentY += controlHeight + spacing;

            Rect joinButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = joinButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            joinButton.width *= scale;
            joinButton.height *= scale;
            joinButton.x = (tabWidth - joinButton.width) / 2f;

            string joinButtonText = PhotonNetwork.InRoom ?
                "LEAVE ROOM" :
                "JOIN ROOM:\n" + (hideRoomCode ? "####" : roomtojoin.ToUpper());

            if (GUI.Button(joinButton, joinButtonText, label))
            {
                if (!PhotonNetwork.InRoom)
                    PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomtojoin.ToUpper(), JoinType.Solo);
                else
                    PhotonNetwork.Disconnect();
            }
            currentY += controlHeight * scale + spacing + 5;

            // Name field
            Rect nameField = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            nametochange = eUI.TextField(nametochange.ToUpper(), nameField, sliderBackTex, 10);
            currentY += controlHeight + spacing;

            Rect nameButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = nameButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            nameButton.width *= scale;
            nameButton.height *= scale;
            nameButton.x = (tabWidth - nameButton.width) / 2f;

            if (GUI.Button(nameButton, "CHANGE NAME TO:\n" + nametochange.ToUpper(), label))
            {
                NameChanger.ChangeName(nametochange.ToUpper());
            }
            currentY += controlHeight * scale + spacing + 10;
            
            // ===== GAMEMODE DROPDOWN =====
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "GameMode:", label);
            currentY += 22f;
            
            string[] gameModeNames = { "Casual", "Infection", "SuperInfection", "SuperCasual" };
            int[] gameModeValues = { 0, 1, 11, 12 };
            
            Rect gameModeButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            bool gmHover = gameModeButton.Contains(mousePosition);
            
            if (gmHover && Event.current.type == EventType.MouseDown)
            {
                gameModeDropdownOpen = !gameModeDropdownOpen;
                timeChangerDropdownOpen = false;
            }
            
            GUI.Button(gameModeButton, gameModeNames[currentGameModeIndex] + (gameModeDropdownOpen ? " ▲" : " ▼"), label);
            currentY += controlHeight;
            
            if (gameModeDropdownOpen)
            {
                float dropdownHeight = gameModeNames.Length * controlHeight;
                Rect dropdownRect = new Rect(controlX, currentY, controlWidth, dropdownHeight);
                GUI.DrawTexture(dropdownRect, sliderBackTex);
                
                for (int i = 0; i < gameModeNames.Length; i++)
                {
                    Rect optionRect = new Rect(controlX + 5, currentY + i * controlHeight, controlWidth - 10, controlHeight);
                    if (GUI.Button(optionRect, gameModeNames[i], label))
                    {
                        currentGameModeIndex = i;
                        GorillaComputer.instance.SetGameModeWithoutButton(((GameModeType)gameModeValues[i]).ToString());
                        gameModeDropdownOpen = false;
                    }
                }
                currentY += dropdownHeight;
            }
            
            if ((gameModeDropdownOpen || timeChangerDropdownOpen) && Event.current.type == EventType.MouseDown)
            {
                if (!gameModeButton.Contains(mousePosition) && !gmHover)
                {
                    gameModeDropdownOpen = false;
                    timeChangerDropdownOpen = false;
                }
            }
            currentY += spacing;
            
            // Current game mode display
            try {
                var gc = GorillaComputer.instance;
                if (gc != null && gc.currentGameMode != null)
                {
                    GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Current: " + gc.currentGameMode, label);
                }
                else
                {
                    GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Current: None", label);
                }
            } catch 
            {
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Current: None", label);
            }
            currentY += controlHeight + spacing + 10;
            
            // ===== TIME CHANGER DROPDOWN =====
            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Time Changer:", label);
            currentY += 22f;
            
            string[] timeChangerOptions = { "Normal", "Make Time Day", "Make Time Night", "Clear Weather", "Rainy Weather" };
            int currentTimeChangerIndex = timeOfDayIndex * 2 + weatherIndex;
            
            Rect timeChangerButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            bool tcHover = timeChangerButton.Contains(mousePosition);
            
            if (tcHover && Event.current.type == EventType.MouseDown)
            {
                timeChangerDropdownOpen = !timeChangerDropdownOpen;
                gameModeDropdownOpen = false;
            }
            
            GUI.Button(timeChangerButton, timeChangerOptions[currentTimeChangerIndex] + (timeChangerDropdownOpen ? " ▲" : " ▼"), label);
            currentY += controlHeight;
            
            if (timeChangerDropdownOpen)
            {
                float dropdownHeight = timeChangerOptions.Length * controlHeight;
                Rect dropdownRect = new Rect(controlX, currentY, controlWidth, dropdownHeight);
                GUI.DrawTexture(dropdownRect, sliderBackTex);
                
                for (int i = 0; i < timeChangerOptions.Length; i++)
                {
                    Rect optionRect = new Rect(controlX + 5, currentY + i * controlHeight, controlWidth - 10, controlHeight);
                    if (GUI.Button(optionRect, timeChangerOptions[i], label))
                    {
                        ApplyTimeChanger(i);
                        timeChangerDropdownOpen = false;
                    }
                }
                currentY += dropdownHeight;
            }
            currentY += spacing;
        }

        private void DrawAntiCheatSection()
        {
            float tabWidth = 350f;
            float controlWidth = 300f;
            float controlHeight = 20f;
            float controlX = (tabWidth - controlWidth) / 2f;
            float currentY = 30f;
            float spacing = 15f;
            Vector2 mousePosition = Event.current.mousePosition;

            Rect tabToggleButton = new Rect(controlX, currentY, controlWidth, controlHeight);
            float scale = tabToggleButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            tabToggleButton.width *= scale;
            tabToggleButton.height *= scale;
            tabToggleButton.x = (tabWidth - tabToggleButton.width) / 2f;

            string tabText = antiCheatSelectedTab switch
            {
                0 => "<color=purple>FPS Tags Settings</color>",
                1 => "<color=purple>Platform Tags Settings</color>",
                2 => "<color=purple>Speed Tags Settings</color>",
                3 => "<color=purple>Ping Tags Settings</color>",
                _ => "<color=purple>FPS Tags Settings</color>"
            };

            if (GUI.Button(tabToggleButton, tabText, label))
            {
                antiCheatSelectedTab = (antiCheatSelectedTab + 1) % 4;
            }
            currentY += controlHeight * scale + spacing;

            if (antiCheatSelectedTab == 0)
            {
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "FPS Tag Size: " + NameTags.fpsTagSize.ToString("F2"), label);
                currentY += 20f;
                NameTags.fpsTagSize = eUI.RoundedSlider(NameTags.fpsTagSize, 0.1f, 2f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
                currentY += 20f;

                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "FPS Tag Offset: " + NameTags.fpsTagOffset.ToString("F2"), label);
                currentY += 20f;
                NameTags.fpsTagOffset = eUI.RoundedSlider(NameTags.fpsTagOffset, -0.5f, 0.5f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
                currentY += 20f;

                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Match FPS Color: " + (NameTags.matchFPSColor ? "On" : "Off"), label);
                currentY += 20f;
            }
            else if (antiCheatSelectedTab == 1)
            {
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Platform Tag Size: " + NameTags.platformTagSize.ToString("F2"), label);
                currentY += 20f;
                NameTags.platformTagSize = eUI.RoundedSlider(NameTags.platformTagSize, 0.1f, 2f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
                currentY += 20f;

                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Platform Tag Offset: " + NameTags.platformTagOffset.ToString("F2"), label);
                currentY += 20f;
                NameTags.platformTagOffset = eUI.RoundedSlider(NameTags.platformTagOffset, -0.5f, 0.5f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
                currentY += 20f;

                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Match Platform Color: " + (NameTags.matchPlatformColor ? "On" : "Off"), label);
                currentY += 20f;
            }
            else if (antiCheatSelectedTab == 2)
            {
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Speed Tag Size: " + NameTags.speedTagSize.ToString("F2"), label);
                currentY += 20f;
                NameTags.speedTagSize = eUI.RoundedSlider(NameTags.speedTagSize, 0.1f, 2f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
                currentY += 20f;

                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Speed Tag Offset: " + NameTags.speedTagOffset.ToString("F2"), label);
                currentY += 20f;
                NameTags.speedTagOffset = eUI.RoundedSlider(NameTags.speedTagOffset, -0.5f, 0.5f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
                currentY += 20f;

                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Match Speed Color: " + (NameTags.matchSpeedColor ? "On" : "Off"), label);
                currentY += 20f;
            }
            else if (antiCheatSelectedTab == 3)
            {
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Ping Tag Size: " + NameTags.pingTagSize.ToString("F2"), label);
                currentY += 20f;
                NameTags.pingTagSize = eUI.RoundedSlider(NameTags.pingTagSize, 0.1f, 2f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
                currentY += 20f;

                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Ping Tag Offset: " + NameTags.pingTagOffset.ToString("F2"), label);
                currentY += 20f;
                NameTags.pingTagOffset = eUI.RoundedSlider(NameTags.pingTagOffset, -0.5f, 0.5f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
                currentY += 20f;
            }

            // Toggle buttons for each tag type
            Rect platforms = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            scale = platforms.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            platforms.width *= scale;
            platforms.height *= scale;
            platforms.x = (tabWidth - platforms.width) / 2f;

            if (GUI.Button(platforms, showPlatforms ? "Show Platform Tags <color=green>[ON]</color>" : "Show Platform Tags <color=red>[OFF]</color>", label))
                showPlatforms = !showPlatforms;

            currentY += controlHeight * scale + spacing;

            Rect FPS = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            scale = FPS.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            FPS.width *= scale;
            FPS.height *= scale;
            FPS.x = (tabWidth - FPS.width) / 2f;

            if (GUI.Button(FPS, showFPS ? "Show FPS Nametags <color=green>[ON]</color>" : "Show FPS Nametags <color=red>[OFF]</color>", label))
                showFPS = !showFPS;

            currentY += controlHeight * scale + spacing;

            Rect speedToggle = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            scale = speedToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            speedToggle.width *= scale;
            speedToggle.height *= scale;
            speedToggle.x = (tabWidth - speedToggle.width) / 2f;

            if (GUI.Button(speedToggle, showSpeedTags ? "Show Speed Tags <color=green>[ON]</color>" : "Show Speed Tags <color=red>[OFF]</color>", label))
                showSpeedTags = !showSpeedTags;

            currentY += controlHeight * scale + spacing;

            // Show Ping Tags toggle
            Rect pingToggle = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            scale = pingToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            pingToggle.width *= scale;
            pingToggle.height *= scale;
            pingToggle.x = (tabWidth - pingToggle.width) / 2f;

            if (GUI.Button(pingToggle, NameTags.showPingTags ? "Show Ping Tags <color=green>[ON]</color>" : "Show Ping Tags <color=red>[OFF]</color>", label))
                NameTags.showPingTags = !NameTags.showPingTags;

            currentY += controlHeight * scale + spacing;

            // Auto Outline Width toggle - outline gets wider as tags get smaller
            Rect autoOutlineToggle = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            scale = autoOutlineToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            autoOutlineToggle.width *= scale;
            autoOutlineToggle.height *= scale;
            autoOutlineToggle.x = (tabWidth - autoOutlineToggle.width) / 2f;

            if (GUI.Button(autoOutlineToggle, autoOutlineWidth ? "Auto Outline Width <color=green>[ON]</color>" : "Auto Outline Width <color=red>[OFF]</color>", label))
            {
                autoOutlineWidth = !autoOutlineWidth;
                NameTags.autoOutlineWidth = autoOutlineWidth;
                NameTags.UpdateAllOutlines();
            }

            // Update NameTags static variables from local variables
            NameTags.showFPSTags = showFPS;
            NameTags.showPlatformTags = showPlatforms;
            NameTags.showSpeedTags = showSpeedTags;
            NameTags.autoOutlineWidth = autoOutlineWidth;
            NameTags.outlineWidth = outlineWidth;
            NameTags.outlineColor = outlineColor;
        }

        private void DrawExtraFeaturesTab()
        {
            float tabWidth = 350f;
            float controlWidth = 300f;
            float controlHeight = 30;
            float controlX = (tabWidth - controlWidth) / 2f;
            float currentY = 30f;
            float spacing = 15f;

            Vector2 mousePosition = Event.current.mousePosition;

            // ===== FREE CAM =====
            Rect Freecam = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            float scale = Freecam.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            Freecam.width *= scale;
            Freecam.height *= scale;
            Freecam.x = (tabWidth - Freecam.width) / 2f;
            if (GUI.Button(Freecam, freecam ? "Free Cam <color=green>[ON]</color>" : "Free Cam <color=red>[OFF]</color>", label))
                freecam = !freecam;

            currentY += controlHeight * scale + spacing;

            // ===== FREECAM SPEED =====
            if (label != null)
            {
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Freecam Speed: " + CameraUpdater.moveSpeed.ToString("F1"), label);
            }
            currentY += 20f;
            CameraUpdater.moveSpeed = eUI.RoundedSlider(
                CameraUpdater.moveSpeed, 1f, 50f,
                new Rect(controlX - 5, currentY, controlWidth, 12),
                sliderBackTex, sliderFillTex, grayTex,
                true, 10, 1
            );
            currentY += 18f;

            // ===== FREECAM SENSITIVITY =====
            if (label != null)
            {
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Freecam Sensitivity: " + CameraUpdater.lookSensitivity.ToString("F2"), label);
            }
            currentY += 20f;
            CameraUpdater.lookSensitivity = eUI.RoundedSlider(
                CameraUpdater.lookSensitivity, 0.01f, 1f,
                new Rect(controlX - 5, currentY, controlWidth, 12),
                sliderBackTex, sliderFillTex, grayTex,
                true, 10, 2
            );
            currentY += 18f;

             // ===== FREECAM SMOOTHING =====
             if (label != null)
             {
                 GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Freecam Smoothing: " + CameraUpdater.positionSmoothing.ToString("F2"), label);
             }
             currentY += 20f;
             CameraUpdater.positionSmoothing = eUI.RoundedSlider(
                 CameraUpdater.positionSmoothing, 0.01f, 1f,
                 new Rect(controlX - 5, currentY, controlWidth, 12),
                 sliderBackTex, sliderFillTex, grayTex,
                 true, 10, 2
             );
             currentY += 18f;

            // ===== NESTS SECTION =====
            currentY += spacing;

            // Toggle Nests visibility
            //Rect nestToggle = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            //float scale = nestToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            //nestToggle.width *= scale;
            //nestToggle.height *= scale;
            //nestToggle.x = (tabWidth - nestToggle.width) / 2f;
            //if (GUI.Button(nestToggle, "<color=yellow>Nests: Toggle Visibility (T)</color>", label))
            //    UnknownCasting.Core.CameraUpdater.ToggleNestVisibility();
            //currentY += controlHeight * scale + spacing;

            // Save current position as nest
            //Rect saveNestButton = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            //scale = saveNestButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            //saveNestButton.width *= scale;
            //saveNestButton.height *= scale;
            //saveNestButton.x = (tabWidth - saveNestButton.width) / 2f;
            //if (GUI.Button(saveNestButton, "<color=cyan>Nests: Save Position (N)</color>", label))
            //    UnknownCasting.Core.CameraUpdater.SaveNest();
            //currentY += controlHeight * scale + spacing;

            // Cycle through saved nests
            //Rect cycleNestButton = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            //scale = cycleNestButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            //cycleNestButton.width *= scale;
            //cycleNestButton.height *= scale;
            //cycleNestButton.x = (tabWidth - cycleNestButton.width) / 2f;
            //if (GUI.Button(cycleNestButton, "<color=cyan>Nests: Cycle (M)</color>", label))
            //    UnknownCasting.Core.CameraUpdater.CycleNest();
            //currentY += controlHeight * scale + spacing;

            // Delete latest nest
            //Rect deleteLatestButton = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            //scale = deleteLatestButton.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            //deleteLatestButton.width *= scale;
            //deleteLatestButton.height *= scale;
            //deleteLatestButton.x = (tabWidth - deleteLatestButton.width) / 2f;
            //if (GUI.Button(deleteLatestButton, "<color=orange>Nests: Delete Latest</color>", label))
            //    UnknownCasting.Core.CameraUpdater.DeleteLatestNest();
            //currentY += controlHeight * scale + spacing;

            // Clear all nests
            //Rect clearNests = new Rect(controlX - 8, currentY, controlWidth, controlHeight);
            //scale = clearNests.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            //clearNests.width *= scale;
            //clearNests.height *= scale;
            //clearNests.x = (tabWidth - clearNests.width) / 2f;
            //if (GUI.Button(clearNests, "<color=red>Nests: Clear All (Del)</color>", label))
            //    UnknownCasting.Core.CameraUpdater.ClearAllNests();
        }

        void DrawKeyBindsTab()
        {
            float tabWidth = 350f;
            float controlWidth = 300f;
            float controlHeight = 20f;
            float controlX = (tabWidth - controlWidth) / 2f;
            float currentY = 20f;

            if (label == null)
            {
                label = new GUIStyle(GUI.skin.button);
                label.fontSize = 16;
            }

            Rect rect1 = new Rect(controlX, currentY, controlWidth, controlHeight);
            if (GUI.Button(rect1, "GUI Key: " + GuiKeybinds.GuiKey.ToString(), label))
            {
                awaitingKey = true;
                currentKeyTarget = "GuiKey";
            }
            currentY += controlHeight + 8f;

            Rect viewChangerRect = new Rect(controlX, currentY, controlWidth, controlHeight);
            if (GUI.Button(viewChangerRect, "View Changer Key: " + GuiKeybinds.ViewChangerKey.ToString(), label))
            {
                awaitingKey = true;
                currentKeyTarget = "ViewChangerKey";
            }
            currentY += controlHeight + 8f;

            Rect micToggleRect = new Rect(controlX, currentY, controlWidth, controlHeight);
            if (GUI.Button(micToggleRect, "Mic Toggle Key: " + GuiKeybinds.MicToggleKey.ToString(), label))
            {
                awaitingKey = true;
                currentKeyTarget = "MicToggleKey";
            }

            //currentY += controlHeight + 8f;

            //Rect rect2 = new Rect(controlX, currentY, controlWidth, controlHeight);
            //if (GUI.Button(rect2, "Nest Key: " + GuiKeybinds.CreateNestKey.ToString(), label))
            //{
            //    awaitingKey = true;
            //    currentKeyTarget = "CreateNestKey";
            //}

            //currentY += controlHeight + 8f;

            //Rect rect3 = new Rect(controlX, currentY, controlWidth, controlHeight);
            //if (GUI.Button(rect3, "DestroyNests Key: " + GuiKeybinds.DestroyNestKey.ToString(), label))
            //{
            //    awaitingKey = true;
            //    currentKeyTarget = "DestroyNestKey";
            //}

            //if (awaitingKey)
            //{
            //    GUI.Label(new Rect(controlX, currentY + 30f, controlWidth, controlHeight), "Press any key...", label);
            //}
        }

        private void DrawCreditsTab()
        {
            float tabWidth = 350f;
            float controlWidth = 300f;
            float controlHeight = 500f;
            float controlX = (tabWidth - controlWidth) / 2f;
            float currentY = 20f;

            Rect rect = new Rect(controlX, currentY, controlWidth, controlHeight);

            string creditText =
                "GUI Layout:\n" +
                "  - sylphie\n" +
                "  - qutr\n\n" +
                "Camera Function:\n" +
                "  - ollie\n" +
                "  - qutr\n\n";

            GUI.Label(rect, creditText, label);
        }

        private void DrawLeaderBoardTab()
        {
            float tabWidth = 350f;
            float controlWidth = 300f;
            float controlHeight = 20f;
            float controlX = (tabWidth - controlWidth) / 2f;
            float currentY = 30f;
            float spacing = 15f;

            Vector2 mousePosition = Event.current.mousePosition;

            Rect leaderboardToggle = new Rect(controlX, currentY, controlWidth, controlHeight);
            float scale = leaderboardToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            leaderboardToggle.width *= scale;
            leaderboardToggle.height *= scale;
            leaderboardToggle.x = (tabWidth - leaderboardToggle.width) / 2f;

            if (GUI.Button(leaderboardToggle, showLeaderboard ? "Hide Leaderboard <color=green>[ON]</color>" : "Show Leaderboard <color=red>[OFF]</color>", label))
                showLeaderboard = !showLeaderboard;

            currentY += controlHeight * scale + spacing;

            Rect styleToggle = new Rect(controlX, currentY, controlWidth, controlHeight);
            scale = styleToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            styleToggle.width *= scale;
            styleToggle.height *= scale;
            styleToggle.x = (tabWidth - styleToggle.width) / 2f;

            string styleText = LeaderBoard.CurrentStyle == LeaderBoard.LeaderboardStyle.Modern ?
                "<color=green>Modern Style</color>" : "<color=white>Classic Style</color>";

            if (GUI.Button(styleToggle, "Style: " + styleText, label))
                LeaderBoard.ToggleStyle();

            currentY += controlHeight * scale + spacing;

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "X Position: " + leaderboardX.ToString("F0"), label);
            currentY += 20f;
            float newLeaderboardX = eUI.RoundedSlider(leaderboardX, 0, Screen.width - 300, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newLeaderboardX != leaderboardX)
            {
                leaderboardX = newLeaderboardX;
                ApplyLeaderboardSettings();
            }
            currentY += 20f;

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Y Position: " + leaderboardY.ToString("F0"), label);
            currentY += 20f;
            float newLeaderboardY = eUI.RoundedSlider(leaderboardY, 0, Screen.height - 300, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newLeaderboardY != leaderboardY)
            {
                leaderboardY = newLeaderboardY;
                ApplyLeaderboardSettings();
            }
            currentY += 20f;

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Scale: " + leaderboardScale.ToString("F2"), label);
            currentY += 20f;
            float newLeaderboardScale = eUI.RoundedSlider(leaderboardScale, 0.5f, 2f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newLeaderboardScale != leaderboardScale)
            {
                leaderboardScale = newLeaderboardScale;
                ApplyLeaderboardSettings();
            }
            currentY += 20f;

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Opacity: " + leaderboardOpacity.ToString("F2"), label);
            currentY += 20f;
            float newLeaderboardOpacity = eUI.RoundedSlider(leaderboardOpacity, 0.1f, 1f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newLeaderboardOpacity != leaderboardOpacity)
            {
                leaderboardOpacity = newLeaderboardOpacity;
                ApplyLeaderboardSettings();
            }
            currentY += 20f;

            if (LeaderBoard.CurrentStyle == LeaderBoard.LeaderboardStyle.Modern)
            {
                GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Spacing: " + modernSpacing.ToString("F0"), label);
                currentY += 20f;
                float newModernSpacing = eUI.RoundedSlider(modernSpacing, 5f, 30f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
                if (newModernSpacing != modernSpacing)
                {
                    modernSpacing = newModernSpacing;
                    ApplyLeaderboardSettings();
                }
                currentY += 20f;
            }

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Color Box Size: " + colorBoxSize.ToString("F0"), label);
            currentY += 20f;
            float newColorBoxSize = eUI.RoundedSlider(colorBoxSize, 10f, 30f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newColorBoxSize != colorBoxSize)
            {
                colorBoxSize = newColorBoxSize;
                ApplyLeaderboardSettings();
            }
            currentY += 20f;

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Text Scale: " + textScale.ToString("F2"), label);
            currentY += 20f;
            float newTextScale = eUI.RoundedSlider(textScale, 0.5f, 2f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newTextScale != textScale)
            {
                textScale = newTextScale;
                ApplyLeaderboardSettings();
            }
        }

        private void DrawMiniMapTab()
        {
            float tabWidth = 350f;
            float controlWidth = 300f;
            float controlHeight = 20f;
            float controlX = (tabWidth - controlWidth) / 2f;
            float currentY = 30f;
            float spacing = 15f;

            Vector2 mousePosition = Event.current.mousePosition;

            Rect miniMapToggle = new Rect(controlX, currentY, controlWidth, controlHeight);
            float scale = miniMapToggle.Contains(mousePosition) ? (Event.current.type == EventType.MouseDown ? 0.9f : 1.1f) : 1f;
            miniMapToggle.width *= scale;
            miniMapToggle.height *= scale;
            miniMapToggle.x = (tabWidth - miniMapToggle.width) / 2f;

            if (GUI.Button(miniMapToggle, showMiniMap ? "MiniMap: <color=green>ON</color>" : "MiniMap: <color=red>OFF</color>", label))
            {
                showMiniMap = !showMiniMap;
                MiniMap.ShowMiniMap = showMiniMap;
                
                if (MiniMap.Instance == null)
                {
                    GameObject miniMapObj = new GameObject("MiniMapManager");
                    miniMapObj.AddComponent<MiniMap>();
                    DontDestroyOnLoad(miniMapObj);
                }
                
                MiniMap.Instance.UpdateMiniMapDisplay();
            }
            currentY += controlHeight * scale + spacing;

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Size: " + miniMapSize.ToString("F0"), label);
            currentY += 20f;
            float newMiniMapSize = eUI.RoundedSlider(miniMapSize, 100f, 400f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newMiniMapSize != miniMapSize)
            {
                miniMapSize = newMiniMapSize;
                MiniMap.MiniMapSize = miniMapSize;
                if (MiniMap.Instance != null)
                {
                    MiniMap.Instance.UpdateMiniMapDisplay();
                }
                else
                {
                    GameObject miniMapObj = new GameObject("MiniMapManager");
                    miniMapObj.AddComponent<MiniMap>();
                    DontDestroyOnLoad(miniMapObj);
                }
            }
            currentY += 20f;

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Zoom: " + miniMapZoom.ToString("F0"), label);
            currentY += 20f;
            float newMiniMapZoom = eUI.RoundedSlider(miniMapZoom, 5f, 100f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newMiniMapZoom != miniMapZoom)
            {
                miniMapZoom = newMiniMapZoom;
                MiniMap.MiniMapZoom = miniMapZoom;
                if (MiniMap.Instance != null)
                {
                    MiniMap.Instance.UpdateMiniMapDisplay();
                }
            }
            currentY += 20f;

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Opacity: " + miniMapOpacity.ToString("F2"), label);
            currentY += 20f;
            float newMiniMapOpacity = eUI.RoundedSlider(miniMapOpacity, 0.1f, 1f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newMiniMapOpacity != miniMapOpacity)
            {
                miniMapOpacity = newMiniMapOpacity;
                MiniMap.MiniMapOpacity = miniMapOpacity;
                if (MiniMap.Instance != null)
                {
                    MiniMap.Instance.UpdateMiniMapDisplay();
                }
            }
            currentY += 20f;

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "X Position: " + miniMapX.ToString("F0"), label);
            currentY += 20f;
            float newMiniMapX = eUI.RoundedSlider(miniMapX, 0f, Screen.width - 200f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newMiniMapX != miniMapX)
            {
                miniMapX = newMiniMapX;
                MiniMap.MiniMapPosition = new Vector2(miniMapX, miniMapY);
                if (MiniMap.Instance != null)
                {
                    MiniMap.Instance.UpdateMiniMapDisplay();
                }
            }
            currentY += 20f;

            GUI.Label(new Rect(controlX, currentY, controlWidth, 20f), "Y Position: " + miniMapY.ToString("F0"), label);
            currentY += 20f;
            float newMiniMapY = eUI.RoundedSlider(miniMapY, 0f, Screen.height - 200f, new Rect(controlX - 5, currentY, controlWidth, 12), sliderBackTex, sliderFillTex);
            if (newMiniMapY != miniMapY)
            {
                miniMapY = newMiniMapY;
                MiniMap.MiniMapPosition = new Vector2(miniMapX, miniMapY);
                if (MiniMap.Instance != null)
                {
                    MiniMap.Instance.UpdateMiniMapDisplay();
                }
            }
        }

        Texture2D MakeTex(Color col)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        public void ToggleMenu()
        {
            ShowMenu = !ShowMenu;
        }

        private void CycleViewMode()
        {
            // Cycle through: Third Person -> Front Third Person -> First Person -> Third Person
            viewMode = (viewMode + 1) % 3;
            CameraUpdater.FP = (viewMode == 2);
            CameraUpdater.TPFront = (viewMode == 1);
        }

        public static Texture2D LoadTexture(string path)
        {
            Texture2D texture2D = new Texture2D(2, 2);
            Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            if (manifestResourceStream != null)
            {
                byte[] array = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(array, 0, (int)manifestResourceStream.Length);
                texture2D.LoadImage(array);
                Debug.Log($"[UnknownCasting] Loaded texture: {path}");
            }
            else
            {
                Debug.LogWarning($"[UnknownCasting] Could not find texture: {path}");
            }
            return texture2D;
        }

        private Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();

        public void LoadFont(string bundlePath, ref Font targetFont)
        {
            try
            {
                AssetBundle bundle = GetOrLoadBundle(bundlePath);
                if (bundle == null) return;

                string[] assetNames = bundle.GetAllAssetNames();
                if (assetNames.Length == 0) return;

                Font loadedFont = bundle.LoadAsset<Font>(assetNames[0]);
                if (loadedFont != null) targetFont = loadedFont;
            }
            catch { }
        }

        public AssetBundle GetOrLoadBundle(string path)
        {
            if (_loadedBundles.TryGetValue(path, out AssetBundle cachedBundle)) return cachedBundle;

            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
                {
                    if (stream == null) return null;

                    AssetBundle bundle = AssetBundle.LoadFromStream(stream);
                    if (bundle != null)
                    {
                        _loadedBundles[path] = bundle;
                        return bundle;
                    }
                    return null;
                }
            }
            catch { return null; }
        }

        void OnEnable()
        {
            LoadSettings();
            ApplyLeaderboardSettings();
        }

        void OnDestroy()
        {
            foreach (var bundle in _loadedBundles.Values)
            {
                if (bundle != null) bundle.Unload(true);
            }
            _loadedBundles.Clear();
            SaveSettings();
        }

        void OnDisable()
        {
            SaveSettings();
        }

        public static Vector3 GetCameraPos()
        {
            return CameraUpdater.cam != null ? CameraUpdater.cam.transform.forward : Vector3.forward;
        }

        void SaveSettings()
        {
            // Nametag settings
            saved.nametags = nametags;
            saved.selftags = selftags;
            saved.talkingindictor = talkingindictor;
            saved.matchPlayerColor = NameTags.matchPlayerColor;
            saved.tagssize = tagssize;
            saved.tagsoffset = tagsoffset;

            // Outline settings
            saved.outlineWidth = outlineWidth;
            saved.outlineColor = outlineColor;
            saved.matchOutlineToPlayer = matchOutlineToPlayer;

            // Font settings
            saved.selectedFont = availableFonts[selectedFontIndex];

            // FPS tag settings
            saved.showFPS = showFPS;
            saved.fpsTagSize = NameTags.fpsTagSize;
            saved.fpsTagOffset = NameTags.fpsTagOffset;

            // Platform tag settings
            saved.showPlatforms = showPlatforms;
            saved.platformTagSize = NameTags.platformTagSize;
            saved.platformTagOffset = NameTags.platformTagOffset;

            // Speed tag settings
            saved.showSpeedTags = showSpeedTags;
            saved.speedTagSize = NameTags.speedTagSize;
            saved.speedTagOffset = NameTags.speedTagOffset;

            // Existing settings...
            saved.HandCam = HandCam;
            saved.ShowCameraObj = ShowCameraObj;
            saved.EnableSmoothing = EnableSmoothing;
            saved.WASD = WASD;
            saved.fly = UnknownCasting.Core.WASD.fly;
            saved.camX = CameraUpdater.x;
            saved.camY = CameraUpdater.y;
            saved.camZ = CameraUpdater.z;
             saved.camFOV = CameraUpdater.FOV;
             saved.camSmoothing = CameraUpdater.positionSmoothing;
             saved.camRotationSmoothing = CameraUpdater.rotationSmoothing;
             saved.camEnableSmoothing = CameraUpdater.enableSmoothing;
              saved.camSmoothingType = CameraUpdater.smoothingType;
              saved.rollLock = CameraUpdater.rollLock;
              saved.enableSelfRigSmoothing = CameraUpdater.enableSelfRigSmoothing;
              saved.selfRigLerpAmount = CameraUpdater.selfRigLerpAmount;
             saved.leaderboardStyle = (int)LeaderBoard.CurrentStyle;
            saved.hideRoomCode = hideRoomCode;
            saved.cursorEnabled = cursorEnabled;
            saved.micToggleEnabled = micToggleEnabled;
            saved.fpsUnlocked = fpsUnlocked;
            saved.fpsCap = fpsCap;
            saved.nearClip = nearClip;
            
            // Environment settings
            saved.timeOfDayIndex = timeOfDayIndex;
            saved.weatherIndex = weatherIndex;
            
            saved.lavaDistanceEnabled = lavaDistanceEnabled;
            saved.lavaWarningThreshold = lavaWarningThreshold;
            saved.lavaDangerThreshold = lavaDangerThreshold;
            saved.lavaTextSize = Distance.textSize;
            saved.lavaBackgroundOpacity = Distance.backgroundOpacity;
            saved.lavaTextOpacity = Distance.textOpacity;
            saved.antiCheatSelectedTab = antiCheatSelectedTab;
            saved.leaderboardX = leaderboardX;
            saved.leaderboardY = leaderboardY;
            saved.leaderboardScale = leaderboardScale;
            saved.leaderboardOpacity = leaderboardOpacity;
            saved.modernSpacing = modernSpacing;
            saved.colorBoxSize = colorBoxSize;
            saved.textScale = textScale;
            saved.showMiniMap = showMiniMap;
            saved.miniMapSize = miniMapSize;
            saved.miniMapZoom = miniMapZoom;
            saved.miniMapOpacity = miniMapOpacity;
            saved.miniMapX = miniMapX;
            saved.miniMapY = miniMapY;

            File.WriteAllText(configPath, JsonUtility.ToJson(saved, true));
        }

        void LoadSettings()
        {
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                saved = JsonUtility.FromJson<Settings>(json);

                // Nametag settings
                nametags = saved.nametags;
                selftags = saved.selftags;
                talkingindictor = saved.talkingindictor;
                NameTags.matchPlayerColor = saved.matchPlayerColor;
                tagssize = saved.tagssize;
                tagsoffset = saved.tagsoffset;

                // Outline settings
                outlineWidth = saved.outlineWidth;
                outlineColor = saved.outlineColor;
                matchOutlineToPlayer = saved.matchOutlineToPlayer;
                NameTags.outlineWidth = outlineWidth;
                NameTags.outlineColor = outlineColor;

                // Font settings
                if (!string.IsNullOrEmpty(saved.selectedFont))
                {
                    int fontIndex = Array.IndexOf(availableFonts, saved.selectedFont);
                    if (fontIndex >= 0)
                    {
                        selectedFontIndex = fontIndex;
                        NameTags.SetFont(saved.selectedFont);
                    }
                }

                // FPS tag settings
                showFPS = saved.showFPS;
                NameTags.fpsTagSize = saved.fpsTagSize;
                NameTags.fpsTagOffset = saved.fpsTagOffset;

                // Platform tag settings
                showPlatforms = saved.showPlatforms;
                NameTags.platformTagSize = saved.platformTagSize;
                NameTags.platformTagOffset = saved.platformTagOffset;

                // Speed tag settings
                showSpeedTags = saved.showSpeedTags;
                NameTags.speedTagSize = saved.speedTagSize;
                NameTags.speedTagOffset = saved.speedTagOffset;

                // Existing settings...
                HandCam = saved.HandCam;
                ShowCameraObj = saved.ShowCameraObj;
                EnableSmoothing = saved.EnableSmoothing;
                WASD = saved.WASD;
                UnknownCasting.Core.WASD.fly = saved.fly;
                hideRoomCode = saved.hideRoomCode;
                cursorEnabled = saved.cursorEnabled;
                micToggleEnabled = saved.micToggleEnabled;
                fpsUnlocked = saved.fpsUnlocked;
                fpsCap = saved.fpsCap;
                FpsUnLock.SetFPSUnlock(fpsUnlocked);
                nearClip = saved.nearClip;
                CameraUpdater.nearClip = nearClip;
                
                // Environment settings
                timeOfDayIndex = saved.timeOfDayIndex;
                weatherIndex = saved.weatherIndex;
                
                lavaDistanceEnabled = saved.lavaDistanceEnabled;
                lavaWarningThreshold = saved.lavaWarningThreshold;
                lavaDangerThreshold = saved.lavaDangerThreshold;
                Distance.textSize = saved.lavaTextSize;
                Distance.backgroundOpacity = saved.lavaBackgroundOpacity;
                Distance.textOpacity = saved.lavaTextOpacity;

                if (UnknownCasting.Core.Cursor.CursorMod.Instance != null)
                {
                    UnknownCasting.Core.Cursor.CursorMod.Instance.cursorEnabled = cursorEnabled;
                    if (!cursorEnabled)
                    {
                        UnknownCasting.Core.Cursor.CursorMod.Instance.DestroyPointer();
                    }
                }

                if (UnknownCasting.Core.MicToggle.MicToggle.Instance != null)
                {
                    // Only toggle if we want to enable it and it's currently disabled
                    // Or if we want to disable it and it's currently enabled
                    if (micToggleEnabled && !UnknownCasting.Core.MicToggle.MicToggle.Instance.micEnabled)
                    {
                        UnknownCasting.Core.MicToggle.MicToggle.Instance.ToggleMicrophone();
                    }
                    else if (!micToggleEnabled && UnknownCasting.Core.MicToggle.MicToggle.Instance.micEnabled)
                    {
                        UnknownCasting.Core.MicToggle.MicToggle.Instance.ToggleMicrophone();
                    }
                }

                LeaderBoard.CurrentStyle = (LeaderBoard.LeaderboardStyle)saved.leaderboardStyle;
                antiCheatSelectedTab = saved.antiCheatSelectedTab;
                leaderboardX = saved.leaderboardX;
                leaderboardY = saved.leaderboardY;
                leaderboardScale = saved.leaderboardScale;
                leaderboardOpacity = saved.leaderboardOpacity;
                modernSpacing = saved.modernSpacing;
                colorBoxSize = saved.colorBoxSize;
                textScale = saved.textScale;
                showMiniMap = saved.showMiniMap;
                miniMapSize = saved.miniMapSize;
                miniMapZoom = saved.miniMapZoom;
                miniMapOpacity = saved.miniMapOpacity;
                miniMapX = saved.miniMapX;
                miniMapY = saved.miniMapY;
                MiniMap.ShowMiniMap = showMiniMap;
                MiniMap.MiniMapSize = miniMapSize;
                MiniMap.MiniMapZoom = miniMapZoom;
                MiniMap.MiniMapOpacity = miniMapOpacity;
                MiniMap.MiniMapPosition = new Vector2(miniMapX, miniMapY);
            }
        }

        private string configPath => Path.Combine(Paths.ConfigPath, "UnknownCastingConfig.banana");

        private void SetTimeOfDay(int index)
        {
            try
            {
                BetterDayNightManager bdn = BetterDayNightManager.instance;
                if (bdn == null) return;
                
                switch (index)
                {
                    case 0: // Normal - reset to game default
                        bdn.SetTimeOfDay(3);
                        break;
                    case 1: // Day
                        bdn.SetTimeOfDay(3);
                        break;
                    case 2: // Night
                        bdn.SetTimeOfDay(0);
                        break;
                }
            }
            catch { }
        }

        private void SetWeather(int index)
        {
            try
            {
                BetterDayNightManager bdn = BetterDayNightManager.instance;
                if (bdn == null || bdn.weatherCycle == null) return;
                
                switch (index)
                {
                    case 0: // Normal - reset to game default
                        bdn.SetTimeOfDay(3);
                        break;
                    case 1: // Clear
                        for (int i = 1; i < bdn.weatherCycle.Length; i++)
                            bdn.weatherCycle[i] = (BetterDayNightManager.WeatherType) 0;
                        break;
                    case 2: // Rainy
                        for (int i = 1; i < bdn.weatherCycle.Length; i++)
                            bdn.weatherCycle[i] = (BetterDayNightManager.WeatherType) 1;
                        break;
                }
            }
            catch { }
        }

        private void ApplyTimeChanger(int index)
        {
            try
            {
                BetterDayNightManager bdn = BetterDayNightManager.instance;
                if (bdn == null) return;
                
                switch (index)
                {
                    case 0: // Normal
                        timeOfDayIndex = 0;
                        weatherIndex = 0;
                        bdn.SetTimeOfDay(3);
                        for (int i = 1; i < bdn.weatherCycle.Length; i++)
                            bdn.weatherCycle[i] = (BetterDayNightManager.WeatherType) 0;
                        break;
                    case 1: // Make Time Day
                        timeOfDayIndex = 1;
                        bdn.SetTimeOfDay(3);
                        break;
                    case 2: // Make Time Night
                        timeOfDayIndex = 2;
                        bdn.SetTimeOfDay(0);
                        break;
                    case 3: // Clear Weather
                        weatherIndex = 1;
                        for (int i = 1; i < bdn.weatherCycle.Length; i++)
                            bdn.weatherCycle[i] = (BetterDayNightManager.WeatherType) 0;
                        break;
                    case 4: // Rainy Weather
                        weatherIndex = 2;
                        for (int i = 1; i < bdn.weatherCycle.Length; i++)
                            bdn.weatherCycle[i] = (BetterDayNightManager.WeatherType) 1;
                        break;
                }
            }
            catch { }
        }

        [Serializable]
        public class Settings
        {
            // Nametag settings
            public bool nametags = false;
            public bool selftags = false;
            public bool talkingindictor = false;
            public bool matchPlayerColor = false;
            public float tagssize = 1;
            public float tagsoffset = 0.3f;

            // Outline settings
            public float outlineWidth = 0.07f;
            public Color outlineColor = Color.black;
            public bool matchOutlineToPlayer = false;

            // Font settings
            public string selectedFont = "Pixel";

            // FPS tag settings
            public bool showFPS = false;
            public float fpsTagSize = 0.8f;
            public float fpsTagOffset = 0.2f;

            // Platform tag settings
            public bool showPlatforms = false;
            public float platformTagSize = 0.8f;
            public float platformTagOffset = 0.2f;

            // Speed tag settings
            public bool showSpeedTags = false;
            public float speedTagSize = 0.8f;
            public float speedTagOffset = 0.4f;

            // Existing settings...
            public bool HandCam;
            public bool ShowCameraObj;
            public bool EnableSmoothing = true;
            public bool WASD = false;
            public bool fly = false;
            public float camX = 0f;
            public float camY = 0.6f;
            public float camZ = 2f;
            public float camFOV = 110f;
            public float camSmoothing = 0.2f;
            public bool camEnableSmoothing = true;
            public int camSmoothingType = 1;
            public float camRotationSmoothing = 0.5f;
            public bool rollLock = false;
            public bool enableSelfRigSmoothing = false;
            public float selfRigLerpAmount = 0f;
            public float LerpValue = 20f;
            public int leaderboardStyle = 0;
            public int antiCheatSelectedTab = 0;
            public bool hideRoomCode = false;
            public bool cursorEnabled = false;
            public bool micToggleEnabled = false;
            public float nearClip = 0.03f;
            public bool lavaDistanceEnabled = true;
            public float lavaWarningThreshold = 10f;
            public float lavaDangerThreshold = 5f;
            public float lavaDisplayX = 20f;
            public float lavaDisplayY = 20f;
            public float lavaDisplayWidth = 300f;
            public float lavaDisplayHeight = 80f;
            public float lavaTextSize = 24f;
            public float lavaBackgroundOpacity = 0.8f;
            public float lavaTextOpacity = 1f;
            public float leaderboardX = 22f;
            public float leaderboardY = 692f;
            public float leaderboardScale = 1.13f;
            public float leaderboardOpacity = 0.54f;
            public float modernSpacing = 8f;
            public float colorBoxSize = 14f;
            public float textScale = 1.10f;
            public bool showMiniMap = false;
            public float miniMapSize = 200f;
            public float miniMapZoom = 10f;
            public float miniMapOpacity = 1f;
            public float miniMapX = 808f;
            public float miniMapY = 387f;
            public bool fpsUnlocked = false;
            public float fpsCap = 144f;
            
            // Environment settings
            public int timeOfDayIndex = 0;
            public int weatherIndex = 0;
        }

        private void OnApplicationQuit()
        {
            SaveSettings();
        }

        private Settings saved = new Settings();
    }
}
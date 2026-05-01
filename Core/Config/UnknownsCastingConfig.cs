using System;
using System.IO;
using UnityEngine;

namespace UnknownCasting.Core.Config
{
    [Serializable]
    public class UnknownCastingConfig
    {
        public int version = 1;

        // General
        public bool nametags = true;
        public bool selftags = false;
        public bool talkingIndicator = false;
        public bool hideRoomCode = false;
        public bool cursorEnabled = false;
        public bool micToggleEnabled = false;
        public bool fpsUnlocked = false;

        // Camera Settings
        public float camX = 0f;
        public float camY = 0.6f;
        public float camZ = 2f;
        public float camFOV = 110f;
        public float camSmoothing = 0.2f;
        public bool camEnableSmoothing = true;
        public int camSmoothingType = 1;
        public float nearClip = 0.03f;
        public bool handCam = false;
        public bool showCameraObj = false;
        public bool enableSmoothing = true;
        public bool wasdEnabled = false;

        // Name Tags - General
        public float tagSize = 1f;
        public float tagOffset = 0.3f;
        public bool matchPlayerColor = true;
        public bool showName = true;
        public string selectedFont = "Pixel";

        // Name Tags - Outline
        public float outlineWidth = 0.07f;
        public Color outlineColor = Color.black;
        public bool matchOutlineToPlayer = false;

        // Name Tags - FPS
        public bool showFPS = true;
        public float fpsTagSize = 0.8f;
        public float fpsTagOffset = 0.2f;
        public bool matchFPSColor = true;
        public Color customFPSColor = Color.white;
        public Color fpsGreen = Color.green;
        public Color fpsYellow = Color.yellow;
        public Color fpsRed = Color.red;
        public bool useFPSIndicator = true;

        // Name Tags - Platform
        public bool showPlatform = true;
        public float platformTagSize = 0.8f;
        public float platformTagOffset = 0.2f;
        public bool matchPlatformColor = true;
        public Color customPlatformColor = Color.white;

        // Name Tags - Speed
        public bool showSpeed = true;
        public float speedTagSize = 0.8f;
        public float speedTagOffset = 0.4f;
        public bool matchSpeedColor = true;
        public Color customSpeedColor = Color.white;

        // Custom Colors
        public Color customNameColor = Color.white;

        // Leaderboard
        public int leaderboardStyle = 0;
        public float leaderboardX = 22f;
        public float leaderboardY = 692f;
        public float leaderboardScale = 1.13f;
        public float leaderboardOpacity = 0.54f;
        public float modernSpacing = 8f;
        public float colorBoxSize = 14f;
        public float textScale = 1.10f;
        public float imageExposure = 1f;
        public bool rankNumberInFront = true;
        public int antiCheatSelectedTab = 0;

        // Lava Distance
        public bool lavaDistanceEnabled = true;
        public float lavaWarningThreshold = 10f;
        public float lavaDangerThreshold = 5f;
        public float lavaTextSize = 24f;
        public float lavaBackgroundOpacity = 0.8f;
        public float lavaTextOpacity = 1f;

        // MiniMap
        public bool showMiniMap = false;
        public float miniMapSize = 200f;
        public float miniMapZoom = 10f;
        public float miniMapOpacity = 1f;
        public float miniMapX = 808f;
        public float miniMapY = 387f;

        public void Save(string path)
        {
            string json = JsonUtility.ToJson(this, true);
            File.WriteAllText(path, json);
        }

        public static UnknownCastingConfig Load(string path)
        {
            if (!File.Exists(path))
                return new UnknownCastingConfig();

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<UnknownCastingConfig>(json);
            }
            catch
            {
                return new UnknownCastingConfig();
            }
        }
    }

    public static class ConfigManager
    {
        private static UnknownCastingConfig _config;
        private static string _configPath;

        public static UnknownCastingConfig Config
        {
            get
            {
                if (_config == null)
                    Load();
                return _config;
            }
        }

        public static void Initialize(string configPath)
        {
            _configPath = configPath;
            Load();
        }

        public static void Load()
        {
            _config = UnknownCastingConfig.Load(_configPath);
        }

        public static void Save()
        {
            _config?.Save(_configPath);
        }

        public static string ConfigPath => _configPath;
    }
}
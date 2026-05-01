using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnknownCasting.Core.PlatformChecker
{
    [BepInPlugin("platform.checker", "platform checker", "1.0.0")]
    public class PlatformChecker
     {
        private bool showGUI = false;
        private Vector2 scrollPos;
        public static Dictionary<string, PlayerRankData> rankCache = new Dictionary<string, PlayerRankData>();
        public static float lastRequestTime;
        private const float requestCooldown = 10f;
        private Texture2D blackTex;
        private Texture2D whiteTex;
        private Texture2D yellowTex;
        private Texture2D grayTex;
        private Texture2D PlatformCheckerPanel;
        Texture2D MakeTex(Color col)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        Texture2D LoadTextureFromEmbeddedResource(string resourceName)
        { 
            var assembly = Assembly.GetExecutingAssembly(); 
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Debug.LogError($"Failed to load embedded resource: {resourceName}");
                    return null;
                } 
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length); 
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                return texture;
            }
        }
        void Start()
        {
            PlatformCheckerPanel = LoadTextureFromEmbeddedResource("PlatformCheckerPanel.png");
            blackTex = MakeTex(new Color(0.188f, 0.188f, 0.188f));
            whiteTex = MakeTex(Color.white);
            yellowTex = MakeTex(new Color(1f, 0.902f, 0f));
            grayTex = MakeTex(new Color(0.251f, 0.251f, 0.251f));
        }
        void Awake() => Harmony.CreateAndPatchAll(typeof(RankPatches));
        void Update()
        {
            if (Time.time - lastRequestTime > requestCooldown && PhotonNetwork.InRoom)
            {
                RefreshAllRanks();
                lastRequestTime = Time.time;
            }

            showGUI = Dev.Plugin.showPlatforms;
        }

        void OnGUI()
        {
            if (!showGUI || !PhotonNetwork.InRoom) return;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                font = Dev.Plugin.silkScreen,
                fontSize = 20, 
                alignment = TextAnchor.MiddleCenter,
            };
            Rect rect = new Rect(1800, 20, 250, 400);
            GUI.DrawTexture(rect, PlatformCheckerPanel, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(rect.x + 30, rect.y + 25, rect.width - 30, 20), "PLATFORMS", style);
            GUI.BeginGroup(rect);
            {
                rect.y += 50;
                rect.width -= 10;
                rect.x += 10;
                rect.height += 10;
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    int i = 1;
                    CreatePlayerCard(new Rect(rect.x, rect.y, rect.width, 45), player, i);
                    i++;
                }
            }
            GUI.EndGroup();
        }

        void CreatePlayerCard(Rect rect, Player player, int PlayersRank)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                font = Dev.Plugin.silkScreen,
                fontSize = 10, 
                alignment = TextAnchor.MiddleCenter,
            };
            if (!rankCache.TryGetValue(player.UserId, out PlayerRankData data)) return;
            var rig = GetVRRigFromPlayer(player);
            eUI.BorderBox(rect.width, rect.height, blackTex, yellowTex, 10, 1);
            {
                rect.y += 10; 
                GUI.Label(rect, PlayersRank +". " + rig.playerText1.text.ToUpper() + "  " + Platform(data), style);
            }
        }
        
        string Platform(PlayerRankData data)
        {
            if (data.PCScore > 100 && data.QuestScore > 100) return "[pc]";
            if (data.PCScore > 100) return "[steam]";
            if (data.QuestScore > 100) return "[quest]";
            return "null";
        }

        void RefreshAllRanks()
        {
            if (NetworkSystem.Instance == null) return;

            Traverse.Create(typeof(RankedProgressionManager))
                .Method("AcquireLocalPlayerRankInformation")
                .GetValue();

            foreach (var player in PhotonNetwork.PlayerListOthers)
            {
                var netPlayer = NetPlayerToPlayer(player);
                if (netPlayer != null)
                {
                    Traverse.Create(typeof(RankedProgressionManager))
                        .Method("AcquireSinglePlayerRankInformation", netPlayer)
                        .GetValue();
                }
            }
        }
        
        public static Player NetPlayerToPlayer(NetPlayer p) =>
            p.GetPlayerRef();
        public static VRRig GetVRRigFromPlayer(NetPlayer p) =>
            GorillaGameManager.instance.FindPlayerVRRig(p);
    }
    
    public class PlayerRankData
    {
        public string PCRank { get; set; }
        public float PCScore { get; set; }
        public string QuestRank { get; set; }
        public float QuestScore { get; set; }
    }
    
    [HarmonyPatch(typeof(RankedProgressionManager))]
    public class RankPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnLocalPlayerRankedInformationAcquired")]
        static void CacheLocalPlayerRank(GorillaTagCompetitiveServerApi.RankedModeProgressionData __0)
        {
            if (__0?.playerData == null || __0.playerData.Count == 0) return;
            CacheRankData(NetworkSystem.Instance.LocalPlayer.UserId, __0.playerData[0]);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnPlayersRankedInformationAcquired")]
        static void CachePlayerRanks(GorillaTagCompetitiveServerApi.RankedModeProgressionData __0)
        {
            if (__0?.playerData == null) return;
            foreach (var playerData in __0.playerData) CacheRankData(playerData.playfabID, playerData);
        }

        static void CacheRankData(string playerId, GorillaTagCompetitiveServerApi.RankedModePlayerProgressionData data)
        {
            if (data?.platformData == null || data.platformData.Length < 2) return;
            PlatformChecker.rankCache[playerId] = new PlayerRankData
            {
                PCRank = GetRankName(data.platformData[0].majorTier, data.platformData[0].minorTier),
                PCScore = data.platformData[0].elo,
                QuestRank = GetRankName(data.platformData[1].majorTier, data.platformData[1].minorTier),
                QuestScore = data.platformData[1].elo
            };
        }

        static string GetRankName(int majorTier, int minorTier)
        {
            try { return RankedProgressionManager.Instance.MajorTiers[majorTier].subTiers[minorTier].name; }
            catch { return $"Tier {majorTier}-{minorTier}"; }
        }
    }
}